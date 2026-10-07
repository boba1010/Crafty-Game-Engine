using Crafty.Engine.Dtos;
using Crafty.Engine.Json;
using Silk.NET.Input.Glfw;
using Silk.NET.Windowing.Glfw;
using System.Text.Json;

namespace Crafty.Engine;

internal class Program
{
    public static MainWindow Window { get; private set; } = null!;

    public static GameLaunchConfigDto LaunchConfig { get; private set; } = null!;

    public static void Main(string[] args)
    {
        try
        {
            if (args.Length < 2)
                throw new InvalidDataException("Missing arg! Try Crafty.exe --config-path launch.json or a valid path");

            if (args[0] == "--config-path")
            {
                var launchDataPath = args[1];
                using var fs = File.OpenRead(launchDataPath);
                LaunchConfig = JsonSerializer.Deserialize(fs, LaunchConfigJsonContext.Default.GameLaunchConfigDto)!;
            }

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