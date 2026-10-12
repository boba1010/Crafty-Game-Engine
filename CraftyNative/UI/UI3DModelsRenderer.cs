using CraftyNative.ThreeD.Meshes;
using System.Drawing;
using System.Numerics;
using Vulcan.Graphics;

namespace CraftyNative.UI;

public static class UI3DModelsRenderer
{
    private readonly record struct MeshDrawCommand(Mesh Mesh, Vector2 Position, Vector3 Rotation, Vector3 Scale, Vector2 Size);

    private static readonly List<MeshDrawCommand> _meshes = [];
    private static readonly Dictionary<Mesh, (Vector3 Center, float Scale)> _bounds = [];

    public static void DrawMesh(Mesh mesh, Vector2 position, Vector3 rotation, Vector3 scale, Vector2 size)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        if (size.X <= 0f || size.Y <= 0f || mesh.Vertices.Count == 0 || mesh.Indices.Count == 0)
            return;

        _meshes.Add(new(mesh, position, rotation, scale, size));
    }

    private static (Vector3 Center, float Scale) GetBounds(Mesh mesh)
    {
        if (_bounds.TryGetValue(mesh, out var cached))
            return cached;

        int stride = Math.Max(3, (int)mesh.VertexStride / sizeof(float));

        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (int i = 0; i + 2 < mesh.Vertices.Count; i += stride)
        {
            var position = new Vector3(mesh.Vertices[i], mesh.Vertices[i + 1], mesh.Vertices[i + 2]);

            min = Vector3.Min(min, position);
            max = Vector3.Max(max, position);
        }

        var extent = max - min;
        float largestExtent = MathF.Max(extent.X, MathF.Max(extent.Y, extent.Z));

        var bounds = ((min + max) * 0.5f, largestExtent > 0f ? 1f / largestExtent : 1f);

        _bounds[mesh] = bounds;
        return bounds;
    }

    private static Matrix4x4 BuildMvp(Mesh mesh, Vector2 size, Vector3 rotation, Vector3 drawScale)
    {
        var (center, scale) = GetBounds(mesh);

        var model =
            Matrix4x4.CreateTranslation(-center) *
            Matrix4x4.CreateScale(scale * drawScale) *
            Matrix4x4.CreateRotationY(rotation.Y) *
            Matrix4x4.CreateRotationX(rotation.X) *
            Matrix4x4.CreateRotationZ(rotation.Z) *
            Matrix4x4.CreateTranslation(0f, 0f, -5f);

        float aspect = size.X / size.Y;
        var projection = Matrix4x4.CreateOrthographic(1.8f * aspect, 1.8f, 0.1f, 10f);

        return model * projection;
    }

    public static void Render(ICommandBuffer commandBuffer, Vector2 screenSize)
    {
        if (_meshes.Count == 0)
            return;

        bool depthCleared = false;

        foreach (var draw in _meshes)
        {
            if (!depthCleared)
            {
                commandBuffer.ClearDepth(1f);
                depthCleared = true;
            }

            var center = draw.Position + draw.Size * 0.5f;

            commandBuffer.SetViewport(new Viewport
            {
                X = center.X - draw.Size.X * 0.5f,
                Y = center.Y - draw.Size.Y * 0.5f,
                Width = draw.Size.X,
                Height = draw.Size.Y,
                MinDepth = 0f,
                MaxDepth = 1f
            });

            CraftyNative.DrawHudMesh(draw.Mesh, BuildMvp(draw.Mesh, draw.Size, draw.Rotation, draw.Scale));
        }

        commandBuffer.SetViewport(new Viewport
        {
            X = 0,
            Y = 0,
            Width = screenSize.X,
            Height = screenSize.Y,
            MinDepth = 0f,
            MaxDepth = 1f
        });

        _meshes.Clear();
    }

    public static void Dispose()
    {
        _meshes.Clear();
        _bounds.Clear();
    }
}
