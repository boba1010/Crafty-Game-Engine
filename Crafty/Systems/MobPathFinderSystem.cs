using System.Numerics;
using Crafty.Components;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;

namespace Crafty.Systems;

public sealed class MobPathFinderSystem : IGameplaySystem
{
    public float Speed { get; set; } = 3.5f;

    public void Update(ref Scene scene, double deltaTime)
    {
        var players = scene.GetEntitiesWith<Player>();

        foreach (var player in players)
        {
            if (!scene.HasComponent<Transform>(player))
                continue;

            ref var playerTransform = ref scene.GetComponent<Transform>(player);

            foreach (var mob in scene.GetEntitiesWith<HostileMob>())
            {
                if (!scene.HasComponent<Transform>(mob) || !scene.HasComponent<Movement>(mob))
                    continue;

                ref var mobTransform = ref scene.GetComponent<Transform>(mob);

                ref var movement = ref scene.GetComponent<Movement>(mob);

                Vector3 direction = playerTransform.Position - mobTransform.Position;

                direction.Y = 0;

                float distanceSquared = direction.LengthSquared();

                if (distanceSquared < 0.01f)
                {
                    movement.Velocity.X = 0;
                    movement.Velocity.Z = 0;
                    continue;
                }

                direction = Vector3.Normalize(direction);

                mobTransform.Rotation.Y = MathF.Atan2(-direction.X, -direction.Z);

                movement.Velocity.X = direction.X * Speed;
                movement.Velocity.Z = direction.Z * Speed;
            }
        }
    }
}