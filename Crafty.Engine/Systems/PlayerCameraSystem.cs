using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using System.Numerics;

namespace Crafty.Engine.Systems;

public sealed class PlayerCameraSystem(WorldObject playerModel)
{
    public bool IsMouseMoving { get; set; }

    public float MouseSensitivity { get; set; } = 0.0025f;

    public float Yaw { get; private set; }
    public float Pitch { get; private set; }

    public void Update(ref Scene scene)
    {
        if (GameStateManager.IsInventory || GameStateManager.IsPaused)
        {
            IsMouseMoving = false;
            return;
        }

        var player = scene.GetEntitiesWith<Player>().FirstOrDefault();
        var camera = scene.GetEntitiesWith<Camera>().FirstOrDefault();

        var delta = InputManager.MouseDelta;

        IsMouseMoving = delta.X != 0f || delta.Y != 0f;

        Yaw -= delta.X * MouseSensitivity;
        Pitch -= delta.Y * MouseSensitivity;

        const float pitchLimit = MathF.PI / 2f - 0.001f;
        Pitch = Math.Clamp(Pitch, -pitchLimit, pitchLimit);

        ref var playerComponent = ref scene.GetComponent<Player>(player);
        ref var playerTransform = ref scene.GetComponent<Transform>(player);
        ref var cameraTransform = ref scene.GetComponent<Transform>(camera);
        ref var playerRenderable = ref scene.GetRenderable(playerModel);

        playerTransform.Rotation = new Vector3(0f, Yaw, 0f);

        cameraTransform.LocalRotation = new Vector3(Pitch, 0f, 0f);

        playerRenderable.ShouldRender = playerComponent.Prespective is not Prespective.FirstPerson;
    }
}