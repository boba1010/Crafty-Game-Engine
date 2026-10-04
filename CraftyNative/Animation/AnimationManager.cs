using CraftyNative.ThreeD;
using System.Numerics;

namespace CraftyNative.Animation;

public sealed class AnimationManager(AnimatedMesh player)
{
    private float _walkBlend; // 0 = still, 1 = full bob

    public AnimatedMesh Player { get; } = player;
    public float Time { get; private set; }

    public Vector3 HandPositionOffset { get; private set; }
    public Vector3 HandRotationOffset { get; private set; }

    public void Update(float deltaTime, bool moving, ref Transform cameraTransform)
    {
        Time += deltaTime;

        _walkBlend = Math.Clamp(_walkBlend + (moving ? 1f : -1f) * deltaTime * 6f, 0f, 1f);

        if (moving)
        {
            PlayerAnimation.Walk(Player, Time);
            CameraAnimation.WalkBob(ref cameraTransform, Time, 2f);
        }
        else
        {
            PlayerAnimation.Idle(Player);
            CameraAnimation.Idle(ref cameraTransform);
        }

        ViewModelAnimation.Walk(Time, out var pos, out var rot);
        HandPositionOffset = pos * _walkBlend;
        HandRotationOffset = rot * _walkBlend;
    }
}