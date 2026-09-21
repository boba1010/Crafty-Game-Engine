using CraftyNative.ThreeD.ECS;
using System.Numerics;

namespace CraftyNative.ThreeD;

public struct Transform : IComponent
{
    public Vector3 Position { get; set; }
    public Vector3 LocalPosition { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 LocalRotation { get; set; }
    public Vector3 Scale { get; set; } = Vector3.One;

    public Transform()
    {
    }
}
