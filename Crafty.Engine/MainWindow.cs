using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;
using Crafty.Engine.Components;
using Crafty.Engine.Core;
using Crafty.Engine.Helpers;
using Crafty.Engine.Systems;
using Crafty.Engine.UI;
using CraftyNative;
using CraftyNative.Animation;
using CraftyNative.ECS;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.Physics;
using CraftyNative.UI;
using System.Numerics;

namespace Crafty.Engine;

public sealed class MainWindow : Window
{
    public PlayerCameraSystem CameraSystem;
    private GameWorld _gameWorld = null!;
    private World _world = null!;
    private bool _quitRequested;

    public MainWindow()
    {
        Initialize();

        var seed = WorldSeed.Generate();
        var worldPath = Program.LaunchConfig.SelectedWorldPath;
        ChunkLoader.WorldGenService = new WorldGenService(Path.GetDirectoryName(worldPath)!);

        GameAPIs.Initialize();
        
        CameraSystem = new();

        Activated += Window_Activated;
        Focused += Window_Focused;
    }

    private void Resume()
    {
        GameStateManager.Set(GameState.Playing);
        InputManager.CursorMode = Cursor.Raw;
    }

    private void Pause()
    {
        GameStateManager.Set(GameState.Paused);
        InputManager.CursorMode = Cursor.Normal;
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
        CameraSystem.Update(ref _gameWorld.Scene);
        CameraSystem.IsMouseMoving = false;
    }

    protected override void OnKeyDown(Key key)
    {
        if (key is Key.Escape && GameStateManager.IsPlaying)
            Pause();
        else if (key is Key.Escape && GameStateManager.IsPaused)
            Resume();
    }

    private void QuitAndSave()
    {
        WorldManager.Save();
        WorldManager.DeleteUnchangedChunks();
        _quitRequested = true;
    }

    private AnimatedMesh _playerAnimation = null!;
    private AnimationManager _animationManager = null!;
    private WorldObject _player = default;
    private WorldObject _camera = default;
    private void Window_Activated(object? sender, EventArgs e)
    {
        GameStateManager.Set(GameState.Paused);

        _gameWorld = new(this)
        {
            Scene = new()
        };

        PauseMenu.Initialize(Resume, QuitAndSave);

        _camera = _gameWorld.Scene.CreateObject();
        var playerModel = BuildPlayer();
        _player = _gameWorld.Scene.CreateObject();
        _hand = BuildHand();

        _gameWorld.Scene.AddComponent(_camera, new Transform
        {
            LocalPosition = new(0, 0.5f, 0),
        });
        _gameWorld.Scene.AddComponent(_camera, new Camera
        {
            FieldOfView = 70f * MathF.PI / 180f,
            NearPlane = 0.1f,
            FarPlane = 1000f
        });
        _gameWorld.Scene.SetParent(playerModel, _player);
        _gameWorld.Scene.SetParent(_camera, _player);
        _gameWorld.Scene.SetParent(_hand, _camera);
        _gameWorld.Scene.AddComponent(_player, new Player());
        _gameWorld.Scene.AddComponent(_player, new Inventory());
        _gameWorld.Scene.AddComponent(_player, new Hotbar());
        _gameWorld.Scene.AddComponent(_player, new Transform() { Position = new(0, 75, 0) });
        _gameWorld.Scene.AddComponent(_player, new Movement());
        _gameWorld.Scene.AddComponent(_player, new Collider(new(0.6f, 1.8f, 0.6f)));

        WorldManager.Load();
        _world = WorldManager.Current;
        _world.BlockChanged += (change) => WorldMeshManager.MarkBlockDirty(change.X, change.Y, change.Z);

        _gameWorld.Scene.Systems.Add(new HandViewModelSystem(_hand, _animationManager));
        _gameWorld.Scene.Systems.Add(new PlayerMovementSystem());
        _gameWorld.Scene.Systems.Add(new HotbarSystem());
        _gameWorld.Scene.Systems.Add(new InventorySystem());
        _gameWorld.Scene.Systems.Add(new ChunkStreamingSystem(_world));
        _gameWorld.Scene.Systems.Add(new CollisionSystem());
        _gameWorld.Scene.Systems.Add(new PlayerBlockInteractionSystem(_world));
        _gameWorld.Scene.Systems.Add(new WorldMeshUpdateSystem(_world));

        SystemAPI.JobSystem = new JobSystem();
    }

    // for third person rendering in later alpha versions
    private WorldObject BuildPlayer()
    {
        var mesh = MeshBuilder.BuildPlayerMesh();

        _playerAnimation = PlayerAnimation.Create(mesh);
        _animationManager = new(_playerAnimation);

        var playerModel = _gameWorld.Scene.CreateObject();

        _gameWorld.Scene.AddComponent(playerModel, new Transform
        {
            LockXAxis = true,
            LockZAxis = true,
            LocalPosition = new(0, -0.9f, 0)
        });

        _gameWorld.Scene.SetRenderable(playerModel, new(mesh, false));

        return playerModel;
    }

    private WorldObject _hand;
    private WorldObject BuildHand()
    {
        var mesh = MeshBuilder.BuildPlayerHandMesh();
        var hand = _gameWorld.Scene.CreateObject();

        _gameWorld.Scene.AddComponent(hand, new Transform
        {
            LocalPosition = HandViewModelSystem.RestPosition,
            LocalRotation = HandViewModelSystem.RestRotation,
            RotateAroundParent = true
        });

        _gameWorld.Scene.SetRenderable(hand, new(mesh, true));
        return hand;
    }

    private static double _elapsed;
    private static int _frames;
    public static int FPS { get; private set; }
    public double WorstFrameTime { get; private set; }

    protected override void Render(double deltaTime)
    {
        ref var movement = ref _gameWorld.Scene.GetComponent<Movement>(_player);
        ref var cameraTransform = ref _gameWorld.Scene.GetComponent<Transform>(_camera);
        ref var handTransform = ref _gameWorld.Scene.GetComponent<Transform>(_hand);
        bool isMoving = movement.Velocity.X != 0f || movement.Velocity.Z != 0f;
        _animationManager.Update((float)deltaTime, isMoving, ref cameraTransform);

        if (GameStateManager.IsPaused)
        {
            UIRenderer.DrawRectangle(new(0, 0), Size, new(0, 0, 0, 0.35f));
            PauseMenu.Render(Size);
        }

        _gameWorld.Render(deltaTime);

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

        if (_quitRequested)
            Close();
    }

    public override void Dispose()
    {
        SystemAPI.JobSystem.Dispose();
        _gameWorld?.Dispose();
        base.Dispose();
    }
}