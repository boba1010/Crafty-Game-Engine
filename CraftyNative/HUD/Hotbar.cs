using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.HUD;

public static class Hotbar
{
    private static IBuffer _vertexBuffer = null!;
    private static IShader _vertexShader = null!;
    private static IShader _fragmentShader = null!;
    private static IVertexLayout _vertexLayout = null!;
    private static IRasterizerState _rasterizerState = null!;
    private static IDepthStencilState _depthStencilState = null!;
    private static IBlendState _blendState = null!;
    private static IPipeline _pipeline = null!;

    private static List<float> vertices = [];

    public static void Initialize(IGraphicsDevice device, Vector2 size)
    {
        const float height = 50f;
        const float width = 500f;
        const float radius = height / 2f;   // full pill, like border-radius: 250px on a 50px box
        const int segments = 24;            // smoothness of each cap

        // pixels -> NDC (NDC spans 2 units across the screen)
        float sx = 2f / size.X;
        float sy = 2f / size.Y;

        float halfWidth = width * 0.5f * sx;
        float halfHeight = height * 0.5f * sy;
        float radiusX = radius * sx;
        float radiusY = radius * sy;

        float centerY = -1f + halfHeight + 0.05f;

        float leftCenter = -halfWidth + radiusX;
        float rightCenter = halfWidth - radiusX;

        // Main body (full height, between the two cap centers)
        vertices.AddRange(
        [
            leftCenter,  centerY - halfHeight,
            rightCenter, centerY - halfHeight,
            rightCenter, centerY + halfHeight,

            leftCenter,  centerY - halfHeight,
            rightCenter, centerY + halfHeight,
            leftCenter,  centerY + halfHeight,
        ]);

        // Semicircle cap as a triangle fan
        void AddCap(float cx, float startAngle)
        {
            for (int i = 0; i < segments; i++)
            {
                float a0 = startAngle + MathF.PI * i / segments;
                float a1 = startAngle + MathF.PI * (i + 1) / segments;

                vertices.AddRange(
                [
                    cx, centerY,
                    cx + MathF.Cos(a0) * radiusX, centerY + MathF.Sin(a0) * radiusY,
                    cx + MathF.Cos(a1) * radiusX, centerY + MathF.Sin(a1) * radiusY,
                ]);
            }
        }

        AddCap(leftCenter, MathF.PI * 0.5f);    // 90°  -> 270°
        AddCap(rightCenter, -MathF.PI * 0.5f);  // -90° -> 90°

        _vertexBuffer = device.CreateBuffer(new()
        {
            Size = (ulong)(vertices.Count * sizeof(float)),
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });

        _vertexBuffer.Upload(MemoryMarshal.AsBytes(vertices.AsSpan()));

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
            return float4(0.05, 0.05, 0.05, 0.9);
        }
        """;

        var vertexShaderCode = Shaders.CompileShader(shaderSource, "VSMain", "vs_5_0");

        var fragmentShaderCode = Shaders.CompileShader(shaderSource, "PSMain", "ps_5_0");

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
            Enable = true
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

    public static void Render(ICommandBuffer commandBuffer)
    {
        commandBuffer.SetPipeline(_pipeline);
        commandBuffer.SetVertexBuffer(_vertexBuffer, sizeof(float) * 2);

        commandBuffer.Draw((uint)(vertices.Count / 2));
    }

    public static void Dispose()
    {
        _vertexBuffer?.Dispose();
        _vertexShader?.Dispose();
        _fragmentShader?.Dispose();
        _vertexLayout?.Dispose();
        _rasterizerState?.Dispose();
        _depthStencilState?.Dispose();
        _blendState?.Dispose();
        _pipeline?.Dispose();
    }
}