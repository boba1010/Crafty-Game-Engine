using CraftyNative.ThreeD;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.HUD;

public static class HudRenderer
{
    private static IBuffer _vertexBuffer = null!;
    private static IShader _vertexShader = null!;
    private static IShader _fragmentShader = null!;
    private static IVertexLayout _vertexLayout = null!;
    private static IRasterizerState _rasterizerState = null!;
    private static IDepthStencilState _depthStencilState = null!;
    private static IBlendState _blendState = null!;
    private static IPipeline _pipeline = null!;

    public static void Initialize(IGraphicsDevice device, Vector2 size)
    {
        float thicknessX = 2f / size.X;
        float thicknessY = 2f / size.Y;

        float halfWidth = 20f / size.X;
        float halfHeight = 20f / size.Y;

        float[] vertices =
        [
            -halfWidth, -thicknessY,
             halfWidth, -thicknessY,
             halfWidth,  thicknessY,

            -halfWidth, -thicknessY,
             halfWidth,  thicknessY,
            -halfWidth,  thicknessY,

            -thicknessX, -halfHeight,
             thicknessX, -halfHeight,
             thicknessX,  halfHeight,

            -thicknessX, -halfHeight,
             thicknessX,  halfHeight,
            -thicknessX,  halfHeight
        ];

        _vertexBuffer = device.CreateBuffer(new()
        {
            Size = (ulong)(vertices.Length * sizeof(float)),
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });

        _vertexBuffer.Upload(
            MemoryMarshal.AsBytes(vertices.AsSpan()));

        var shaderSource = """
        struct VSInput
        {
            float2 Position : POSITION;
        };

        struct VSOutput
        {
            float4 Position : SV_Position;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = float4(input.Position, 0.0, 1.0);
            return output;
        }

        float4 PSMain(VSOutput input) : SV_Target
        {
            return float4(0, 0, 0, 0);
        }
        """;

        var vertexShaderCode =
            Shaders.CompileShader(shaderSource, "VSMain", "vs_5_0");

        var fragmentShaderCode =
            Shaders.CompileShader(shaderSource, "PSMain", "ps_5_0");

        _vertexShader = device.CreateShader(new()
        {
            Code = vertexShaderCode,
            Stage = ShaderStage.Vertex,
            EntryPoint = "VSMain"
        });

        _fragmentShader = device.CreateShader(new()
        {
            Code = fragmentShaderCode,
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
                    Format = TextureFormat.R32G32Float,
                    Offset = 0
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
            DepthTestEnable = false,
            DepthWriteEnable = false
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
    }

    public static void Render(ICommandBuffer commandBuffer, Vector2 size)
    {
        commandBuffer.SetPipeline(_pipeline);
        commandBuffer.SetVertexBuffer(_vertexBuffer, sizeof(float) * 2);

        commandBuffer.Draw(12);
    }
}