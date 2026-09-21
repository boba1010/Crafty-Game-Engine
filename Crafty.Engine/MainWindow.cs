using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.World;
using Crafty.Engine.Components;
using Crafty.Engine.Systems;
using CraftyNative;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.Scenes;
using System.Numerics;

namespace Crafty.Engine;

public sealed class MainWindow : Window
{
    public IWorldGenService WorldGenService { get; }
    public ChunkMeshBuilder ChunkMeshBuilder { get; }
    public PlayerCameraSystem CameraSystem;
    
    private GameWorld _world = null!;
    private Chunk _chunk = null!;
    private Mesh _mesh = null!;
    private WorldObject _chunkObject;
    
    private bool _isPaused = true;
    
    public MainWindow()
    {
        Initialize();

        WorldGenService = new WorldGenService(new(@".\saves\silly"));
        CameraSystem = new();
        ChunkMeshBuilder = new();

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
        _world = new(this)
        {
            Scene = new()
        };

        var camera = _world.Scene.CreateObject();

        _world.Scene.AddComponent(camera, new Transform
        {
            LocalPosition = new(0, 0, -3)
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

        _chunkObject = _world.Scene.CreateObject();
        _world.Scene.AddComponent(_chunkObject, new Transform());

        _chunk = WorldGenService.LoadChunk(0, 0);
        _mesh = ChunkMeshBuilder.Build(_chunk);
        _world.Scene.SetRenderable(_chunkObject, new(_mesh));
    }

    protected override void Render(double deltaTime)
    {
        _world.Render(deltaTime);
    }

    public override void Dispose()
    {
        _world?.Dispose();
        base.Dispose();
    }
}
