using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.ECS;
using CraftyNative.ThreeD.Scenes;
using System.Numerics;

namespace Crafty.Engine.Systems;

public struct PlayerCameraSystem() : ISystem
{
    public bool IsMouseMoving { get; set; } = false;
    public float MouseSensitivity { get; set; } = 0.0025f;
    private float _yaw;
    private float _pitch;

    public void Pause(bool paused)
    {
        SystemAPI.Input.CursorMode = paused ? Cursor.Normal : Cursor.Raw;
        SystemAPI.Input.CenterMouse(SystemAPI.WindowSize);
    }

    public void Update(ref Scene scene, double deltaTime)
    {
        if (!IsMouseMoving)
            return;

        var delta = InputManager.MouseDelta;

        _yaw -= delta.X * MouseSensitivity;
        _pitch -= delta.Y * MouseSensitivity;

        const float pitchLimit = MathF.PI / 2f - 0.001f;
        _pitch = Math.Clamp(_pitch, -pitchLimit, pitchLimit);

        foreach (var player in scene.GetEntitiesWith<Player>())
        {
            ref var transform = ref scene.GetComponent<Transform>(player);
            transform.Rotation = new Vector3(_pitch, _yaw, 0f);
        }
    }
}
