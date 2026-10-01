using System.Numerics;

namespace CraftyNative.ThreeD.Physics;

public readonly struct RaycastHit(int x, int y, int z, Vector3 normal, float distance)
{
    public readonly int X = x;
    public readonly int Y = y;
    public readonly int Z = z;

    public readonly Vector3 Normal = normal;
    public readonly float Distance = distance;
}
