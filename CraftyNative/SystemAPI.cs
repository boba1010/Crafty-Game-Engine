using System.Numerics;

namespace CraftyNative;

public static class SystemAPI
{
    public static InputManager Input { get; internal set; } = null!;
    public static Vector2 WindowSize { get; internal set; }
}
