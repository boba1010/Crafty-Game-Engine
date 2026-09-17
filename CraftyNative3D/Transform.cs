using System.Numerics;

namespace CraftyNative3D;

public struct Transform
{
    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale = Vector3.One;

    public Transform()
    {
    }
}
