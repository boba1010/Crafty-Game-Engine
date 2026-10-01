using Crafty.Engine.Components;
using Crafty.Engine.Helpers;
using Crafty.SDK.Client.Blocks;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Physics;
using System.Numerics;

namespace Crafty.Engine.Systems;

public sealed class CollisionSystem : ISystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var entity in scene.GetEntitiesWith<Transform>())
        {
            if (!scene.HasComponent<Collider>(entity) || !scene.HasComponent<Movement>(entity))
                continue;

            ref var movement = ref scene.GetComponent<Movement>(entity);
            ref var transform = ref scene.GetComponent<Transform>(entity);

            ref var collider = ref scene.GetComponent<Collider>(entity);
            movement.IsGrounded = false;
            collider.IsColliding = false;

            Vector3 displacement = movement.Velocity * (float)deltaTime;

            int steps = Math.Max(1, (int)MathF.Ceiling(displacement.Length()));
            Vector3 step = displacement / steps;

            for (int i = 0; i < steps; i++)
            {
                ResolveAxis(ref transform.Position, ref collider, ref movement, step.Y, Vector3.UnitY);
                ResolveAxis(ref transform.Position, ref collider, ref movement, step.X, Vector3.UnitX);
                ResolveAxis(ref transform.Position, ref collider, ref movement, step.Z, Vector3.UnitZ);
            }
        }
    }

    private void ResolveAxis(ref Vector3 position, ref Collider collider, ref Movement movement, float amount, Vector3 axis)
    {
        Vector3 candidate = position + axis * amount;
        var min = collider.GetMin(candidate);
        var max = collider.GetMax(candidate);

        int minX = (int)MathF.Floor(min.X);
        int minY = (int)MathF.Floor(min.Y);
        int minZ = (int)MathF.Floor(min.Z);
        int maxX = (int)MathF.Floor(max.X - 0.001f);
        int maxY = (int)MathF.Floor(max.Y - 0.001f);
        int maxZ = (int)MathF.Floor(max.Z - 0.001f);

        for (int y = minY; y <= maxY; y++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    foreach (var box in GetCollisionBoxes(x, y, z))
                    {
                        var blockMin = new Vector3(box.MinX, box.MinY, box.MinZ);
                        var blockMax = new Vector3(box.MaxX, box.MaxY, box.MaxZ);

                        if (Intersects(min, max, blockMin, blockMax))
                        {
                            collider.IsColliding = true;

                            if (axis == Vector3.UnitY && amount < 0)
                                movement.IsGrounded = true;

                            return;
                        }
                    }
                }
            }
        }

        position = candidate;
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

    private static void Resolve(ref Vector3 position, ref Collider collider, ref Movement movement, Vector3 axis, Vector3 blockMin, Vector3 blockMax)
    {
        var min = collider.GetMin(position);
        var max = collider.GetMax(position);

        if (axis == Vector3.UnitX)
        {
            if (movement.Velocity.X > 0)
            {
                position.X -= max.X - blockMin.X;
                movement.Velocity.X = 0;
            }
            else if (movement.Velocity.X < 0)
            {
                position.X += blockMax.X - min.X;
                movement.Velocity.X = 0;
            }
        }
        else if (axis == Vector3.UnitY)
        {
            if (movement.Velocity.Y > 0)
            {
                position.Y -= max.Y - blockMin.Y;
                movement.Velocity.Y = 0;
            }
            else if (movement.Velocity.Y < 0)
            {
                position.Y += blockMax.Y - min.Y;
                movement.Velocity.Y = 0;
                movement.IsGrounded = true;
            }
        }
        else if (axis == Vector3.UnitZ)
        {
            if (movement.Velocity.Z > 0)
            {
                position.Z -= max.Z - blockMin.Z;
                movement.Velocity.Z = 0;
            }
            else if (movement.Velocity.Z < 0)
            {
                position.Z += blockMax.Z - min.Z;
                movement.Velocity.Z = 0;
            }
        }
    }
}
