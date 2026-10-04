using System.Numerics;
using CraftyNative.ThreeD.Meshes;

namespace CraftyNative.Animation;

public static class PlayerAnimation
{
    public static AnimatedMesh Create(Mesh mesh)
    {
        var animation = new AnimatedMesh(mesh);

        animation.AddPart("Head", new(0, 1.5f, 0), 0, 24);
        animation.AddPart("Body", new(0, 0.75f, 0), 24, 24);
        animation.AddPart("LeftArm", new(-0.25f, 1.5f, 0), 48, 24);
        animation.AddPart("RightArm", new(0.25f, 1.5f, 0), 72, 24);
        animation.AddPart("LeftLeg", new(-0.125f, 0.75f, 0), 96, 24);
        animation.AddPart("RightLeg", new(0.125f, 0.75f, 0), 120, 24);

        return animation;
    }

    public static void Walk(AnimatedMesh player, float time)
    {
        player.ResetPose();

        float swing = MathF.Sin(time * 8f) * 0.5f;

        ref var body = ref player.GetPart("Body");
        body.Rotation = Quaternion.Identity;

        ref var leftArm =
            ref player.GetPart("LeftArm");

        ref var rightArm =
            ref player.GetPart("RightArm");

        ref var leftLeg =
            ref player.GetPart("LeftLeg");

        ref var rightLeg =
            ref player.GetPart("RightLeg");

        leftArm.Rotation =
            Quaternion.CreateFromAxisAngle(
                Vector3.UnitX,
                swing);

        rightArm.Rotation =
            Quaternion.CreateFromAxisAngle(
                Vector3.UnitX,
                -swing);

        leftLeg.Rotation =
            Quaternion.CreateFromAxisAngle(
                Vector3.UnitX,
                -swing);

        rightLeg.Rotation =
            Quaternion.CreateFromAxisAngle(
                Vector3.UnitX,
                swing);

        player.Apply();
    }

    public static void Idle(AnimatedMesh player)
    {
        player.ResetPose();
        player.Apply();
    }
}