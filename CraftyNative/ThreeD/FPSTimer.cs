namespace CraftyNative.ThreeD;

public static class FPSTimer
{
    private static double _elapsed;
    private static int _frames;

    public static int FPS { get; private set; }

    public static void Update(double deltaTime)
    {
        _elapsed += deltaTime;
        _frames++;

        if (_elapsed >= 1.0)
        {
            FPS = _frames;
            _elapsed = 0;
            _frames = 0;
        }
    }
}
