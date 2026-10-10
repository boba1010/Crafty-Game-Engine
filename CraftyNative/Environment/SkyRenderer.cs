using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.Environment;

public static class SkyRenderer
{
    public static Vector4 TopColor { get; set; } = new(0.18f, 0.42f, 0.85f, 1f);
    public static Vector4 HorizonColor { get; set; } = new(0.55f, 0.75f, 0.95f, 1f);

    private const uint VertexStride = 20;

    private static IBuffer _vertexBuffer = null!;
    private static IBuffer _constantBuffer = null!;
    private static IShader _vertexShader = null!;
    private static IShader _fragmentShader = null!;
    private static IVertexLayout _vertexLayout = null!;
    private static IRasterizerState _rasterizerState = null!;
    private static IBlendState _blendState = null!;
    private static IPipeline _pipeline = null!;
    private static IDepthStencilState _depthStencilState = null!;

    internal static void Initialize(IGraphicsDevice device)
    {
        float[] vertices =
        [
            -1f, -1f, 1f, 0f, 1f,
            -1f,  1f, 1f, 0f, 0f,
             1f,  1f, 1f, 1f, 0f,
            -1f, -1f, 1f, 0f, 1f,
             1f,  1f, 1f, 1f, 0f,
             1f, -1f, 1f, 1f, 1f
        ];

        _vertexBuffer = device.CreateBuffer(new()
        {
            Size = (ulong)(vertices.Length * sizeof(float)),
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });

        _vertexBuffer.Upload(MemoryMarshal.AsBytes(vertices.AsSpan()));

        _constantBuffer = device.CreateBuffer(new()
        {
            Size = 32,
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

        cbuffer SkyConstants : register(b0)
        {
            float4 TopColor;
            float4 HorizonColor;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = float4(input.Position, 1.0);
            output.UV = input.UV;
            return output;
        }

        float4 PSMain(VSOutput input) : SV_Target
        {
            float t = saturate(1.0 - input.UV.y);
            return lerp(HorizonColor, TopColor, t);
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
            DepthClipEnable = false
        });

        _blendState = device.CreateBlendState(new()
        {
            Enable = false
        });

        _depthStencilState = device.CreateDepthStencilState(new()
        {
            DepthTestEnable = false,
            DepthWriteEnable = false
        });

        _pipeline = device.CreatePipeline(new()
        {
            VertexShader = _vertexShader,
            FragmentShader = _fragmentShader,
            VertexLayout = _vertexLayout,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            Rasterizer = _rasterizerState,
            Blend = _blendState,
            DepthStencil = _depthStencilState
        });

        SunRenderer.Initialize(device);
        MoonRenderer.Initialize(device);
    }

    internal static void Render(ICommandBuffer commandBuffer, Vector3 cameraPosition, Matrix4x4 view, Matrix4x4 projection)
    {
        var constants = new SkyConstants
        {
            TopColor = TopColor,
            HorizonColor = HorizonColor
        };

        _constantBuffer.Upload(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref constants, 1)));

        commandBuffer.SetPipeline(_pipeline);
        commandBuffer.SetVertexBuffer(_vertexBuffer, VertexStride);
        commandBuffer.SetUniformBuffer(_constantBuffer, 0);
        commandBuffer.Draw(6);

        SunRenderer.Render(commandBuffer, cameraPosition, view, projection);
        MoonRenderer.Render(commandBuffer, cameraPosition, view, projection);
    }

    public static void Dispose()
    {
        _pipeline?.Dispose();
        _blendState?.Dispose();
        _rasterizerState?.Dispose();
        _vertexLayout?.Dispose();
        _vertexShader?.Dispose();
        _fragmentShader?.Dispose();
        _constantBuffer?.Dispose();
        _vertexBuffer?.Dispose();
        _depthStencilState?.Dispose();

        SunRenderer.Dispose();
    }

    private struct SkyConstants
    {
        public Vector4 TopColor;
        public Vector4 HorizonColor;
    }
}
