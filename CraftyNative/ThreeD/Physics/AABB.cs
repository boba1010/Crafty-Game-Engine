using System.Numerics;

namespace CraftyNative.ThreeD.Physics;

public struct AABB
{
    public Vector3 Center;
    public Vector3 HalfExtents;

    public Vector3 Min => Center - HalfExtents;
    public Vector3 Max => Center + HalfExtents;
}
