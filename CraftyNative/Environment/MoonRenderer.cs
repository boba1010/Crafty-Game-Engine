using StbImageSharp;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.Environment;

public static class MoonRenderer
{
    public static float MoonSize { get; set; } = 5f;
    public static bool MoonVisible { get; set; } = true;

    private const uint VertexStride = 20;
    private const float MoonDistance = 50f;

    private static IBuffer _vertexBuffer = null!;
    private static IBuffer _constantBuffer = null!;
    private static IShader _vertexShader = null!;
    private static IShader _fragmentShader = null!;
    private static IVertexLayout _vertexLayout = null!;
    private static IRasterizerState _rasterizerState = null!;
    private static IDepthStencilState _depthStencilState = null!;
    private static IBlendState _blendState = null!;
    private static IPipeline _pipeline = null!;
    private static ITexture _moonTexture = null!;
    private static ISampler _moonSampler = null!;

    internal static void Initialize(IGraphicsDevice device)
    {
        float[] vertices = new float[30];

        _vertexBuffer = device.CreateBuffer(new()
        {
            Size = (ulong)(vertices.Length * sizeof(float)),
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });

        _constantBuffer = device.CreateBuffer(new()
        {
            Size = 80,
            Usage = BufferUsage.Uniform,
            MemoryUsage = MemoryUsage.Upload
        });

        const string shaderSource = """
        struct VSInput
        {
            float3 Position : POSITION;
            float2 UV : TEXCOORD0;
        };

        struct VSOutput
        {
            float4 Position : SV_Position;
            float2 UV : TEXCOORD0;
        };

        cbuffer MoonConstants : register(b0)
        {
            row_major matrix MVP;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = mul(float4(input.Position, 1.0), MVP);
            output.UV = input.UV;
            return output;
        }

        Texture2D MoonTexture : register(t0);
        SamplerState MoonSampler : register(s0);

        float4 PSMain(VSOutput input) : SV_Target
        {
            return MoonTexture.Sample(MoonSampler, input.UV);
        }
        """;

        _vertexShader = device.CreateShader(new()
        {
            Code = Shaders.CompileShader(shaderSource, "VSMain", "vs_5_0"),
            Stage = ShaderStage.Vertex,
            EntryPoint = "VSMain"
        });

        _fragmentShader = device.CreateShader(new()
        {
            Code = Shaders.CompileShader(shaderSource, "PSMain", "ps_5_0"),
            Stage = ShaderStage.Fragment,
            EntryPoint = "PSMain"
        });

        _vertexLayout = device.CreateVertexLayout(new()
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
                }
            }
        });

        _rasterizerState = device.CreateRasterizerState(new()
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Solid,
            DepthClipEnable = true
        });

        _depthStencilState = device.CreateDepthStencilState(new()
        {
            DepthTestEnable = true,
            DepthWriteEnable = false,
            DepthCompare = CompareOperation.Less
        });

        _blendState = device.CreateBlendState(new()
        {
            Enable = false
        });

        _pipeline = device.CreatePipeline(new()
        {
            VertexShader = _vertexShader,
            FragmentShader = _fragmentShader,
            VertexLayout = _vertexLayout,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            Rasterizer = _rasterizerState,
            DepthStencil = _depthStencilState,
            Blend = _blendState
        });

        LoadMoonTexture(device, Path.Combine("Assets", "Textures", "Environment", "moon.png"));
    }

    private static void LoadMoonTexture(IGraphicsDevice device, string path)
    {
        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        _moonTexture = device.CreateTexture(new()
        {
            Width = (uint)image.Width,
            Height = (uint)image.Height,
            Depth = 1,
            MipLevels = 1,
            ArrayLayers = 1,
            Format = TextureFormat.R8G8B8A8Unorm,
            Usage = TextureUsage.Sampled,
            Type = TextureType.Texture2D,
            Samples = SampleCount.X1
        });

        _moonTexture.Upload(image.Data, (uint)(image.Width * 4));

        _moonSampler = device.CreateSampler(new()
        {
            MinFilter = Filter.Linear,
            MagFilter = Filter.Linear,
            MipmapFilter = Filter.Linear,
            AddressU = AddressMode.ClampToEdge,
            AddressV = AddressMode.ClampToEdge,
            AddressW = AddressMode.ClampToEdge,
            MinLod = 0,
            MaxLod = 0
        });
    }

    internal static void Render(ICommandBuffer commandBuffer, Vector3 cameraPosition, Matrix4x4 view, Matrix4x4 projection)
    {
        if (!MoonVisible)
            return;

        Vector3 direction = -SunRenderer.Direction;
        if (direction.LengthSquared() < 0.0001f)
            return;

        direction = Vector3.Normalize(direction);

        Vector3 referenceUp = MathF.Abs(Vector3.Dot(direction, Vector3.UnitY)) > 0.99f ? Vector3.UnitZ : Vector3.UnitY;

        Vector3 right = Vector3.Normalize(Vector3.Cross(direction, referenceUp));
        Vector3 up = Vector3.Normalize(Vector3.Cross(right, direction));

        Vector3 center = cameraPosition + direction * MoonDistance;
        float size = MathF.Max(0.01f, MoonSize);

        Vector3 bottomLeft = center - right * size - up * size;
        Vector3 topLeft = center - right * size + up * size;
        Vector3 topRight = center + right * size + up * size;
        Vector3 bottomRight = center + right * size - up * size;

        float[] vertices =
        [
            bottomLeft.X, bottomLeft.Y, bottomLeft.Z, 0f, 1f,
            topLeft.X, topLeft.Y, topLeft.Z, 0f, 0f,
            topRight.X, topRight.Y, topRight.Z, 1f, 0f,
            bottomLeft.X, bottomLeft.Y, bottomLeft.Z, 0f, 1f,
            topRight.X, topRight.Y, topRight.Z, 1f, 0f,
            bottomRight.X, bottomRight.Y, bottomRight.Z, 1f, 1f
        ];

        _vertexBuffer.Upload(MemoryMarshal.AsBytes(vertices.AsSpan()));

        var constants = new MoonConstants
        {
            MVP = view * projection,
        };

        _constantBuffer.Upload(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref constants, 1)));

        commandBuffer.SetPipeline(_pipeline);
        commandBuffer.SetVertexBuffer(_vertexBuffer, VertexStride);
        commandBuffer.SetUniformBuffer(_constantBuffer, 0);
        commandBuffer.SetSampler(_moonSampler, 0);

        commandBuffer.SetTexture(_moonTexture, 0);
        commandBuffer.Draw(6);
    }

    internal static void Dispose()
    {
        _pipeline?.Dispose();
        _blendState?.Dispose();
        _depthStencilState?.Dispose();
        _rasterizerState?.Dispose();
        _vertexLayout?.Dispose();
        _vertexShader?.Dispose();
        _fragmentShader?.Dispose();
        _moonSampler?.Dispose();
        _moonTexture?.Dispose();
        _constantBuffer?.Dispose();
        _vertexBuffer?.Dispose();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MoonConstants
    {
        public Matrix4x4 MVP;
    }
}
