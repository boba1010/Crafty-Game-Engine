using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.Environment;

public static class SkyRenderer
{
    private const uint VertexStride = 20;

    private static IBuffer _vertexBuffer = null!;
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

            float3 horizon = float3(0.55, 0.75, 0.95);
            float3 top = float3(0.18, 0.42, 0.85);

            float3 color = lerp(horizon, top, t);

            return float4(color, 1.0);
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
    }

    internal static void Render(ICommandBuffer commandBuffer, Vector3 cameraPosition, Matrix4x4 view, Matrix4x4 projection)
    {
        commandBuffer.SetPipeline(_pipeline);
        commandBuffer.SetVertexBuffer(_vertexBuffer, VertexStride);
        commandBuffer.Draw(6);

        SunRenderer.Render(commandBuffer, cameraPosition, view, projection);
    }

    public static void Dispose()
    {
        _pipeline?.Dispose();
        _blendState?.Dispose();
        _rasterizerState?.Dispose();
        _vertexLayout?.Dispose();
        _vertexShader?.Dispose();
        _fragmentShader?.Dispose();
        _vertexBuffer?.Dispose();
        _depthStencilState?.Dispose();

        SunRenderer.Dispose();
    }
}
