using Silk.NET.Input.Glfw;
using Silk.NET.Windowing.Glfw;

namespace Crafty.Engine;

internal class Program
{
    public static MainWindow Window { get; private set; } = null!;

    public static void Main()
    {
        try
        {
            GlfwWindowing.RegisterPlatform();
            GlfwInput.RegisterPlatform();

            Window = new()
            {
                Title = "Crafty"
            };

            Window.Activate();
        }
        finally
        {
            Dispose();
        }
    }

    private static void Dispose()
    {
        Window?.Dispose();
    }
}