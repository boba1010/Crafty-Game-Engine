using CraftyNative;
using CraftyNative.Animation;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using System.Numerics;

namespace Crafty.Engine.Systems;

/// <summary>
/// Drives the first-person hand. The hand is a child of the camera, so it only ever
/// sets LocalPosition / LocalRotation (camera-space); HierarchySystem does the rest.
/// Must run BEFORE HierarchySystem (insert it at index 0 of Scene.Systems).
/// </summary>
internal sealed class HandViewModelSystem(WorldObject hand, AnimationManager animation) : IGameplaySystem
{
    public static readonly Vector3 RestRotation = new(1.1f, 0f, 0f);       // ~63 deg up
    public static readonly Vector3 RestPosition = new(0.56f, -0.95f, -0.7f);

    private const float SwingDuration = 0.3f; // seconds
    private float _swing = -1f; // -1 = idle, otherwise progress 0..1
    private bool _wasPressed;

    public void Update(ref Scene scene, double deltaTime)
    {
        bool uiActive = GameStateManager.IsInventory;

        bool down = SystemAPI.Input.IsMouseButtonDown(MouseButton.Left);

        if (down && !_wasPressed && !uiActive)
            _swing = 0f;

        _wasPressed = down;

        float t = 0f;
        if (_swing >= 0f)
        {
            _swing += (float)deltaTime / SwingDuration;
            if (_swing >= 1f)
                _swing = -1f;
            else
                t = _swing;
        }

        float s = MathF.Sqrt(t);
        float swingSin = MathF.Sin(s * MathF.PI);

        var posOffset = new Vector3(
            -0.4f * swingSin,
             0.5f * MathF.Sin(s * MathF.Tau),
            -0.2f * MathF.Sin(t * MathF.PI));

        var rotOffset = new Vector3(-80f * MathF.PI / 180f * swingSin, 0f, -20f * MathF.PI / 180f * MathF.Sin(t * t * MathF.PI));

        posOffset += animation.HandPositionOffset;
        rotOffset += animation.HandRotationOffset;

        ref var transform = ref scene.GetComponent<Transform>(hand);
        transform.LocalPosition = RestPosition + posOffset;
        transform.LocalRotation = RestRotation + rotOffset;
    }
}