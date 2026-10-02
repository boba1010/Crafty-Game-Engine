using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;
using Crafty.Engine.Components;
using Crafty.Engine.Helpers;
using Crafty.Engine.Systems;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.Physics;
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
            LocalPosition = new(0, 0.7f, 0)
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
        _world.Scene.AddComponent(playerParent, new Transform() { Position = new(0, 75, 0) });
        _world.Scene.AddComponent(playerParent, new Movement());
        _world.Scene.AddComponent(playerParent, new Collider(new(0.6f, 1.8f, 0.6f)));

        var world = WorldIOManager.LoadWorld(Path.Combine(ChunkLoader.WorldDirectory, "silly.world"));

        world.BlockChanged += (change) => WorldMeshManager.MarkBlockDirty(change.X, change.Y, change.Z);

        _world.Scene.Systems.Add(CameraSystem);
        _world.Scene.Systems.Add(new PlayerMovementSystem());
        _world.Scene.Systems.Add(new ChunkStreamingSystem(world));
        _world.Scene.Systems.Add(new CollisionSystem());
        _world.Scene.Systems.Add(new PlayerBlockInteractionSystem(world));
        _world.Scene.Systems.Add(new WorldMeshUpdateSystem(world));

        SystemAPI.JobSystem = new JobSystem();
    }

    private static double _elapsed;
    private static int _frames;
    public static int FPS { get; private set; }
    public double WorstFrameTime { get; private set; }

    protected override void Render(double deltaTime)
    {
        _world.Render(deltaTime);

        double frameTime = deltaTime * 1000.0;

        _elapsed += deltaTime;
        _frames++;

        if (frameTime > WorstFrameTime)
            WorstFrameTime = frameTime;

        if (_elapsed >= 1.0)
        {
            FPS = _frames;

            Console.WriteLine($"FPS: {FPS}");
            Console.WriteLine($"Last Frame: {frameTime:F2} ms");
            Console.WriteLine($"Worst Frame: {WorstFrameTime:F2} ms");

            _elapsed = 0;
            _frames = 0;
            WorstFrameTime = 0;
        }
    }

    public override void Dispose()
    {
        SystemAPI.JobSystem.Dispose();
        _world?.Dispose();
        base.Dispose();
    }
}
