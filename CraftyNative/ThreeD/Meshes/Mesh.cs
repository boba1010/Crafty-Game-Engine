namespace CraftyNative.ThreeD.Meshes;

public sealed class Mesh(List<float> vertices, List<uint> indices, ImageData? texture = null, uint vertexStride = 12)
{
    public List<float> Vertices { get; } = vertices;
    public List<uint> Indices { get; } = indices;
    public uint VertexStride { get; } = vertexStride;
    public ImageData? Texture { get; set; } = texture;
}