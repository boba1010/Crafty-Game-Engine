using Crafty.Components;
using Crafty.UI;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;

namespace Crafty.Systems.Physics;

public sealed class FallDamageSystem : IGameplaySystem
{
    public float SafeFallDistance { get; set; } = 3f;
    public float DamagePerBlock { get; set; } = 1f;

    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var entity in scene.GetEntitiesWith<FallDamage>())
        {
            if (!scene.HasComponent<Transform>(entity) || !scene.HasComponent<Movement>(entity))
                continue;

            ref var fall = ref scene.GetComponent<FallDamage>(entity);
            ref var transform = ref scene.GetComponent<Transform>(entity);
            ref var movement = ref scene.GetComponent<Movement>(entity);

            if (scene.HasComponent<Player>(entity))
            {
                ref var player = ref scene.GetComponent<Player>(entity);

                if (player.GameMode is GameMode.Creative)
                {
                    fall.IsTracking = false;
                    continue;
                }
            }

            if (movement.IsFlying)
            {
                fall.IsTracking = false;
                continue;
            }

            if (!movement.IsGrounded)
            {
                if (!fall.IsTracking)
                {
                    fall.IsTracking = true;
                    fall.HighestY = transform.Position.Y;
                }
                else
                    fall.HighestY = MathF.Max(fall.HighestY, transform.Position.Y);

                continue;
            }

            if (!fall.IsTracking)
                continue;

            float fallDistance = fall.HighestY - transform.Position.Y;
            fall.IsTracking = false;

            if (fallDistance <= SafeFallDistance || !scene.HasComponent<Damageable>(entity))
                continue;

            int damage = (int)MathF.Floor((fallDistance - SafeFallDistance) * DamagePerBlock);

            if (damage <= 0)
                continue;

            ref var health = ref scene.GetComponent<Damageable>(entity);
            health.Health = Math.Max(0, health.Health - damage);

            if (scene.HasComponent<Player>(entity))
                HudHearts.SetHealth(health.Health);
        }
    }
}
