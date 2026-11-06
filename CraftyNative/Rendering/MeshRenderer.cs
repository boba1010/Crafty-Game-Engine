using CraftyNative.ThreeD.Meshes;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.Rendering;

public sealed class MeshRenderer(IGraphicsDevice device, ICommandBuffer commandBuffer, RenderResources resources) : IDisposable
{
    private readonly Dictionary<Mesh, GPUMesh> _gpuMeshes = [];

    public void Draw(Mesh mesh, Matrix4x4 mvp, bool applyWorldLighting = true, Vector4? clip = null)
    {
        var gpuMesh = GetOrCreateMesh(mesh);

        if (mesh.IsDirty)
        {
            EnsureBufferCapacity(gpuMesh, mesh);

            UpdateVertexBuffer(gpuMesh, mesh);
            UpdateIndexBuffer(gpuMesh, mesh);

            mesh.ClearDirty();
        }

        Span<byte> data = stackalloc byte[96];
        data.Clear();

        MemoryMarshal.Write(data, in mvp);

        float daylight = applyWorldLighting ? CraftyNative.Daylight : 1f;

        MemoryMarshal.Write(data[64..], in daylight);

        float moonlight = applyWorldLighting ? CraftyNative.Moonlight : 0f;

        MemoryMarshal.Write(data[68..], in moonlight);

        Vector4 clipRect = clip ?? new Vector4(0, 0, 100000f, 100000f);

        MemoryMarshal.Write(data[80..], in clipRect);

        resources.ConstantBuffer.Upload(data);

        commandBuffer.SetVertexBuffer(gpuMesh.VertexBuffer, mesh.VertexStride);

        commandBuffer.SetIndexBuffer(gpuMesh.IndexBuffer);
        commandBuffer.SetUniformBuffer(resources.ConstantBuffer);
        commandBuffer.SetSampler(resources.Sampler);

        if (mesh.Texture is { } texture)
        {
            var material = NativeMaterialsManager.Get(texture);
            commandBuffer.SetTexture(material.Atlas.Resource);
        }

        commandBuffer.DrawIndexed((uint)mesh.Indices.Count);
    }

    public void Release(Mesh? mesh)
    {
        if (mesh is null)
            return;

        if (_gpuMeshes.Remove(mesh, out var gpuMesh))
            gpuMesh.Dispose();
    }

    private GPUMesh GetOrCreateMesh(Mesh mesh)
    {
        if (_gpuMeshes.TryGetValue(mesh, out var gpuMesh))
            return gpuMesh;

        var vertexData = MemoryMarshal.AsBytes(mesh.Vertices.AsSpan());
        var indexData = MemoryMarshal.AsBytes(mesh.Indices.AsSpan());

        ulong vertexCapacity = GetBufferCapacity((ulong)vertexData.Length);
        ulong indexCapacity = GetBufferCapacity((ulong)indexData.Length);

        var vertexBuffer = CreateVertexBuffer(vertexCapacity);
        var indexBuffer = CreateIndexBuffer(indexCapacity);

        if (!vertexData.IsEmpty)
            vertexBuffer.Upload(vertexData);

        if (!indexData.IsEmpty)
            indexBuffer.Upload(indexData);

        gpuMesh = new GPUMesh(vertexBuffer, indexBuffer, vertexCapacity, indexCapacity);
        _gpuMeshes.Add(mesh, gpuMesh);

        return gpuMesh;
    }

    private void EnsureBufferCapacity(GPUMesh gpu, Mesh mesh)
    {
        ulong vertexSize = (ulong)mesh.Vertices.Count * sizeof(float);

        ulong indexSize = (ulong)mesh.Indices.Count * sizeof(uint);

        if (vertexSize > gpu.VertexCapacity)
        {
            ulong capacity = GetBufferCapacity(vertexSize);
            gpu.ResizeVertexBuffer(CreateVertexBuffer(capacity), capacity);
        }

        if (indexSize > gpu.IndexCapacity)
        {
            ulong capacity = GetBufferCapacity(indexSize);
            gpu.ResizeIndexBuffer(CreateIndexBuffer(capacity), capacity);
        }
    }

    private static ulong GetBufferCapacity(ulong required)
    {
        ulong capacity = 4096;

        while (capacity < required)
            capacity = checked(capacity * 2);

        return capacity;
    }

    private IBuffer CreateVertexBuffer(ulong size)
    {
        return device.CreateBuffer(new()
        {
            Size = size,
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });
    }

    private IBuffer CreateIndexBuffer(ulong size)
    {
        return device.CreateBuffer(new()
        {
            Size = size,
            Usage = BufferUsage.Index,
            MemoryUsage = MemoryUsage.Upload
        });
    }

    private static void UpdateVertexBuffer(GPUMesh gpu, Mesh mesh)
    {
        var data = MemoryMarshal.AsBytes(mesh.Vertices.AsSpan());

        if (!data.IsEmpty)
            gpu.VertexBuffer.Upload(data);
    }

    private static void UpdateIndexBuffer(GPUMesh gpu, Mesh mesh)
    {
        var data = MemoryMarshal.AsBytes(mesh.Indices.AsSpan());

        if (!data.IsEmpty)
            gpu.IndexBuffer.Upload(data);
    }

    public void Dispose()
    {
        foreach (var gpuMesh in _gpuMeshes.Values)
            gpuMesh.Dispose();

        _gpuMeshes.Clear();
    }

    private sealed class GPUMesh(IBuffer vertexBuffer, IBuffer indexBuffer, ulong vertexCapacity, ulong indexCapacity) : IDisposable
    {
        public IBuffer VertexBuffer { get; private set; } = vertexBuffer;
        public IBuffer IndexBuffer { get; private set; } = indexBuffer;

        public ulong VertexCapacity { get; private set; } = vertexCapacity;
        public ulong IndexCapacity { get; private set; } = indexCapacity;

        public void ResizeVertexBuffer(IBuffer buffer, ulong capacity)
        {
            VertexBuffer.Dispose();

            VertexBuffer = buffer;
            VertexCapacity = capacity;
        }

        public void ResizeIndexBuffer(IBuffer buffer, ulong capacity)
        {
            IndexBuffer.Dispose();

            IndexBuffer = buffer;
            IndexCapacity = capacity;
        }

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
