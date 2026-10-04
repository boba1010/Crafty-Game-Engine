using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using System.Numerics;

namespace Crafty.Engine.Systems;

public class PlayerCameraSystem
{
    public bool IsMouseMoving { get; set; } = false;
    public float MouseSensitivity { get; set; } = 0.0025f;
    private float _yaw;
    private float _pitch;

    public void Update(ref Scene scene)
    {
        if (GameStateManager.IsInventory || GameStateManager.IsPaused)
            return;

        var delta = InputManager.MouseDelta;

        _yaw -= delta.X * MouseSensitivity;
        _pitch -= delta.Y * MouseSensitivity;

        const float pitchLimit = MathF.PI / 2f - 0.001f;
        _pitch = Math.Clamp(_pitch, -pitchLimit, pitchLimit);

        var player = scene.GetEntitiesWith<Player>().FirstOrDefault();

        ref var playerComponent = ref scene.GetComponent<Player>(player);
        ref var playerTransform = ref scene.GetComponent<Transform>(player);

        if (playerComponent.Prespective is Prespective.FirstPerson)
        {
            var playerModel = new WorldObject(1);
            ref var renderable = ref scene.GetRenderable(playerModel);
            renderable.ShouldRender = false;
        }

        playerTransform.Rotation = new Vector3(_pitch, _yaw, 0f);
    }
}
