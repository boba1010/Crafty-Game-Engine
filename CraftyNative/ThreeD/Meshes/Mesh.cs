namespace CraftyNative.ThreeD.Meshes;

public sealed class Mesh(List<float> vertices, List<uint> indices, Material material = null!, uint vertexStride = 12) : IDisposable
{
    public List<float> Vertices { get; } = vertices;
    public List<uint> Indices { get; } = indices;
    public uint VertexStride { get; } = vertexStride;
    public Material Material { get; set; } = material;

    public void Dispose()
    {
        Material?.Dispose();
    }
}