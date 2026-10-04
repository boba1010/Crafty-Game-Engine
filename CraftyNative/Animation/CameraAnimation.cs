using CraftyNative.ThreeD;

namespace CraftyNative.Animation;

public static class CameraAnimation
{
    public static void WalkBob(ref Transform camera, float time, float speed)
    {
        float frequency = 8f * speed;
        float amplitude = 0.035f;

        camera.LocalPosition = new(MathF.Cos(time * frequency * 0.5f) * amplitude, 0.7f + MathF.Sin(time * frequency) * amplitude, 0);
    }

    public static void Idle(ref Transform camera)
    {
        camera.LocalPosition = new(0, 0.7f, 0);
    }
}