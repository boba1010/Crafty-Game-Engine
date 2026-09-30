using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using System.Numerics;

namespace Crafty.Engine.Systems;

public struct PlayerMovementSystem : ISystem
{
    public float Speed { get; set; } = 20f;
    private bool _isSprintPressed;

    public PlayerMovementSystem()
    {
        SystemAPI.Input.KeyUp += Input_KeyUp;
    }

    private void Input_KeyUp(Key key)
    {
        if (key is Key.ControlLeft)
            _isSprintPressed = !_isSprintPressed;
    }

    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var camera in scene.GetEntitiesWith<Player>())
        {
            ref var transform = ref scene.GetComponent<Transform>(camera);
            ref var movementComponent = ref scene.GetComponent<Movement>(camera);

            Vector3 movement = Vector3.Zero;

            if (SystemAPI.Input.IsKeyPressed(Key.W))
                movement.Z -= 1;

            if (SystemAPI.Input.IsKeyPressed(Key.S))
                movement.Z += 1;

            if (SystemAPI.Input.IsKeyPressed(Key.A))
                movement.X -= 1;

            if (SystemAPI.Input.IsKeyPressed(Key.D))
                movement.X += 1;

            if (SystemAPI.Input.IsKeyPressed(Key.Space))
                movement.Y += 1;

            if (SystemAPI.Input.IsKeyPressed(Key.ShiftLeft))
                movement.Y -= 1;

            if (movement == Vector3.Zero)
            {
                movementComponent.Velocity = Vector3.Zero;
                continue;
            }

            movement = Vector3.Normalize(movement);

            float yaw = transform.Rotation.Y;

            float sin = MathF.Sin(yaw);
            float cos = MathF.Cos(yaw);

            movement = new Vector3(movement.X * cos + movement.Z * sin, movement.Y, -movement.X * sin + movement.Z * cos);

            float speed = _isSprintPressed ? Speed + 5 : Speed;

            movementComponent.Velocity = movement * speed;
        }
    }
}
