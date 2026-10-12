using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.UI;

public struct UITextVertex(Vector2 position, Vector2 uv, Vector4 color)
{
    public Vector2 Position = position;
    public Vector2 UV = uv;
    public Vector4 Color = color;
}

public sealed class UIFont
{
    public required ITexture Atlas { get; init; }
    public required Dictionary<char, Glyph> Glyphs { get; init; }
    public float Ascent { get; init; }
    public float Descent { get; init; }
}

public readonly struct Glyph(Vector2 uvPosition, Vector2 uvSize, Vector2 size, Vector2 bearing, float advance)
{
    public readonly Vector2 UVPosition = uvPosition;
    public readonly Vector2 UVSize = uvSize;
    public readonly Vector2 Size = size;
    public readonly Vector2 Bearing = bearing;
    public readonly float Advance = advance;
}

public static class UITextRenderer
{
    private static IBuffer _textVertexBuffer = null!;
    private static IShader _textVertexShader = null!;
    private static IShader _textFragmentShader = null!;
    private static IVertexLayout _textVertexLayout = null!;
    private static IRasterizerState _textRasterizerState = null!;
    private static IDepthStencilState _textDepthStencilState = null!;
    private static IBlendState _textBlendState = null!;
    private static IPipeline _textPipeline = null!;
    private static ISampler _textSampler = null!;
    private static Vector2 _screenSize;

    private static UIFont _font = null!;

    private readonly record struct TextBatch(int StartVertex, int VertexCount, Scissor Scissor);

    // Normal text queue.
    private static readonly List<UITextVertex> _textVertices = [];
    private static readonly List<TextBatch> _textBatches = [];

    private static int _batchStartVertex;
    private static Scissor _activeScissor;
    private static bool _batchInitialized;

    // Overlay text queue. Always rendered with a full-window scissor.
    private static readonly List<UITextVertex> _overlayVertices = [];

    public static void Initialize(IGraphicsDevice device, UIFont font)
    {
        _font = font;

        var shaderSource = """
        Texture2D FontTexture : register(t0);
        SamplerState FontSampler : register(s0);

        struct VSInput
        {
            float2 Position : POSITION;
            float2 UV : TEXCOORD;
            float4 Color : COLOR;
        };

        struct VSOutput
        {
            float4 Position : SV_Position;
            float2 UV : TEXCOORD;
            float4 Color : COLOR;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = float4(input.Position, 0.0, 1.0);
            output.UV = input.UV;
            output.Color = input.Color;
            return output;
        }

        float4 PSMain(VSOutput input) : SV_Target
        {
            float alpha = FontTexture.Sample(FontSampler, input.UV).r;
            return float4(input.Color.rgb, input.Color.a * alpha);
        }
        """;

        var vertexShaderCode =
            Shaders.CompileShader(shaderSource, "VSMain", "vs_5_0");

        var fragmentShaderCode =
            Shaders.CompileShader(shaderSource, "PSMain", "ps_5_0");

        _textVertexShader = device.CreateShader(new()
        {
            Code = vertexShaderCode,
            Stage = ShaderStage.Vertex,
            EntryPoint = "VSMain"
        });

        _textFragmentShader = device.CreateShader(new()
        {
            Code = fragmentShaderCode,
            Stage = ShaderStage.Fragment,
            EntryPoint = "PSMain"
        });

        _textVertexBuffer = device.CreateBuffer(new()
        {
            Size = 256 * 6 * (uint)Marshal.SizeOf<UITextVertex>(),
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });

        _textVertexLayout = device.CreateVertexLayout(new()
        {
            VertexShader = _textVertexShader,
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
                    Semantic = "TEXCOORD",
                    Location = 0,
                    Format = TextureFormat.R32G32Float,
                    Offset = 8
                },
                new()
                {
                    Semantic = "COLOR",
                    Location = 0,
                    Format = TextureFormat.R32G32B32A32Float,
                    Offset = 16
                }
            }
        });

        _textRasterizerState = device.CreateRasterizerState(new()
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Solid,
            DepthClipEnable = true,
            ScissorEnable = true
        });

        _textBlendState = device.CreateBlendState(new()
        {
            Enable = true,

            SourceColor = BlendFactor.SourceAlpha,
            DestinationColor = BlendFactor.InverseSourceAlpha,
            ColorOperation = BlendOperation.Add,

            SourceAlpha = BlendFactor.One,
            DestinationAlpha = BlendFactor.InverseSourceAlpha,
            AlphaOperation = BlendOperation.Add
        });

        _textDepthStencilState = device.CreateDepthStencilState(new()
        {
            DepthTestEnable = false,
            DepthWriteEnable = false
        });

        _textPipeline = device.CreatePipeline(new()
        {
            VertexShader = _textVertexShader,
            FragmentShader = _textFragmentShader,
            VertexLayout = _textVertexLayout,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            Rasterizer = _textRasterizerState,
            DepthStencil = _textDepthStencilState,
            Blend = _textBlendState
        });

        _textSampler = device.CreateSampler(new()
        {
            MinFilter = Filter.Linear,
            MagFilter = Filter.Linear,
            MipmapFilter = Filter.Linear,
            AddressU = AddressMode.ClampToEdge,
            AddressV = AddressMode.ClampToEdge,
            AddressW = AddressMode.ClampToEdge,
            MipLodBias = 0,
            MinLod = 0,
            MaxLod = float.MaxValue
        });
    }

    public static void DrawText(string text, Vector2 position, float scale, Vector4 color)
    {
        CaptureScissor();
        AppendText(_textVertices, text, position, scale, color);
    }

    public static void DrawOverlayText(string text, Vector2 position, float scale, Vector4 color)
    {
        AppendText(_overlayVertices, text, position, scale, color);
    }

    public static void DrawTextCentered(string text, Vector2 position, Vector2 size, float scale, Vector4 color)
    {
        DrawCenteredText(text, position, size, scale, color, overlay: false);
    }

    public static void DrawTextTopLeft(string text, Vector2 position, float scale, Vector4 color)
    {
        float baseline = position.Y + _font.Ascent * scale;

        DrawText(text, new Vector2(position.X, baseline), scale, color);
    }

    public static void DrawOverlayTextCentered(string text, Vector2 position, Vector2 size, float scale, Vector4 color)
    {
        DrawCenteredText(text, position, size, scale, color, overlay: true);
    }

    private static void DrawCenteredText(string text, Vector2 position, Vector2 size, float scale, Vector4 color, bool overlay)
    {
        var textSize = MeasureText(text, scale);

        float x = position.X + (size.X - textSize.X) / 2f;
        float textHeight = (_font.Ascent - _font.Descent) * scale;

        float baseline = position.Y + (size.Y - textHeight) / 2f + _font.Ascent * scale;

        var textPosition = new Vector2(x, baseline);

        if (overlay)
            DrawOverlayText(text, textPosition, scale, color);
        else
            DrawText(text, textPosition, scale, color);
    }

    private static void AppendText(List<UITextVertex> vertices, string text, Vector2 position, float scale, Vector4 color)
    {
        Vector2 cursor = position;

        foreach (char character in text)
        {
            if (!_font.Glyphs.TryGetValue(character, out var glyph))
                continue;

            var glyphPosition = cursor + glyph.Bearing * scale;
            var glyphSize = glyph.Size * scale;

            float left = glyphPosition.X / _screenSize.X * 2f - 1f;

            float right = (glyphPosition.X + glyphSize.X) / _screenSize.X * 2f - 1f;

            float top = 1f - glyphPosition.Y / _screenSize.Y * 2f;

            float bottom = 1f - (glyphPosition.Y + glyphSize.Y) / _screenSize.Y * 2f;

            var uvTopLeft = glyph.UVPosition;
            var uvTopRight = glyph.UVPosition + new Vector2(glyph.UVSize.X, 0);

            var uvBottomLeft = glyph.UVPosition + new Vector2(0, glyph.UVSize.Y);

            var uvBottomRight = glyph.UVPosition + glyph.UVSize;

            vertices.Add(new(new(left, top), uvTopLeft, color));
            vertices.Add(new(new(right, top), uvTopRight, color));
            vertices.Add(new(new(left, bottom), uvBottomLeft, color));

            vertices.Add(new(new(left, bottom), uvBottomLeft, color));
            vertices.Add(new(new(right, top), uvTopRight, color));
            vertices.Add(new(new(right, bottom), uvBottomRight, color));

            cursor.X += glyph.Advance * scale;
        }
    }

    public static Vector2 MeasureText(string text, float scale)
    {
        Vector2 size = Vector2.Zero;

        foreach (char character in text)
        {
            if (!_font.Glyphs.TryGetValue(character, out var glyph))
                continue;

            size.X += glyph.Advance * scale;
            size.Y = MathF.Max(size.Y, glyph.Size.Y * scale);
        }

        return size;
    }

    public static void Render(ICommandBuffer commandBuffer, Vector2 size)
    {
        _screenSize = size;

        if (_textVertices.Count == 0)
        {
            _textBatches.Clear();
            _batchStartVertex = 0;
            _batchInitialized = false;
            return;
        }

        CaptureScissor();

        int remaining = _textVertices.Count - _batchStartVertex;

        if (remaining > 0)
            _textBatches.Add(new TextBatch(_batchStartVertex, remaining, _activeScissor));

        commandBuffer.SetPipeline(_textPipeline);
        commandBuffer.SetTexture(_font.Atlas);
        commandBuffer.SetSampler(_textSampler);

        var data = MemoryMarshal.AsBytes(_textVertices.AsSpan());
        _textVertexBuffer.Upload(data);

        commandBuffer.SetVertexBuffer(_textVertexBuffer, (uint)Marshal.SizeOf<UITextVertex>());

        foreach (TextBatch batch in _textBatches)
        {
            commandBuffer.SetScissor(batch.Scissor);
            commandBuffer.Draw((uint)batch.VertexCount, firstVertex: (uint)batch.StartVertex);
        }

        _textVertices.Clear();
        _textBatches.Clear();
        _batchStartVertex = 0;
        _batchInitialized = false;
    }

    public static void RenderOverlay(ICommandBuffer commandBuffer, Vector2 size)
    {
        _screenSize = size;

        if (_overlayVertices.Count == 0)
            return;

        commandBuffer.SetPipeline(_textPipeline);
        commandBuffer.SetTexture(_font.Atlas);
        commandBuffer.SetSampler(_textSampler);

        var data = MemoryMarshal.AsBytes(_overlayVertices.AsSpan());
        _textVertexBuffer.Upload(data);

        commandBuffer.SetVertexBuffer(_textVertexBuffer, (uint)Marshal.SizeOf<UITextVertex>());

        commandBuffer.SetScissor(new Scissor
        {
            X = 0,
            Y = 0,
            Width = (uint)size.X,
            Height = (uint)size.Y
        });

        commandBuffer.Draw((uint)_overlayVertices.Count, firstVertex: 0);

        _overlayVertices.Clear();
    }

    private static void CaptureScissor()
    {
        Scissor current = UIRenderer.CurrentScissor;

        if (_batchInitialized &&
            current.X == _activeScissor.X &&
            current.Y == _activeScissor.Y &&
            current.Width == _activeScissor.Width &&
            current.Height == _activeScissor.Height)
        {
            return;
        }

        int count = _textVertices.Count - _batchStartVertex;

        if (count > 0)
        {
            _textBatches.Add(new TextBatch(_batchStartVertex, count, _activeScissor));
            _batchStartVertex = _textVertices.Count;
        }

        _activeScissor = current;
        _batchInitialized = true;
    }

    public static void Dispose()
    {
        _font.Atlas?.Dispose();
        _textVertexBuffer?.Dispose();
        _textVertexShader?.Dispose();
        _textFragmentShader?.Dispose();
        _textVertexLayout?.Dispose();
        _textRasterizerState?.Dispose();
        _textDepthStencilState?.Dispose();
        _textBlendState?.Dispose();
        _textPipeline?.Dispose();
        _textSampler?.Dispose();
    }
}
