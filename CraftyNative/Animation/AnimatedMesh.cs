using CraftyNative.ThreeD.Meshes;
using System.Numerics;
using System.Runtime.InteropServices;

namespace CraftyNative.Animation;

public sealed class AnimatedMesh
{
    public Mesh Mesh { get; }

    private readonly float[] _bindPose;

    private readonly Dictionary<string, RigidPart> _parts = [];

    public AnimatedMesh(Mesh mesh)
    {
        Mesh = mesh;
        _bindPose = [.. mesh.Vertices];
    }

    public void AddPart(string name, Vector3 pivot, int vertexStart, int vertexCount)
    {
        _parts[name] = new RigidPart(pivot, vertexStart, vertexCount);
    }

    public ref RigidPart GetPart(string name)
    {
        return ref CollectionsMarshal.GetValueRefOrNullRef(_parts, name);
    }

    public bool TryGetPart(string name, out RigidPart part)
    {
        return _parts.TryGetValue(name, out part);
    }

    public void Reset()
    {
        _bindPose.AsSpan().CopyTo(Mesh.Vertices.AsSpan());
    }

    public void Apply()
    {
        Reset();

        foreach (var part in _parts.Values)
            ApplyPart(part);

        Mesh.MarkDirty();
    }

    public void ResetPose()
    {
        foreach (var name in _parts.Keys)
        {
            ref var part = ref CollectionsMarshal.GetValueRefOrNullRef(_parts, name);
            part.Rotation = Quaternion.Identity;
        }
    }

    private void ApplyPart(in RigidPart part)
    {
        var vertices = Mesh.Vertices;

        int end = part.VertexStart + part.VertexCount;

        for (int vertex = part.VertexStart; vertex < end; vertex++)
        {
            int index = vertex * 5;

            var position = new Vector3(vertices[index], vertices[index + 1], vertices[index + 2]);

            position -= part.Pivot;

            position = Vector3.Transform(position, part.Rotation);

            position += part.Pivot;

            vertices[index] = position.X;
            vertices[index + 1] = position.Y;
            vertices[index + 2] = position.Z;
        }
    }
}