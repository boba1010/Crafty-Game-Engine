using Crafty.ChunkGeneration.World;
using Crafty.Components;
using Crafty.Core;
using Crafty.Helpers;
using Crafty.SDK.Client.Blocks;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Physics;
using System.Numerics;

namespace Crafty.Systems.Physics;

public sealed class CollisionSystem(World world) : IGameplaySystem
{
    private const float CollisionEpsilon = 0.001f;
    private const float MaxStep = 0.5f;

    public void Update(ref Scene scene, double deltaTime)
    {
        float dt = (float)Math.Clamp(deltaTime, 0, 0.05);

        foreach (var entity in scene.GetEntitiesWith<Transform>())
        {
            if (!scene.HasComponent<Collider>(entity) || !scene.HasComponent<Movement>(entity))
                continue;

            ref var movement = ref scene.GetComponent<Movement>(entity);
            ref var transform = ref scene.GetComponent<Transform>(entity);
            ref var collider = ref scene.GetComponent<Collider>(entity);

            movement.IsGrounded = false;
            collider.IsColliding = false;

            Vector3 displacement = movement.Velocity * dt;

            int steps = Math.Max(1, (int)MathF.Ceiling(displacement.Length() / MaxStep));

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
        if (amount == 0)
            return;

        Vector3 candidate = position + axis * amount;
        Vector3 min = collider.GetMin(candidate);
        Vector3 max = collider.GetMax(candidate);

        int minX = (int)MathF.Floor(min.X);
        int minY = (int)MathF.Floor(min.Y);
        int minZ = (int)MathF.Floor(min.Z);

        int maxX = (int)MathF.Floor(max.X - CollisionEpsilon);
        int maxY = (int)MathF.Floor(max.Y - CollisionEpsilon);
        int maxZ = (int)MathF.Floor(max.Z - CollisionEpsilon);

        for (int y = minY; y <= maxY; y++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    foreach (var box in GetCollisionBoxes(x, y, z))
                    {
                        Vector3 blockMin = new(box.MinX, box.MinY, box.MinZ);

                        Vector3 blockMax = new(box.MaxX, box.MaxY, box.MaxZ);

                        if (!Intersects(min, max, blockMin, blockMax))
                            continue;

                        collider.IsColliding = true;

                        if (axis == Vector3.UnitY)
                        {
                            movement.Velocity.Y = 0;

                            if (amount < 0)
                                movement.IsGrounded = true;
                        }
                        else if (axis == Vector3.UnitX)
                            movement.Velocity.X = 0;
                        else
                            movement.Velocity.Z = 0;

                        return;
                    }
                }
            }
        }

        position = candidate;
    }

    private IEnumerable<BoundingBox> GetCollisionBoxes(int x, int y, int z)
    {
        uint blockId = ChunkHelper.GetBlockIdByGlobalPosition(world, x, y, z);

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
}