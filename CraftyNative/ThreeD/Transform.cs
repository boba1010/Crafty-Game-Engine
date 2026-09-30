using CraftyNative.ECS;
using System.Numerics;

namespace CraftyNative.ThreeD;

public struct Transform : IComponent
{
    public Vector3 Position;
    public Vector3 LocalPosition;
    public Vector3 Rotation;
    public Vector3 LocalRotation;
    public Vector3 Scale = Vector3.One;

    public Transform()
    {
    }
}
