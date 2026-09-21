using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.Scenes;
using Silk.NET.Windowing;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.DirectX;
using Vulcan.Graphics;
using Vulcan.Graphics.Descriptions;

namespace CraftyNative.ThreeD;

internal unsafe static class CraftyNative3D
{
    public static IWindow Window { get; private set; } = null!;
    public static IGraphicsDevice Device { get; private set; } = null!;

    private static readonly Dictionary<Mesh, GPUMesh> _gpuMeshes = [];

    private static ICommandBuffer _commandBuffer = null!;
    private static IPipeline _pipeline = null!;
    private static ISwapchain _swapchain = null!;
    private static ICommandQueue _queue = null!;
    private static IShader _vertexShader = null!;
    private static IShader _fragmentShader = null!;
    private static IVertexLayout _vertexLayout = null!;
    private static IRasterizerState _rasterizerState = null!;
    private static IDepthStencilState _depthStencilState = null!;
    private static IBlendState _blendState = null!;
    private static IBuffer _constantBuffer = null!;
    private static ITexture _depthTexture = null!;

    private static GPUMesh GetOrCreateMesh(Mesh mesh)
    {
        if (_gpuMeshes.TryGetValue(mesh, out var gpuMesh))
            return gpuMesh;

        var vertexData = MemoryMarshal.AsBytes(mesh.Vertices.AsSpan());

        var vertexBuffer = Device.CreateBuffer(new()
        {
            Size = (ulong)vertexData.Length,
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });

        vertexBuffer.Upload(vertexData);

        var indexData = MemoryMarshal.AsBytes(mesh.Indices.AsSpan());

        var indexBuffer = Device.CreateBuffer(new()
        {
            Size = (ulong)indexData.Length,
            Usage = BufferUsage.Index,
            MemoryUsage = MemoryUsage.Upload
        });

        indexBuffer.Upload(indexData);

        gpuMesh = new(vertexBuffer, indexBuffer);
        _gpuMeshes.Add(mesh, gpuMesh);

        return gpuMesh;
    }

    public static void Initialize(IWindow window)
    {
        Window = window;
        Device = Vulcan.Vulcan.CreateDevice(Window);
        Device.Initialize();

        CreateSwapchain(false);

        var shaderSource = """
        struct VSInput
        {
            float3 Position : POSITION;
        };

        struct VSOutput
        {
            float4 Position : SV_Position;
        };

        cbuffer Transform : register(b0)
        {
            row_major matrix MVP;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = mul(float4(input.Position, 1.0), MVP);
            return output;
        }

        float4 PSMain(VSOutput input) : SV_Target
        {
            return float4(1.0, 0.0, 0.0, 1.0);
        }
        """;

        var vertexShaderCode =
            Shaders.CompileShader(shaderSource, "VSMain", "vs_5_0");

        var fragmentShaderCode =
            Shaders.CompileShader(shaderSource, "PSMain", "ps_5_0");

        _vertexShader = Device.CreateShader(new()
        {
            Code = vertexShaderCode,
            Stage = ShaderStage.Vertex,
            EntryPoint = "VSMain"
        });

        _fragmentShader = Device.CreateShader(new()
        {
            Code = fragmentShaderCode,
            Stage = ShaderStage.Fragment,
            EntryPoint = "PSMain"
        });

        _vertexLayout = Device.CreateVertexLayout(new()
        {
            VertexShader = _vertexShader,
            Elements = new VertexElement[]
            {
                new()
                {
                    Semantic = "POSITION",
                    Location = 0,
                    Format = TextureFormat.R32G32B32Float,
                    Offset = 0
                }
            }
        });

        _rasterizerState = Device.CreateRasterizerState(new()
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Solid,
            DepthClipEnable = true
        });

        _depthStencilState = Device.CreateDepthStencilState(new()
        {
            DepthTestEnable = true,
            DepthWriteEnable = true,
            DepthCompare = CompareOperation.Less
        });

        _blendState = Device.CreateBlendState(new()
        {
            Enable = false
        });

        _pipeline = Device.CreatePipeline(new()
        {
            VertexShader = _vertexShader,
            FragmentShader = _fragmentShader,
            VertexLayout = _vertexLayout,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            Rasterizer = _rasterizerState,
            DepthStencil = _depthStencilState,
            Blend = _blendState
        });

        _constantBuffer = Device.CreateBuffer(new()
        {
            Size = 64,
            Usage = BufferUsage.Uniform,
            MemoryUsage = MemoryUsage.Upload
        });

        _queue = Device.CreateCommandQueue();
        _commandBuffer = _queue.CreateCommandBuffer();

        ((D3D11CommandBuffer)_commandBuffer).RenderTargetView = ((D3D11Swapchain)_swapchain).RenderTargetView;
        ((D3D11CommandBuffer)_commandBuffer).DepthStencilView = ((D3D11Texture)_depthTexture).DepthStencilView;

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

    public static void Render(ref Scene scene, WorldObject cameraObject)
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

        _commandBuffer.SetPipeline(_pipeline);

        ref var cameraTransform = ref scene.GetComponent<Transform>(cameraObject);
        ref var camera = ref scene.GetComponent<Camera>(cameraObject);

        var forward = Vector3.Transform(
            -Vector3.UnitZ,
            Quaternion.CreateFromYawPitchRoll(
                cameraTransform.Rotation.Y,
                cameraTransform.Rotation.X,
                0f));

        var view = Matrix4x4.CreateLookAt(
            cameraTransform.Position,
            cameraTransform.Position + forward,
            Vector3.UnitY);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            camera.FieldOfView,
            size.X / (float)size.Y,
            camera.NearPlane,
            camera.FarPlane);

        foreach (var (objectId, renderable) in scene.Renderables)
        {
            ref var transform = ref scene.GetComponent<Transform>(new WorldObject(objectId));

            var model =
                Matrix4x4.CreateScale(transform.Scale) *
                Matrix4x4.CreateRotationX(transform.Rotation.X) *
                Matrix4x4.CreateRotationY(transform.Rotation.Y) *
                Matrix4x4.CreateRotationZ(transform.Rotation.Z) *
                Matrix4x4.CreateTranslation(transform.Position);

            var mvp = model * view * projection;

            var mvpData = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref mvp, 1));

            _constantBuffer.Upload(mvpData);

            var gpuMesh = GetOrCreateMesh(renderable.Mesh);

            _commandBuffer.SetVertexBuffer(gpuMesh.VertexBuffer);
            _commandBuffer.SetIndexBuffer(gpuMesh.IndexBuffer);
            _commandBuffer.SetUniformBuffer(_constantBuffer);

            _commandBuffer.DrawIndexed((uint)renderable.Mesh.Indices.Length);
        }

        _commandBuffer.End();

        _swapchain.Present();
    }

    public static void Dispose()
    {
        _depthTexture?.Dispose();
        _constantBuffer?.Dispose();
        _vertexShader?.Dispose();
        _fragmentShader?.Dispose();
        _vertexLayout?.Dispose();
        _rasterizerState?.Dispose();
        _depthStencilState?.Dispose();
        _blendState?.Dispose();
        _pipeline?.Dispose();
        _commandBuffer?.Dispose();
        _queue?.Dispose();
        _swapchain?.Dispose();
    }

    private sealed class GPUMesh(IBuffer vertexBuffer, IBuffer indexBuffer) : IDisposable
    {
        public IBuffer VertexBuffer { get; } = vertexBuffer;
        public IBuffer IndexBuffer { get; } = indexBuffer;

        public void Dispose()
        {
            VertexBuffer.Dispose();
            IndexBuffer.Dispose();
        }
    }
}
