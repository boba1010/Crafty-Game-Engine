using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;
using Crafty.Engine.Components;
using Crafty.Engine.Core;
using Crafty.Engine.Helpers;
using Crafty.Engine.Systems;
using CraftyNative;
using CraftyNative.Animation;
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
    
    public MainWindow()
    {
        Initialize();

        ChunkLoader.WorldDirectory = "saves/silly";
        var seed = WorldSeed.Generate();
        ChunkLoader.WorldGenService = new WorldGenService(ChunkLoader.WorldDirectory, seed);

        GameAPIs.Initialize();
        
        CameraSystem = new();

        Activated += Window_Activated;
        Focused += Window_Focused;
    }
    
    private void Window_Focused(object? sender, bool focused)
    {
        if (!focused)
        {
            if (GameStateManager.IsInventory)
                return;
            GameStateManager.Set(GameState.Paused);
            InputManager.CursorMode = Cursor.Normal;
            return;
        }
    }

    protected override void OnMouseMove(Vector2 position)
    {
        if (GameStateManager.IsPaused)
            return;

        if (GameStateManager.IsInventory)
        {
            CraftyNative.HUD.Inventory.SetCursor(position);
            return;
        }

        CameraSystem.IsMouseMoving = true;
        CameraSystem.Update(ref _world.Scene);
        CameraSystem.IsMouseMoving = false;
    }

    private AnimatedMesh _playerAnimation = null!;
    private AnimationManager _animationManager = null!;
    private WorldObject _player = default;
    private WorldObject _camera = default;
    private void Window_Activated(object? sender, EventArgs e)
    {
        _world = new(this)
        {
            Scene = new()
        };

        GameStateManager.Initialize();


        _camera = _world.Scene.CreateObject();
        var playerModel = BuildPlayer();
        _player = _world.Scene.CreateObject();
        _hand = BuildHand();

        _world.Scene.AddComponent(_camera, new Transform
        {
            LocalPosition = new(0, 0.5f, 0),
        });
        _world.Scene.AddComponent(_camera, new Camera
        {
            FieldOfView = 70f * MathF.PI / 180f,
            NearPlane = 0.1f,
            FarPlane = 1000f
        });
        _world.Scene.SetParent(playerModel, _player);
        _world.Scene.SetParent(_camera, _player);
        _world.Scene.SetParent(_hand, _camera);
        _world.Scene.AddComponent(_player, new Player());
        _world.Scene.AddComponent(_player, new Inventory());
        _world.Scene.AddComponent(_player, new Hotbar());
        _world.Scene.AddComponent(_player, new Transform() { Position = new(0, 75, 0) });
        _world.Scene.AddComponent(_player, new Movement());
        _world.Scene.AddComponent(_player, new Collider(new(0.6f, 1.8f, 0.6f)));

        var world = WorldIOManager.LoadWorld(Path.Combine(ChunkLoader.WorldDirectory, "silly.world"));
        world.BlockChanged += (change) => WorldMeshManager.MarkBlockDirty(change.X, change.Y, change.Z);

        _world.Scene.Systems.Add(new HandViewModelSystem(_hand, _animationManager));
        _world.Scene.Systems.Add(new PlayerMovementSystem());
        _world.Scene.Systems.Add(new HotbarSystem());
        _world.Scene.Systems.Add(new InventorySystem());
        _world.Scene.Systems.Add(new ChunkStreamingSystem(world));
        _world.Scene.Systems.Add(new CollisionSystem());
        _world.Scene.Systems.Add(new PlayerBlockInteractionSystem(world));
        _world.Scene.Systems.Add(new WorldMeshUpdateSystem(world));

        SystemAPI.JobSystem = new JobSystem();
    }

    private WorldObject BuildPlayer()
    {
        var mesh = MeshBuilder.BuildPlayerMesh();

        _playerAnimation = PlayerAnimation.Create(mesh);
        _animationManager = new(_playerAnimation);

        var playerModel = _world.Scene.CreateObject();

        _world.Scene.AddComponent(playerModel, new Transform
        {
            LockXAxis = true,
            LockZAxis = true,
            //Position = new(0, 75, 0),
            LocalPosition = new(0, -0.9f, 0)
        });

        _world.Scene.SetRenderable(playerModel, new(mesh, true));

        return playerModel;
    }

    private WorldObject _hand;

    private WorldObject BuildHand()
    {
        var mesh = MeshBuilder.BuildPlayerHandMesh();
        var hand = _world.Scene.CreateObject();

        _world.Scene.AddComponent(hand, new Transform
        {
            LocalPosition = HandViewModelSystem.RestPosition,
            LocalRotation = HandViewModelSystem.RestRotation,
            RotateAroundParent = true
        });

        _world.Scene.SetRenderable(hand, new(mesh, true));
        return hand;
    }

    private static double _elapsed;
    private static int _frames;
    public static int FPS { get; private set; }
    public double WorstFrameTime { get; private set; }

    protected override void Render(double deltaTime)
    {
        ref var movement = ref _world.Scene.GetComponent<Movement>(_player);
        ref var cameraTransform = ref _world.Scene.GetComponent<Transform>(_camera);
        ref var handTransform = ref _world.Scene.GetComponent<Transform>(_hand);
        bool isMoving = movement.Velocity.X != 0f || movement.Velocity.Z != 0f;
        _animationManager.Update((float)deltaTime, isMoving, ref cameraTransform);

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