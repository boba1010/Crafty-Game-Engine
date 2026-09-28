using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.ECS;
using CraftyNative.ThreeD.Scenes;
using System.Numerics;

namespace Crafty.Engine.Systems;

public struct PlayerMovementSystem() : ISystem
{
    public float Speed { get; set; } = 20f;

    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var camera in scene.GetEntitiesWith<Player>())
        {
            ref var transform = ref scene.GetComponent<Transform>(camera);

            Vector3 movement = Vector3.Zero;

            if (SystemAPI.Input.IsKeyDown(Key.W))
                movement.Z -= 1;

            if (SystemAPI.Input.IsKeyDown(Key.S))
                movement.Z += 1;

            if (SystemAPI.Input.IsKeyDown(Key.A))
                movement.X -= 1;

            if (SystemAPI.Input.IsKeyDown(Key.D))
                movement.X += 1;

            if (SystemAPI.Input.IsKeyDown(Key.Space))
                movement.Y += 1;

            if (SystemAPI.Input.IsKeyDown(Key.ShiftLeft))
                movement.Y -= 1;

            if (movement == Vector3.Zero)
                continue;

            movement = Vector3.Normalize(movement);

            float yaw = transform.Rotation.Y;

            float sin = MathF.Sin(yaw);
            float cos = MathF.Cos(yaw);

            movement = new Vector3(movement.X * cos + movement.Z * sin, movement.Y, -movement.X * sin + movement.Z * cos);

            transform.Position += movement * Speed * (float)deltaTime;
        }
    }
}
