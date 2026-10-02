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

    private static float[] vertices = [];
    private const int FloatsPerVertex = 6; // x, y, r, g, b, a
    private const int SlotCount = 9;
    private const int SelectedSlot = 0; // which slot is highlighted

    public static void Initialize(IGraphicsDevice device, Vector2 size)
    {
        const float margin = 20f;
        const float width = 400f;
        const float height = 50f;
        const float radius = height / 2f;
        const int segments = 24;

        const float slotSize = 40f;

        float sx = 2f / size.X;
        float sy = 2f / size.Y;

        float halfWidth = width * 0.5f * sx;
        float halfHeight = height * 0.5f * sy;
        float radiusX = radius * sx;
        float radiusY = radius * sy;

        float centerY = -1f + halfHeight + margin * sy;

        float leftCenter = -halfWidth + radiusX;
        float rightCenter = halfWidth - radiusX;

        var list = new List<float>();

        void V(float x, float y, Vector4 c) => list.AddRange([x, y, c.X, c.Y, c.Z, c.W]);

        void Quad(float x0, float y0, float x1, float y1, Vector4 c)
        {
            V(x0, y0, c); V(x1, y0, c); V(x1, y1, c);
            V(x0, y0, c); V(x1, y1, c); V(x0, y1, c);
        }

        void Cap(float cx, float startAngle, Vector4 c)
        {
            for (int i = 0; i < segments; i++)
            {
                float a0 = startAngle + MathF.PI * i / segments;
                float a1 = startAngle + MathF.PI * (i + 1) / segments;

                V(cx, centerY, c);
                V(cx + MathF.Cos(a0) * radiusX, centerY + MathF.Sin(a0) * radiusY, c);
                V(cx + MathF.Cos(a1) * radiusX, centerY + MathF.Sin(a1) * radiusY, c);
            }
        }

        void Circle(float cx, float cy, float rx, float ry, Vector4 c, int segs = 32)
        {
            for (int i = 0; i < segs; i++)
            {
                float a0 = MathF.PI * 2f * i / segs;
                float a1 = MathF.PI * 2f * (i + 1) / segs;

                V(cx, cy, c);
                V(cx + MathF.Cos(a0) * rx, cy + MathF.Sin(a0) * ry, c);
                V(cx + MathF.Cos(a1) * rx, cy + MathF.Sin(a1) * ry, c);
            }
        }

        var bgColor = new Vector4(0.05f, 0.05f, 0.05f, 0.9f);
        var slotColor = new Vector4(0.35f, 0.35f, 0.35f, 1f);
        var selectedColor = new Vector4(0.9f, 0.9f, 0.9f, 1f);

        // Pill background
        Quad(leftCenter, centerY - halfHeight, rightCenter, centerY + halfHeight, bgColor);
        Cap(leftCenter, MathF.PI * 0.5f, bgColor);
        Cap(rightCenter, -MathF.PI * 0.5f, bgColor);

        // Slots, spread across the whole pill
        float slotHalfX = slotSize * 0.5f * sx;
        float slotHalfY = slotSize * 0.5f * sy;

        for (int i = 0; i < SlotCount; i++)
        {
            float t = i / (float)(SlotCount - 1);
            float cx = leftCenter + (rightCenter - leftCenter) * t;
            var color = i == SelectedSlot ? selectedColor : slotColor;

            Circle(cx, centerY, slotHalfX, slotHalfY, color);
        }

        vertices = [.. list];

        _vertexBuffer = device.CreateBuffer(new()
        {
            Size = (ulong)(vertices.Length * sizeof(float)),
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });

        _vertexBuffer.Upload(MemoryMarshal.AsBytes(vertices.AsSpan()));

        var shaderSource = """
        struct VSInput
        {
            float2 Position : POSITION;
            float4 Color : COLOR;
        };

        struct VSOutput
        {
            float4 Position : SV_Position;
            float4 Color : COLOR;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = float4(input.Position, 0.0, 1.0);
            output.Color = input.Color;
            return output;
        }

        float4 PSMain(VSOutput input) : SV_Target
        {
            return input.Color;
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
                },
                new()
                {
                    Semantic = "COLOR",
                    Location = 0,
                    Format = TextureFormat.R32G32B32A32Float,
                    Offset = sizeof(float) * 2
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

    public static void Render(ICommandBuffer commandBuffer)
    {
        commandBuffer.SetPipeline(_pipeline);
        commandBuffer.SetVertexBuffer(_vertexBuffer, sizeof(float) * FloatsPerVertex);

        commandBuffer.Draw((uint)(vertices.Length / FloatsPerVertex));
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