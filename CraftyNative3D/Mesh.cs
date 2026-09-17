namespace CraftyNative3D;

public sealed class Mesh(float[] vertices, uint[] indices, uint vertexStride = 12)
{
    public float[] Vertices { get; } = vertices;
    public uint[] Indices { get; } = indices;

    public uint VertexStride { get; } = vertexStride;
}