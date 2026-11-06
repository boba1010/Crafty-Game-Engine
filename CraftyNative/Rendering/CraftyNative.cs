using CraftyNative.Environment;
using CraftyNative.HUD;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.UI;
using Silk.NET.Windowing;
using System.Diagnostics;
using System.Numerics;
using Vulcan;
using Vulcan.DirectX;
using Vulcan.Graphics;
using Vulcan.Graphics.Descriptions;

namespace CraftyNative.Rendering;

public unsafe static class CraftyNative
{
    internal static IWindow Window { get; private set; } = null!;
    internal static IGraphicsDevice Device { get; private set; } = null!;
    public static float Daylight { get; set; } = 1f;
    public static float Moonlight { get; set; } = 1f;

    private static double _maxUpdateMs, _maxDrawMs, _elapsed;

    private static ICommandBuffer _commandBuffer = null!;
    private static ISwapchain _swapchain = null!;
    private static ICommandQueue _queue = null!;
    private static ITexture _depthTexture = null!;
    
    private static RenderResources _resources = null!;
    private static MeshRenderer _meshRenderer = null!;
    private static WorldBatchRenderer _worldBatchRenderer = null!;

    internal static void Initialize(IWindow window)
    {
        Window = window;
        Device = Vulcan.Vulcan.CreateDevice(Window);
        Device.Initialize();

        CreateSwapchain(false);

        _resources = new(Device);

        _queue = Device.CreateCommandQueue();
        _commandBuffer = _queue.CreateCommandBuffer();

        ((D3D11CommandBuffer)_commandBuffer).RenderTargetView = ((D3D11Swapchain)_swapchain).RenderTargetView;
        ((D3D11CommandBuffer)_commandBuffer).DepthStencilView = ((D3D11Texture)_depthTexture).DepthStencilView;

        _meshRenderer = new(Device, _commandBuffer, _resources);
        _worldBatchRenderer = new(_meshRenderer);

        Vector2 size = new(Window.Size.X, Window.Size.Y);

        SkyRenderer.Initialize(Device);
        HudRenderer.Initialize(Device, size);
        UIRenderer.Initialize(Device);
        UITextRenderer.Initialize(Device, FontLoader.Load(Device, "Assets/Fonts/Roboto-Regular.ttf", 32));
        SpriteRenderer.Initialize(Device);

        Window.FramebufferResize += newSize =>
        {
            if (newSize.X <= 0 || newSize.Y <= 0)
                return;
            CreateSwapchain(true);
        };
    }

    private static void CreateSwapchain(bool updateViews)
    {
        _swapchain?.Dispose();
        _depthTexture?.Dispose();

        _swapchain = Device.CreateSwapchain(new SwapchainDescription
        {
            WindowHandle = Window.Native!.Win32!.Value.Hwnd,
            Width = (uint)Window.Size.X,
            Height = (uint)Window.Size.Y,
            Format = TextureFormat.R8G8B8A8Unorm,
            BufferCount = 2
        });

        _depthTexture = Device.CreateTexture(new TextureDescription
        {
            Width = (uint)Window.Size.X,
            Height = (uint)Window.Size.Y,
            Depth = 1,
            MipLevels = 1,
            ArrayLayers = 1,
            Format = TextureFormat.D32Float,
            Usage = TextureUsage.DepthStencil,
            Type = TextureType.Texture2D,
            Samples = SampleCount.X1
        });

        if (updateViews)
        {
            ((D3D11CommandBuffer)_commandBuffer).RenderTargetView = ((D3D11Swapchain)_swapchain).RenderTargetView;
            ((D3D11CommandBuffer)_commandBuffer).DepthStencilView = ((D3D11Texture)_depthTexture).DepthStencilView;
        }
    }

    internal static void Render(ref Scene scene, WorldObject cameraObject, double deltaTime)
    {
        _commandBuffer.Begin();

        var size = Window.Size;

        _commandBuffer.SetViewport(new Viewport
        {
            X = 0,
            Y = 0,
            Width = size.X,
            Height = size.Y,
            MinDepth = 0,
            MaxDepth = 1
        });

        _commandBuffer.SetScissor(new Scissor
        {
            X = 0,
            Y = 0,
            Width = (uint)size.X,
            Height = (uint)size.Y
        });

        _commandBuffer.ClearColor(1f, 1f, 1f, 1f);
        _commandBuffer.ClearDepth(1f);

        ref var cameraTransform = ref scene.GetComponent<Transform>(cameraObject);
        ref var camera = ref scene.GetComponent<Camera>(cameraObject);

        var forward = Vector3.Transform(
            -Vector3.UnitZ,
            Quaternion.CreateFromYawPitchRoll(cameraTransform.Rotation.Y, cameraTransform.Rotation.X, 0f));

        var view = Matrix4x4.CreateLookAt(cameraTransform.Position, cameraTransform.Position + forward, Vector3.UnitY);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView(camera.FieldOfView, size.X / (float)size.Y, camera.NearPlane, camera.FarPlane);

        var viewProjection = view * projection;

        SkyRenderer.Render(_commandBuffer, cameraTransform.Position, viewProjection);

        var clip = new Vector4(0, 0, size.X, size.Y);

        // ---- Pass 1: opaque + cutout (world sections, then entities/items) ----
        _commandBuffer.SetPipeline(_resources.OpaquePipeline);

        var updateWatch = Stopwatch.StartNew();
        _worldBatchRenderer.Update(cameraTransform.Position);
        updateWatch.Stop();

        var drawingStopWatch = Stopwatch.StartNew();
        _worldBatchRenderer.DrawOpaque(viewProjection, clip);
        drawingStopWatch.Stop();

        foreach (var (objectId, renderable) in scene.Renderables)
        {
            ref var transform = ref scene.GetComponent<Transform>(new WorldObject(objectId));

            var model =
                Matrix4x4.CreateScale(transform.Scale) *
                Matrix4x4.CreateRotationX(transform.Rotation.X) *
                Matrix4x4.CreateRotationY(transform.Rotation.Y) *
                Matrix4x4.CreateRotationZ(transform.Rotation.Z) *
                Matrix4x4.CreateTranslation(transform.Position);

            if (renderable.ShouldRender)
                _meshRenderer.Draw(renderable.Mesh, model * view * projection, clip: new Vector4(0, 0, size.X, size.Y));
        }

        // ---- Pass 2: translucent sections (glass, ice), far to near ----
        _commandBuffer.SetPipeline(_resources.TranslucentPipeline);
        _worldBatchRenderer.DrawTranslucent(viewProjection, clip, cameraTransform.Position);

        Vector2 vector2Size = new(size.X, size.Y);

        UIRenderer.BeginFrame(vector2Size);
        SpriteRenderer.BeginFrame(vector2Size);

        HudRenderer.Render(_commandBuffer, vector2Size);
        UIRenderer.Render(_commandBuffer, vector2Size);
        UITextRenderer.Render(_commandBuffer, vector2Size);
        SpriteRenderer.Render(_commandBuffer);

        UIRenderer.RenderOverlay(_commandBuffer, vector2Size);
        SpriteRenderer.Render(_commandBuffer);
        UITextRenderer.RenderOverlay(_commandBuffer, vector2Size);

        _commandBuffer.End();

        _swapchain.Present();

        _elapsed += deltaTime;
        _maxUpdateMs = Math.Max(_maxUpdateMs, updateWatch.Elapsed.TotalMilliseconds);
        _maxDrawMs = Math.Max(_maxDrawMs, drawingStopWatch.Elapsed.TotalMilliseconds);

        if (_elapsed >= 1.0)
        {
            Console.WriteLine($"Update max: {_maxUpdateMs:F3}ms | Draw max: {_maxDrawMs:F3}ms");
            _maxUpdateMs = _maxDrawMs = 0;
            _elapsed = 0;
        }
    }

    internal static void DrawHudMesh(Mesh mesh, Matrix4x4 mvp, Vector4 clip)
    {
        _commandBuffer.SetPipeline(_resources.OpaquePipeline);
        _meshRenderer.Draw(mesh, mvp, false, clip);
    }

    internal static void ReleaseMesh(Mesh mesh)
    {
        _meshRenderer.Release(mesh);
    }

    internal static void Dispose()
    {
        SkyRenderer.Dispose();
        HudRenderer.Dispose();
        UIRenderer.Dispose();
        UITextRenderer.Dispose();

        _worldBatchRenderer?.Dispose();
        _meshRenderer.Dispose();
        _resources.Dispose();

        NativeMaterialsManager.Clear();
        
        _depthTexture?.Dispose();
        _commandBuffer?.Dispose();
        _queue?.Dispose();
        _swapchain?.Dispose();
    }
}