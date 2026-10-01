using CraftyNative.ECS;
using System.Numerics;

namespace CraftyNative.ThreeD.Physics;

public struct Collider : IComponent
{
    public Vector3 Size;
    public Vector3 Offset;

    public bool IsColliding;

    public Collider(Vector3 size)
    {
        Size = size;
        Offset = Vector3.Zero;
        IsColliding = false;
    }

    public Collider(Vector3 size, Vector3 offset)
    {
        Size = size;
        Offset = offset;
        IsColliding = false;
    }

    public readonly Vector3 HalfExtents => Size * 0.5f;

    public readonly Vector3 GetMin(Vector3 position)
    {
        return position + Offset - HalfExtents;
    }

    public readonly Vector3 GetMax(Vector3 position)
    {
        return position + Offset + HalfExtents;
    }
}