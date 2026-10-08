using Crafty.Engine.Components;
using Crafty.Engine.Core;
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
internal sealed class HandViewModelSystem(WorldObject hand, WorldObject heldBlock, AnimationManager animation) : IGameplaySystem
{
    public static readonly Vector3 RestRotation = new(1.1f, 0f, 0f);
    public static readonly Vector3 RestPosition = new(0.56f, -0.95f, -0.7f);

    private const float SwingDuration = 0.3f;
    private const float SwapDuration = 0.6f;
    private const float SwapDistance = 2.5f;

    private float _swing = -1f;
    private float _swap = -1f;
    private bool _swapped;

    private int _lastSelectedSlot = -1;
    private bool _wasPressed;

    public void Update(ref Scene scene, double deltaTime)
    {
        bool uiActive = GameStateManager.IsInventory;

        var hotbarEntity = scene.GetEntitiesWith<Hotbar>().FirstOrDefault();

        ref var hotbar = ref scene.GetComponent<Hotbar>(hotbarEntity);

        int selectedSlot = hotbar.SelectedSlot;
        var slot = hotbar.Slots[selectedSlot];

        if (selectedSlot != _lastSelectedSlot)
        {
            bool hadPreviousBlock = _lastSelectedSlot >= 0 && hotbar.Slots[_lastSelectedSlot].BlockId.HasValue;

            bool hasCurrentBlock = slot.BlockId.HasValue;

            _lastSelectedSlot = selectedSlot;

            if (!uiActive && (hadPreviousBlock || hasCurrentBlock))
            {
                _swap = 0f;
                _swapped = false;
            }
        }

        float swapT = 0f;

        if (_swap >= 0f)
        {
            _swap += (float)deltaTime / SwapDuration;

            if (!_swapped && _swap >= 0.5f)
            {
                _swapped = true;

                if (slot.BlockId.HasValue)
                {
                    uint blockId = slot.BlockId.Value;
                    var mesh = MeshBuilder.BuildBlockMesh(blockId);

                    scene.SetRenderable(heldBlock, new(mesh, true));
                    scene.GetRenderable(hand).ShouldRender = false;
                }
                else
                {
                    scene.RemoveRenderable(heldBlock);
                    scene.GetRenderable(hand).ShouldRender = true;
                }
            }

            if (_swap >= 1f)
                _swap = -1f;
            else
                swapT = _swap;
        }

        float swapS = MathF.Sqrt(swapT);
        float swapSin = MathF.Sin(swapS * MathF.PI);

        bool down = SystemAPI.Input.IsMouseButtonDown(MouseButton.Left);

        if (down && !_wasPressed && !uiActive)
            _swing = 0f;

        _wasPressed = down;

        float swingT = 0f;

        if (_swing >= 0f)
        {
            _swing += (float)deltaTime / SwingDuration;

            if (_swing >= 1f)
                _swing = -1f;
            else
                swingT = _swing;
        }

        float swingS = MathF.Sqrt(swingT);
        float swingSin = MathF.Sin(swingS * MathF.PI);

        var posOffset = new Vector3(
            -0.4f * swingSin,
            0.5f * MathF.Sin(swingS * MathF.Tau),
            -0.2f * MathF.Sin(swingT * MathF.PI));

        var rotOffset = new Vector3(
            -80f * MathF.PI / 180f * swingSin,
            0f,
            -20f * MathF.PI / 180f * MathF.Sin(swingT * swingT * MathF.PI));

        posOffset.Y -= swapSin * SwapDistance;

        posOffset += animation.HandPositionOffset;
        rotOffset += animation.HandRotationOffset;

        ref var transform = ref scene.GetComponent<Transform>(hand);

        transform.LocalPosition = RestPosition + posOffset;
        transform.LocalRotation = RestRotation + rotOffset;
    }
}