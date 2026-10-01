using System.Numerics;

namespace CraftyNative.ThreeD.Physics;

public static class VoxelRaycast
{
    public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, Func<int, int, int, bool> isSolid, out RaycastHit hit)
    {
        hit = default;

        direction = Vector3.Normalize(direction);

        int x = (int)MathF.Floor(origin.X);
        int y = (int)MathF.Floor(origin.Y);
        int z = (int)MathF.Floor(origin.Z);

        int stepX = Math.Sign(direction.X);
        int stepY = Math.Sign(direction.Y);
        int stepZ = Math.Sign(direction.Z);

        float tDeltaX = direction.X == 0
            ? float.PositiveInfinity
            : MathF.Abs(1f / direction.X);

        float tDeltaY = direction.Y == 0
            ? float.PositiveInfinity
            : MathF.Abs(1f / direction.Y);

        float tDeltaZ = direction.Z == 0
            ? float.PositiveInfinity
            : MathF.Abs(1f / direction.Z);

        float tMaxX = IntBound(origin.X, direction.X);
        float tMaxY = IntBound(origin.Y, direction.Y);
        float tMaxZ = IntBound(origin.Z, direction.Z);

        Vector3 normal = Vector3.Zero;
        float distance = 0;

        while (distance <= maxDistance)
        {
            if (isSolid(x, y, z))
            {
                hit = new RaycastHit(x, y, z, normal, distance);
                return true;
            }

            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                x += stepX;
                distance = tMaxX;
                tMaxX += tDeltaX;
                normal = new Vector3(-stepX, 0, 0);
            }
            else if (tMaxY < tMaxZ)
            {
                y += stepY;
                distance = tMaxY;
                tMaxY += tDeltaY;
                normal = new Vector3(0, -stepY, 0);
            }
            else
            {
                z += stepZ;
                distance = tMaxZ;
                tMaxZ += tDeltaZ;
                normal = new Vector3(0, 0, -stepZ);
            }
        }

        return false;
    }

    private static float IntBound(float coordinate, float direction)
    {
        if (direction == 0)
            return float.PositiveInfinity;

        if (direction > 0)
            return (MathF.Floor(coordinate) + 1 - coordinate) / direction;

        return (coordinate - MathF.Floor(coordinate)) / -direction;
    }
}
