using System.Numerics;

namespace CraftyNative.Animation;

public static class ViewModelAnimation
{
    public static void Idle(out Vector3 position, out Vector3 rotation)
    {
        position = Vector3.Zero;
        rotation = Vector3.Zero;
    }

    public static void Walk(float time, out Vector3 position, out Vector3 rotation)
    {
        float bob = MathF.Sin(time * 10f);

        position = new(0f, bob * 0.015f, 0f);
        rotation = new(bob * 0.025f, 0f, bob * 0.015f);
    }
}