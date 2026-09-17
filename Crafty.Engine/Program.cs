using CraftyNative3D.Scenes;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Vulcan;

namespace Crafty.Engine;

internal unsafe class Program
{
    public static IWindow Window { get; private set; } = null!;
    public static IInputContext Input { get; private set; } = null!;
    
    private static Scene _scene = null!;

    public static void Main()
    {
        var options = WindowOptions.Default;
        options.Title = "Crafty";
        options.API = GraphicsAPI.None;
        options.Size = new(1280, 720);
        options.VSync = false;
        
        try
        {
            Window = Silk.NET.Windowing.Window.Create(options);

            Window.Load += OnWindowLoad;
            Window.Render += OnRender;

            Window.Run();
        }
        finally
        {
            Dispose();
        }
    }

    //private static WorldObject _cube;

    private static void OnWindowLoad()
    {
        Input = Window.CreateInput();

        //_scene = new Scene(Window);

        //_cube = _scene.CreateObject();
        //_scene.SetRenderable(_cube, new(PrimitiveMeshes.Cube));
    }

    private static void OnRender(double deltaTime)
    {
        //ref var transform = ref _scene.GetTransform(_cube);
        //transform.Rotation.Y += (float)deltaTime;

        //_scene.Render();
    }

    private static void Dispose()
    {
        _scene?.Dispose();
        Input?.Dispose();
        Window?.Dispose();
    }
}