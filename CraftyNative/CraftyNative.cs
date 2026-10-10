using CraftyNative.Environment;
using CraftyNative.HUD;
using CraftyNative.Scenes;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.UI;
using Silk.NET.Windowing;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.DirectX;
using Vulcan.Graphics;
using Vulcan.Graphics.Descriptions;

namespace CraftyNative;

public unsafe static class CraftyNative
{
    public static IWindow Window { get; private set; } = null!;
    public static IGraphicsDevice Device { get; private set; } = null!;
    public static float Daylight { get; set; } = 1f;
    public static float Moonlight { get; set; } = 1f;

    private static readonly Dictionary<Mesh, GPUMesh> _gpuMeshes = [];

    // Reused every frame so the translucent pass doesn't allocate
    private static readonly List<(float Dist, int Index)> _translucentOrder = [];

    private static ICommandBuffer _commandBuffer = null!;
    private static ISwapchain _swapchain = null!;
    private static ICommandQueue _queue = null!;
    private static IVertexLayout _vertexLayout = null!;
    private static IRasterizerState _rasterizerState = null!;
    private static IBuffer _constantBuffer = null!;
    private static ITexture _depthTexture = null!;
    private static ISampler _sampler = null!;

    private static IShader _vertexShader = null!;
    private static IShader _fragmentShader = null!;
    private static IShader _translucentFragmentShader = null!;

    // Opaque + alpha-tested (cutout) pipeline: no blending, depth writes on
    private static IPipeline _pipeline = null!;
    private static IDepthStencilState _depthStencilState = null!;
    private static IBlendState _blendState = null!;

    // Translucent pipeline (glass, ice): blending on, depth writes off
    private static IPipeline _translucentPipeline = null!;
    private static IDepthStencilState _translucentDepthState = null!;
    private static IRasterizerState _translucentRasterizerState = null!;
    private static IBlendState _alphaBlendState = null!;

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

    /// <summary>
    /// Frees the GPU buffers of a mesh that is being replaced or unloaded
    /// (e.g. a section that was rebuilt). Never call this for a mesh that is
    /// still being drawn this frame.
    /// </summary>
    public static void ReleaseMesh(Mesh? mesh)
    {
        if (mesh is null)
            return;

        if (_gpuMeshes.Remove(mesh, out var gpuMesh))
            gpuMesh.Dispose();
    }

    public static Texture CreateTexture(ImageData image)
    {
        var texture = Device.CreateTexture(new TextureDescription
        {
            Width = image.Width,
            Height = image.Height,
            Depth = 1,
            MipLevels = 1,
            ArrayLayers = 1,
            Format = TextureFormat.R8G8B8A8Unorm,
            Usage = TextureUsage.Sampled,
            Type = TextureType.Texture2D,
            Samples = SampleCount.X1
        });

        var rowPitch = image.Width * 4;

        texture.Upload(image.Pixels, rowPitch);

        return new Texture(texture);
    }

    public static Material CreateMaterial(ImageData atlas)
    {
        var atlasTexture = CreateTexture(atlas);

        return new Material
        {
            Atlas = atlasTexture
        };
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
            float2 UV : TEXCOORD0;
            float Light : TEXCOORD1;
        };

        struct VSOutput
        {
            float4 Position : SV_Position;
            float2 UV : TEXCOORD0;
            float Light : TEXCOORD1;
        };

        cbuffer Transform : register(b0)
        {
            row_major matrix MVP;
            float Daylight;
            float Moonlight;
            float3 Padding;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = mul(float4(input.Position, 1.0), MVP);
            output.UV = input.UV;
            output.Light = input.Light;
            return output;
        }

        Texture2D Texture : register(t0);
        SamplerState Sampler : register(s0);

        float4 PSMain(VSOutput input) : SV_Target
        {
            float4 color = Texture.Sample(Sampler, input.UV);
            clip(color.a - 0.5f);

            float ambient = 0.04f + saturate(Daylight) * 0.96f + saturate(Moonlight) * 0.12f;
            color.rgb *= saturate(input.Light) * saturate(ambient);

            return color;
        }

        float4 PSMainTranslucent(VSOutput input) : SV_Target
        {
            float4 color = Texture.Sample(Sampler, input.UV);
            clip(color.a - 0.004f);

            float ambient = 0.04f + saturate(Daylight) * 0.96f + saturate(Moonlight) * 0.12f;
            color.rgb *= saturate(input.Light) * saturate(ambient);

            return color;
        }
        """;

        var vertexShaderCode = Shaders.CompileShader(shaderSource, "VSMain", "vs_5_0");
        var fragmentShaderCode = Shaders.CompileShader(shaderSource, "PSMain", "ps_5_0");
        var translucentFragmentShaderCode = Shaders.CompileShader(shaderSource, "PSMainTranslucent", "ps_5_0");

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

        _translucentFragmentShader = Device.CreateShader(new()
        {
            Code = translucentFragmentShaderCode,
            Stage = ShaderStage.Fragment,
            EntryPoint = "PSMainTranslucent"
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
                },
                new()
                {
                    Semantic = "TEXCOORD",
                    Location = 0,
                    Format = TextureFormat.R32G32Float,
                    Offset = 12
                },
                new()
                {
                    Semantic = "TEXCOORD",
                    Location = 1,
                    Format = TextureFormat.R32Float,
                    Offset = 20
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

        // ---- opaque / cutout ----
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

        // ---- translucent ----
        _alphaBlendState = Device.CreateBlendState(new()
        {
            Enable = true,
            SourceColor = BlendFactor.SourceAlpha,
            DestinationColor = BlendFactor.InverseSourceAlpha,
            ColorOperation = BlendOperation.Add,
            SourceAlpha = BlendFactor.One,
            DestinationAlpha = BlendFactor.InverseSourceAlpha,
            AlphaOperation = BlendOperation.Add
        });

        _translucentRasterizerState = Device.CreateRasterizerState(new()
        {
            CullMode = CullMode.Back,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Solid,
            DepthClipEnable = true,
        });

        _translucentDepthState = Device.CreateDepthStencilState(new()
        {
            DepthTestEnable = true,
            DepthWriteEnable = false,   // glass must not hide what's drawn after it
            DepthCompare = CompareOperation.Less
        });

        _translucentPipeline = Device.CreatePipeline(new()
        {
            VertexShader = _vertexShader,
            FragmentShader = _translucentFragmentShader,
            VertexLayout = _vertexLayout,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            Rasterizer = _translucentRasterizerState,
            DepthStencil = _translucentDepthState,
            Blend = _alphaBlendState
        });

        _constantBuffer = Device.CreateBuffer(new()
        {
            Size = 80,
            Usage = BufferUsage.Uniform,
            MemoryUsage = MemoryUsage.Upload
        });

        _sampler = Device.CreateSampler(new()
        {
            MinFilter = Filter.Nearest,
            MagFilter = Filter.Nearest,
            MipmapFilter = Filter.Nearest,
            AddressU = AddressMode.Repeat,
            AddressV = AddressMode.Repeat,
            AddressW = AddressMode.Repeat,
            MipLodBias = 0,
            MinLod = 0,
            MaxLod = float.MaxValue
        });

        _queue = Device.CreateCommandQueue();
        _commandBuffer = _queue.CreateCommandBuffer();

        ((D3D11CommandBuffer)_commandBuffer).RenderTargetView = ((D3D11Swapchain)_swapchain).RenderTargetView;
        ((D3D11CommandBuffer)_commandBuffer).DepthStencilView = ((D3D11Texture)_depthTexture).DepthStencilView;

        Vector2 size = new(Window.Size.X, Window.Size.Y);

        SkyRenderer.Initialize(Device);
        HudRenderer.Initialize(Device, size);
        UIRenderer.Initialize(Device);
        UITextRenderer.Initialize(Device, FontLoader.Load(Device, "Assets/Fonts/Roboto-Regular.ttf", 32));

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

        ref var cameraTransform = ref scene.GetComponent<Transform>(cameraObject);
        ref var camera = ref scene.GetComponent<Camera>(cameraObject);

        var forward = Vector3.Transform(
            -Vector3.UnitZ,
            Quaternion.CreateFromYawPitchRoll(cameraTransform.Rotation.Y, cameraTransform.Rotation.X, 0f));

        var view = Matrix4x4.CreateLookAt(cameraTransform.Position, cameraTransform.Position + forward, Vector3.UnitY);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView(camera.FieldOfView, size.X / (float)size.Y, camera.NearPlane, camera.FarPlane);

        SkyRenderer.Render(_commandBuffer, cameraTransform.Position, view, projection);

        // ---- Pass 1: opaque + cutout (world sections, then entities/items) ----
        _commandBuffer.SetPipeline(_pipeline);

        var visibleSectionsCount = WorldMeshManager.GetVisibleSections(cameraTransform.Position, out var visibleSections);
        for (int i = 0; i < visibleSectionsCount; i++)
        {
            var section = visibleSections[i];
            var mesh = section!.Mesh;

            if (mesh.Vertices.Count <= 0 || mesh.Indices.Count <= 0)
                continue;

            var model = Matrix4x4.CreateTranslation(section.Coordinate.WorldPosition);
            DrawMesh(mesh, model * view * projection);
        }

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
                DrawMesh(renderable.Mesh, model * view * projection);
        }

        // ---- Pass 2: translucent sections (glass, ice), far to near ----
        _translucentOrder.Clear();

        for (int i = 0; i < visibleSectionsCount; i++)
        {
            var section = visibleSections[i]!;
            var translucentMesh = section.TranslucentMesh;

            if (translucentMesh is null || translucentMesh.Vertices.Count <= 0 || translucentMesh.Indices.Count <= 0)
                continue;

            var distance = Vector3.DistanceSquared(cameraTransform.Position, section.Coordinate.WorldPosition);
            _translucentOrder.Add((distance, i));
        }

        if (_translucentOrder.Count > 0)
        {
            _translucentOrder.Sort((a, b) => b.Dist.CompareTo(a.Dist));

            _commandBuffer.SetPipeline(_translucentPipeline);

            foreach (var (_, index) in _translucentOrder)
            {
                var section = visibleSections[index]!;
                var model = Matrix4x4.CreateTranslation(section.Coordinate.WorldPosition);
                DrawMesh(section.TranslucentMesh, model * view * projection);
            }
        }

        Vector2 vector2Size = new(size.X, size.Y);

        HudRenderer.Render(_commandBuffer, vector2Size);
        UIRenderer.Render(_commandBuffer, vector2Size);
        UITextRenderer.Render(_commandBuffer, vector2Size);

        _commandBuffer.End();

        _swapchain.Present();
    }

    private static void DrawMesh(Mesh mesh, Matrix4x4 mvp, bool applyWorldLighting = true)
    {
        var gpuMesh = GetOrCreateMesh(mesh);
        if (mesh.IsDirty)
        {
            UpdateVertexBuffer(gpuMesh, mesh);
            mesh.ClearDirty();
        }

        Span<byte> data = stackalloc byte[80];
        data.Clear();

        MemoryMarshal.Write(data, in mvp);

        float daylight = applyWorldLighting ? Daylight : 1f;
        MemoryMarshal.Write(data[64..], in daylight);

        float moonlight = applyWorldLighting ? Moonlight : 0f;
        MemoryMarshal.Write(data[68..], in moonlight);

        _constantBuffer.Upload(data);

        _commandBuffer.SetVertexBuffer(gpuMesh.VertexBuffer, mesh.VertexStride);

        _commandBuffer.SetIndexBuffer(gpuMesh.IndexBuffer);
        _commandBuffer.SetUniformBuffer(_constantBuffer);
        _commandBuffer.SetSampler(_sampler);

        var texture = mesh.Texture;

        if (texture is null)
        {
            _commandBuffer.DrawIndexed((uint)mesh.Indices.Count);
            return;
        }

        // One material per atlas, shared by every mesh that uses it
        var material = NativeMaterialsManager.Get(texture.Value);

        _commandBuffer.SetTexture(material.Atlas.Resource);
        _commandBuffer.DrawIndexed((uint)mesh.Indices.Count, 1, 0);
    }

    private static void UpdateVertexBuffer(GPUMesh gpuMesh, Mesh mesh)
    {
        var data = MemoryMarshal.AsBytes(mesh.Vertices.AsSpan());
        gpuMesh.VertexBuffer.Upload(data);
    }

    internal static void DrawHudMesh(Mesh mesh, Matrix4x4 mvp)
    {
        _commandBuffer.SetPipeline(_pipeline);
        DrawMesh(mesh, mvp, false);
    }

    public static void Dispose()
    {
        SkyRenderer.Dispose();
        HudRenderer.Dispose();
        UIRenderer.Dispose();
        UITextRenderer.Dispose();

        NativeMaterialsManager.Clear();

        foreach (var gpuMesh in _gpuMeshes.Values)
            gpuMesh.Dispose();
        _gpuMeshes.Clear();

        _translucentPipeline?.Dispose();
        _translucentDepthState?.Dispose();
        _alphaBlendState?.Dispose();

        _pipeline?.Dispose();
        _depthStencilState?.Dispose();
        _blendState?.Dispose();

        _depthTexture?.Dispose();
        _constantBuffer?.Dispose();
        _vertexShader?.Dispose();
        _fragmentShader?.Dispose();
        _translucentFragmentShader?.Dispose();
        _vertexLayout?.Dispose();
        _rasterizerState?.Dispose();
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