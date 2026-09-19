using Silk.NET.Windowing;
using System.Numerics;
using Vulcan;

namespace CraftyNative.TwoD;

internal static class CraftyNative2D
{
    public static I2DGraphicsDevice Device { get; private set; } = null!;
    private static IWindow _window = null!;
    public static float DpiX { get; private set; } = 96;
    public static float DpiY { get; private set; } = 96;

    public static float DpiScaleX => DpiX / 96f;
    public static float DpiScaleY => DpiY / 96f;
    public static float Width { get; private set; }
    public static float Height { get; private set; }

    public static Vector2 Size
    {
        get
        {
            Device.GetSize(out var width, out var height);
            return new Vector2(width, height);
        }
    }

    public static void Initialize(IWindow window)
    {
        _window = window;
        Device = Vulcan.Vulcan.Create2DDevice(window);
        Device.Initialize();

        UpdateDpi();
    }

    public static void UpdateDpi()
    {
        Device.GetDpi(out float dpiX, out float dpiY);

        DpiX = dpiX;
        DpiY = dpiY;

        Width = _window.Size.X / DpiScaleX;
        Height = _window.Size.Y / DpiScaleY;
    }

    public static void ResizeFrameBuffer(uint width, uint height)
    {
        Device.ResizeFrameBuffer(width, height);
    }

    public static void Dispose()
    {
        Device?.Dispose();
    }
}
