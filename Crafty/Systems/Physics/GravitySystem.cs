using Crafty.Components;
using CraftyNative.ECS;
using CraftyNative.Scenes;

namespace Crafty.Systems.Physics;

public sealed class GravitySystem : IGameplaySystem
{
    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var entity in scene.GetEntitiesWith<Gravity>())
        {
            ref var gravity = ref scene.GetComponent<Gravity>(entity);
            ref var movement = ref scene.GetComponent<Movement>(entity);

            if (movement.IsFlying)
                continue;

            if (movement.IsGrounded && movement.Velocity.Y < 0)
                movement.Velocity.Y = 0;

            if (movement.IsGrounded)
                continue;

            movement.Velocity.Y -= gravity.Acceleration * (float)deltaTime;
        }
    }
}