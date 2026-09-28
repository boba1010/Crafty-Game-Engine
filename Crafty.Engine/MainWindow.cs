using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;
using Crafty.Engine.ChunkBuilding;
using Crafty.Engine.Components;
using Crafty.Engine.Systems;
using CraftyNative;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.ECS;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.Scenes;
using CraftyNative.ThreeD.World;
using System.Numerics;

namespace Crafty.Engine;

public sealed class MainWindow : Window
{
    public PlayerCameraSystem CameraSystem;
    
    private GameWorld _world = null!;
    private bool _isPaused = true;
    
    public MainWindow()
    {
        Initialize();

        ChunkLoader.WorldDirectory = "saves/silly";
        var seed = WorldSeed.Generate();
        ChunkLoader.WorldGenService = new WorldGenService(ChunkLoader.WorldDirectory, seed);

        CameraSystem = new();

        Activated += Window_Activated;
        Focused += Window_Focused;
    }
    
    private void Window_Focused(object? sender, bool focused)
    {
        if (!focused)
        {
            _isPaused = true;
            InputManager.CursorMode = Cursor.Normal;
            return;
        }
    }

    protected override void OnMouseMove(Vector2 position)
    {
        if (_isPaused)
            return;

        CameraSystem.IsMouseMoving = true;
        CameraSystem.Update(ref _world.Scene, 0);
        CameraSystem.IsMouseMoving = false;
    }

    protected override void OnKeyDown(Key key)
    {
        if (key != Key.Escape)
            return;
        _isPaused = !_isPaused;
        CameraSystem.Pause(_isPaused);
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        GameAPIs.Initialize();

        _world = new(this)
        {
            Scene = new()
        };

        var camera = _world.Scene.CreateObject();

        _world.Scene.AddComponent(camera, new Transform
        {
            LocalPosition = new(0, 2, -3)
        });
        _world.Scene.AddComponent(camera, new Camera
        {
            FieldOfView = MathF.PI / 4,
            NearPlane = 0.1f,
            FarPlane = 1000f
        });

        var playerParent = _world.Scene.CreateObject();
        _world.Scene.SetParent(camera, playerParent);
        _world.Scene.AddComponent(playerParent, new Player());
        _world.Scene.AddComponent(playerParent, new Transform());
        
        _world.Scene.Systems.Add(CameraSystem);
        _world.Scene.Systems.Add(new PlayerMovementSystem());

        var world = WorldIOManager.LoadWorld(Path.Combine(ChunkLoader.WorldDirectory, "silly.world"));
        _world.Scene.Systems.Add(new ChunkStreamingSystem(world));

        SystemAPI.JobSystem = new JobSystem();
    }

    protected override void Render(double deltaTime)
    {
        _world.Render(deltaTime);
    }

    public override void Dispose()
    {
        SystemAPI.JobSystem.Dispose();
        _world?.Dispose();
        base.Dispose();
    }
}
