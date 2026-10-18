using Crafty.Components;
using Crafty.UI;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using System.Numerics;

namespace Crafty.Systems;

public sealed class DeathSystem : IGameplaySystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var entity in scene.GetEntitiesWith<Damageable>())
        {
            ref var damageable = ref scene.GetComponent<Damageable>(entity);

            if (damageable.Health > 0 || scene.HasComponent<Dead>(entity))
                continue;

            scene.AddComponent(entity, new Dead());

            if (scene.HasComponent<Movement>(entity))
            {
                ref var movement = ref scene.GetComponent<Movement>(entity);

                movement.Velocity = Vector3.Zero;
                movement.IsGrounded = false;
            }

            if (scene.HasComponent<Player>(entity))
            {
                GameStateManager.Set(GameState.Dead);
                HudDeathScreen.Open();
            }
        }
    }
}
