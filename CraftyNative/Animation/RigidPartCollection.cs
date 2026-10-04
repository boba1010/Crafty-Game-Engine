using System.Numerics;
using System.Runtime.InteropServices;

namespace CraftyNative.Animation;

public sealed class RigidPartCollection
{
    private readonly Dictionary<string, RigidPart> _parts = [];

    public void Add(string name, Vector3 pivot, int vertexStart, int vertexCount)
    {
        _parts[name] = new RigidPart
        {
            Pivot = pivot,
            Rotation = Quaternion.Identity,
            VertexStart = vertexStart,
            VertexCount = vertexCount
        };
    }

    public ref RigidPart Get(string name)
    {
        return ref CollectionsMarshal.GetValueRefOrNullRef(_parts, name);
    }

    public bool TryGet(string name, out RigidPart part)
    {
        return _parts.TryGetValue(name, out part);
    }
}