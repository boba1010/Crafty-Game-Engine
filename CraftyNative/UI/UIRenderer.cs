using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.UI;

public struct UIVertex(Vector2 position, Vector4 color)
{
    public Vector2 Position = position;
    public Vector4 Color = color;
}

public static class UIRenderer
{
    private static IBuffer _vertexBuffer = null!;
    private static IShader _vertexShader = null!;
    private static IShader _fragmentShader = null!;
    private static IVertexLayout _vertexLayout = null!;
    private static IRasterizerState _rasterizerState = null!;
    private static IDepthStencilState _depthStencilState = null!;
    private static IBlendState _blendState = null!;
    private static IPipeline _pipeline = null!;
    private static readonly List<UIVertex> _vertices = [];
    private static Vector2 _screenSize;

    public static void Initialize(IGraphicsDevice device)
    {
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

        _vertexBuffer = device.CreateBuffer(new()
        {
            Size = 256 * 6 * (uint)Marshal.SizeOf<UIVertex>(),
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
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
                    Offset = 8
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
            Enable = true,

            SourceColor = BlendFactor.SourceAlpha,
            DestinationColor = BlendFactor.InverseSourceAlpha,
            ColorOperation = BlendOperation.Add,

            SourceAlpha = BlendFactor.One,
            DestinationAlpha = BlendFactor.InverseSourceAlpha,
            AlphaOperation = BlendOperation.Add
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

    public static void DrawRectangle(Vector2 position, Vector2 size, Vector4 color)
    {
        float left = position.X / _screenSize.X * 2f - 1f;
        float right = (position.X + size.X) / _screenSize.X * 2f - 1f;
        float top = 1f - position.Y / _screenSize.Y * 2f;
        float bottom = 1f - (position.Y + size.Y) / _screenSize.Y * 2f;

        _vertices.Add(new(new(left, top), color));
        _vertices.Add(new(new(right, top), color));
        _vertices.Add(new(new(left, bottom), color));

        _vertices.Add(new(new(left, bottom), color));
        _vertices.Add(new(new(right, top), color));
        _vertices.Add(new(new(right, bottom), color));
    }

    public static void Render(ICommandBuffer commandBuffer, Vector2 size)
    {
        _screenSize = size;

        if (_vertices.Count == 0)
            return;

        commandBuffer.SetPipeline(_pipeline);

        var data = MemoryMarshal.AsBytes(_vertices.AsSpan());
        _vertexBuffer.Upload(data);

        commandBuffer.SetVertexBuffer(_vertexBuffer, sizeof(float) * 6);
        commandBuffer.Draw((uint)_vertices.Count);

        _vertices.Clear();
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
