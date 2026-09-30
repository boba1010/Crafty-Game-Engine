using Crafty.Engine.Components;
using Crafty.Engine.Helpers;
using Crafty.SDK.Client.Blocks;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Physics;
using System.Numerics;

namespace Crafty.Engine.Systems;

public struct CollisionSystem() : ISystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        //var stopwatch = Stopwatch.StartNew();

        foreach (var entity in scene.GetEntitiesWith<Transform>())
        {
            if (!scene.HasComponent<Collider>(entity) && !scene.HasComponent<Movement>(entity))
                continue;

            ref var transform = ref scene.GetComponent<Transform>(entity);
            ref var collider = ref scene.GetComponent<Collider>(entity);

            collider.IsGrounded = false;
            collider.IsColliding = false;

            ref var movement = ref scene.GetComponent<Movement>(entity);
            Vector3 displacement = movement.Velocity * (float)deltaTime;

            int steps = Math.Max(1, (int)MathF.Ceiling(displacement.Length()));
            Vector3 step = displacement / steps;

            for (int i = 0; i < steps; i++)
            {
                transform.Position += step;

                ResolveAxis(ref transform.Position, ref collider, Vector3.UnitX);
                ResolveAxis(ref transform.Position, ref collider, Vector3.UnitY);
                ResolveAxis(ref transform.Position, ref collider, Vector3.UnitZ);
            }
        }

        //stopwatch.Stop();
        //Console.WriteLine($"Collision Update loop took: {stopwatch.Elapsed.TotalMilliseconds:F4} ms");
    }

    private void ResolveAxis(ref Vector3 position, ref Collider collider, Vector3 axis)
    {
        var min = collider.GetMin(position);
        var max = collider.GetMax(position);

        int minX = (int)MathF.Floor(min.X);
        int minY = (int)MathF.Floor(min.Y);
        int minZ = (int)MathF.Floor(min.Z);

        int maxX = (int)MathF.Floor(max.X - 0.001f);
        int maxY = (int)MathF.Floor(max.Y - 0.001f);
        int maxZ = (int)MathF.Floor(max.Z - 0.001f);

        for (int y = minY; y <= maxY; y++)
        {
            if (y < 0)
                continue;

            for (int z = minZ; z <= maxZ; z++)
            {
                if (z < 0)
                    continue;

                for (int x = minX; x <= maxX; x++)
                {
                    if (x < 0)
                        continue;

                    foreach (var box in GetCollisionBoxes(x, y, z))
                    {
                        var blockMin = new Vector3(box.MinX, box.MinY, box.MinZ);

                        var blockMax = new Vector3(box.MaxX, box.MaxY, box.MaxZ);

                        if (!Intersects(min, max, blockMin, blockMax))
                            continue;

                        Console.WriteLine($"Collision: {axis} | Player={position} | Block={blockMin}->{blockMax}");

                        Resolve(ref position, ref collider, axis, blockMin, blockMax);

                        min = collider.GetMin(position);
                        max = collider.GetMax(position);

                        collider.IsColliding = true;
                    }
                }
            }
        }
    }

    private IEnumerable<BoundingBox> GetCollisionBoxes(int x, int y, int z)
    {
        var blockId = ChunkHelper.GetBlockIdByGlobalPosition(x, y, z);
        if (blockId == 0)
            yield break;

        var block = GameAPIs.BlockRegistry.Get(blockId);

        foreach (var box in block.Collision.Boxes)
            yield return new BoundingBox(x + box.MinX, y + box.MinY, z + box.MinZ, x + box.MaxX, y + box.MaxY, z + box.MaxZ);
    }

    private static bool Intersects(Vector3 minA, Vector3 maxA, Vector3 minB, Vector3 maxB)
    {
        return minA.X < maxB.X && maxA.X > minB.X &&
               minA.Y < maxB.Y && maxA.Y > minB.Y &&
               minA.Z < maxB.Z && maxA.Z > minB.Z;
    }

    private static void Resolve(ref Vector3 position, ref Collider collider, Vector3 axis, Vector3 blockMin, Vector3 blockMax)
    {
        var min = collider.GetMin(position);
        var max = collider.GetMax(position);

        if (axis == Vector3.UnitX)
        {
            if (max.X - blockMin.X < blockMax.X - min.X)
                position.X -= max.X - blockMin.X;
            else
                position.X += blockMax.X - min.X;
        }
        else if (axis == Vector3.UnitY)
        {
            if (max.Y - blockMin.Y < blockMax.Y - min.Y)
            {
                position.Y -= max.Y - blockMin.Y;
                collider.IsGrounded = true;
            }
            else
            {
                position.Y += blockMax.Y - min.Y;
            }
        }
        else
        {
            if (max.Z - blockMin.Z < blockMax.Z - min.Z)
                position.Z -= max.Z - blockMin.Z;
            else
                position.Z += blockMax.Z - min.Z;
        }
    }
}
