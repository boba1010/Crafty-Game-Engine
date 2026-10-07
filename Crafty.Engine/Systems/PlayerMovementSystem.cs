using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using System.Diagnostics;
using System.Numerics;

namespace Crafty.Engine.Systems;

public sealed class PlayerMovementSystem : IGameplaySystem
{
    public double Speed { get; set; } = 5;
    public double Gravity {get;set;} = 30;
    public double JumpForce { get; set; } = 10;
    private bool _isSprintPressed;
    private bool _isFlying = true;

    public PlayerMovementSystem()
    {
        SystemAPI.Input.KeyUp += Input_KeyUp;
        SystemAPI.Input.KeyDown += Input_KeyDown;
    }

    private void Input_KeyUp(Key key)
    {
        if (GameStateManager.IsInventory || GameStateManager.IsPaused)
            return;

        if (key is Key.ControlLeft)
            _isSprintPressed = !_isSprintPressed;
    }


    private void Input_KeyDown(Key key)
    {
        if (GameStateManager.IsInventory || GameStateManager.IsPaused)
            return;

        if (key is Key.Space && IsDoublePressed())
            _isFlying = !_isFlying;
    }

    private long _lastPress;

    private bool IsDoublePressed()
    {
        long now = Stopwatch.GetTimestamp();

        double elapsed = (now - _lastPress) / (double)Stopwatch.Frequency;

        _lastPress = now;

        return elapsed <= 0.3;
    }

    public void Update(ref Scene scene, double deltaTime)
    {
        if (GameStateManager.IsInventory)
            return;

        foreach (var player in scene.GetEntitiesWith<Player>())
        {
            ref var transform = ref scene.GetComponent<Transform>(player);
            ref var movementComponent = ref scene.GetComponent<Movement>(player);

            Vector3 movement = Vector3.Zero;

            movementComponent.IsFlying = _isFlying;

            if (SystemAPI.Input.IsKeyPressed(Key.W))
                movement.Z -= 1;

            if (SystemAPI.Input.IsKeyPressed(Key.S))
                movement.Z += 1;

            if (SystemAPI.Input.IsKeyPressed(Key.A))
                movement.X -= 1;

            if (SystemAPI.Input.IsKeyPressed(Key.D))
                movement.X += 1;

            if (_isFlying)
            {
                if (SystemAPI.Input.IsKeyPressed(Key.Space))
                    movement.Y += 1;

                if (SystemAPI.Input.IsKeyPressed(Key.ShiftLeft))
                    movement.Y -= 1;
            }
            else
            {
                movementComponent.Velocity.Y -= (float)(Gravity * deltaTime);

                if (SystemAPI.Input.IsKeyPressed(Key.Space) && movementComponent.IsGrounded)
                {
                    movementComponent.Velocity.Y = (float)JumpForce;
                    movementComponent.IsGrounded = false;
                }
            }

            if (movement.X != 0 || movement.Z != 0)
            {
                Vector2 horizontal = Vector2.Normalize(new(movement.X, movement.Z));

                float yaw = transform.Rotation.Y;
                float sin = MathF.Sin(yaw);
                float cos = MathF.Cos(yaw);

                Vector3 direction = new(horizontal.X * cos + horizontal.Y * sin, 0, -horizontal.X * sin + horizontal.Y * cos);

                float speed = (float)(_isSprintPressed ? Speed + 5 : Speed);

                movementComponent.Velocity.X = direction.X * speed;
                movementComponent.Velocity.Z = direction.Z * speed;
            }
            else
            {
                movementComponent.Velocity.X = 0;
                movementComponent.Velocity.Z = 0;
            }

            if (_isFlying)
            {
                movementComponent.Velocity.Y = movement.Y * (float)(_isSprintPressed ? Speed + 5 : Speed);
            }
        }
    }
}
