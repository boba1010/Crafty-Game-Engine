using CraftyNative.ThreeD.Meshes;
using System.Numerics;

namespace CraftyNative.ThreeD;

public static class ChunkBatch
{
    public static List<float> Vertices { get; } = [];
    public static List<uint> Indices { get; } = [];

    public static void Add(Mesh mesh, Vector3 position)
    {
        uint vertexOffset = (uint)(mesh.Vertices.Count / 5);

        for (int i = 0; i < mesh.Vertices.Count; i += 5)
        {
            Vertices.Add(mesh.Vertices[i] + position.X);
            Vertices.Add(mesh.Vertices[i + 1]);
            Vertices.Add(mesh.Vertices[i + 2] + position.Z);
            Vertices.Add(mesh.Vertices[i + 3]);
            Vertices.Add(mesh.Vertices[i + 4]);
        }

        foreach (uint index in mesh.Indices)
            Indices.Add(index + vertexOffset);
    }

    public static Mesh Build()
    {
        return new(Vertices, Indices, vertexStride: 20);
    }
}
