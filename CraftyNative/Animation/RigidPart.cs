using System.Numerics;

namespace CraftyNative.Animation;

public struct RigidPart(Vector3 pivot, int vertexStart, int vertexCount)
{
    public Vector3 Pivot = pivot;
    public Quaternion Rotation = Quaternion.Identity;

    public int VertexStart = vertexStart;
    public int VertexCount = vertexCount;
}