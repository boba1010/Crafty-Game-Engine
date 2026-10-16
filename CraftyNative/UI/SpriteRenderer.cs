using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;
using Vulcan.Graphics.Descriptions;

namespace CraftyNative.UI;

public static class SpriteRenderer
{
    private struct SpriteVertex(Vector2 position, Vector4 color, Vector4 clip, Vector2 uv)
    {
        public Vector2 Position = position;
        public Vector4 Color = color;
        public Vector4 Clip = clip;
        public Vector2 UV = uv;
    }

    // Mutable so consecutive quads with the same texture/scissor can extend the last batch in place
    private struct SpriteBatch
    {
        public int StartVertex;
        public int VertexCount;
        public ITexture Texture;
        public Scissor Scissor;
    }

    private readonly record struct TextureKey(byte[] Pixels, uint Width, uint Height);

    private sealed class TextureKeyComparer : IEqualityComparer<TextureKey>
    {
        public bool Equals(TextureKey x, TextureKey y) =>
            ReferenceEquals(x.Pixels, y.Pixels) && x.Width == y.Width && x.Height == y.Height;

        public int GetHashCode(TextureKey key) =>
            HashCode.Combine(RuntimeHelpers.GetHashCode(key.Pixels), key.Width, key.Height);
    }

    private static IBuffer _vertexBuffer = null!;
    private static IShader _vertexShader = null!;
    private static IShader _fragmentShader = null!;
    private static IVertexLayout _vertexLayout = null!;
    private static IRasterizerState _rasterizerState = null!;
    private static IDepthStencilState _depthStencilState = null!;
    private static IBlendState _blendState = null!;
    private static IPipeline _pipeline = null!;
    private static ISampler _sampler = null!;
    private static IGraphicsDevice _device = null!;

    private static readonly List<SpriteVertex> _vertices = [];
    private static readonly List<SpriteBatch> _batches = [];

    private static readonly Dictionary<TextureKey, ITexture> _textureCache = new(new TextureKeyComparer());

    // Always current, so Draw works at any point in the frame (including before the first BeginFrame)
    private static Vector2 ScreenSize => SystemAPI.WindowSize;
    private static uint _vertexBufferCapacity = 256 * 6;
    private static readonly uint _vertexStride = (uint)Marshal.SizeOf<SpriteVertex>();

    private static Scissor ActiveScissor => CreateScissor(0, 0, ScreenSize.X, ScreenSize.Y);
    private static bool _initialized;

    public static void Initialize(IGraphicsDevice device)
    {
        if (_initialized)
            throw new InvalidOperationException("SpriteRenderer is already initialized.");

        _device = device;

        const string shaderSource = """
        Texture2D SpriteTexture : register(t0);
        SamplerState SpriteSampler : register(s0);

        struct VSInput
        {
            float2 Position : POSITION;
            float4 Color : COLOR;
            float4 Clip : TEXCOORD0;
            float2 UV : TEXCOORD1;
        };

        struct VSOutput
        {
            float4 Position : SV_Position;
            float4 Color : COLOR;
            nointerpolation float4 Clip : TEXCOORD0;
            float2 UV : TEXCOORD1;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = float4(input.Position, 0.0, 1.0);
            output.Color = input.Color;
            output.Clip = input.Clip;
            output.UV = input.UV;
            return output;
        }

        float4 PSMain(VSOutput input) : SV_Target
        {
            clip(float4(
                input.Position.x - input.Clip.x,
                input.Position.y - input.Clip.y,
                input.Clip.z - input.Position.x,
                input.Clip.w - input.Position.y));

            return SpriteTexture.Sample(SpriteSampler, input.UV)
                * input.Color;
        }
        """;

        byte[] vertexShaderCode = Shaders.CompileShader(shaderSource, "VSMain", "vs_5_0");
        byte[] fragmentShaderCode = Shaders.CompileShader(shaderSource, "PSMain", "ps_5_0");

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
            Size = _vertexBufferCapacity * _vertexStride,
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
                },
                new()
                {
                    Semantic = "TEXCOORD",
                    Location = 0,
                    Format = TextureFormat.R32G32B32A32Float,
                    Offset = 24
                },
                new()
                {
                    Semantic = "TEXCOORD",
                    Location = 1,
                    Format = TextureFormat.R32G32Float,
                    Offset = 40
                }
            }
        });

        _rasterizerState = device.CreateRasterizerState(new()
        {
            CullMode = CullMode.None,
            FrontFace = FrontFace.CounterClockwise,
            FillMode = FillMode.Solid,
            DepthClipEnable = true,
            ScissorEnable = true
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

        _sampler = device.CreateSampler(new SamplerDescription
        {
            MinFilter = Filter.Nearest,
            MagFilter = Filter.Nearest,
            MipmapFilter = Filter.Nearest,

            AddressU = AddressMode.ClampToEdge,
            AddressV = AddressMode.ClampToEdge,
            AddressW = AddressMode.ClampToEdge,

            MipLodBias = 0f,
            MinLod = 0f,
            MaxLod = 0f
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

        _initialized = true;
    }

    public static void BeginFrame(Vector2 screenSize)
    {
        // Intentionally empty. Screen size comes from SystemAPI.WindowSize, and the queue is NOT
        // cleared here because the UI tree may call Draw before BeginFrame. Render() clears it.
        // Kept only so existing call sites still compile; you can delete it and its call.
        EnsureInitialized();
    }

    public static void Draw(
        byte[] pixels, uint imageWidth, uint imageHeight,
        Vector2 position, Vector2 size,
        Vector4 tint, Vector2 uvMin, Vector2 uvMax)
    {
        EnsureInitialized();

        if (pixels is null || imageWidth == 0 || imageHeight == 0 || size.X <= 0 || size.Y <= 0 || ScreenSize.X <= 0 || ScreenSize.Y <= 0)
            return;

        long requiredBytes = (long)imageWidth * imageHeight * 4;

        if (requiredBytes > pixels.LongLength)
            throw new ArgumentException("Pixel buffer is smaller than the specified RGBA texture dimensions.", nameof(pixels));

        ITexture texture = GetOrCreateTexture(pixels, imageWidth, imageHeight);

        Vector4 clip = new(0, 0, ScreenSize.X, ScreenSize.Y);

        float left = position.X / ScreenSize.X * 2f - 1f;
        float right = (position.X + size.X) / ScreenSize.X * 2f - 1f;
        float top = 1f - position.Y / ScreenSize.Y * 2f;
        float bottom = 1f - (position.Y + size.Y) / ScreenSize.Y * 2f;

        Vector2 topLeft = new(uvMin.X, uvMin.Y);
        Vector2 topRight = new(uvMax.X, uvMin.Y);
        Vector2 bottomLeft = new(uvMin.X, uvMax.Y);
        Vector2 bottomRight = new(uvMax.X, uvMax.Y);

        AppendQuad(
            texture,
            new SpriteVertex(new(left, top), tint, clip, topLeft),
            new SpriteVertex(new(right, top), tint, clip, topRight),
            new SpriteVertex(new(left, bottom), tint, clip, bottomLeft),
            new SpriteVertex(new(right, bottom), tint, clip, bottomRight));
    }

    public static void Render(ICommandBuffer commandBuffer)
    {
        EnsureInitialized();

        if (_vertices.Count == 0 || _batches.Count == 0)
        {
            _vertices.Clear();
            _batches.Clear();
            return;
        }

        commandBuffer.SetPipeline(_pipeline);

        EnsureVertexBufferCapacity((uint)_vertices.Count);

        _vertexBuffer.Upload(MemoryMarshal.AsBytes(_vertices.AsSpan()));

        commandBuffer.SetVertexBuffer(_vertexBuffer, _vertexStride);
        commandBuffer.SetSampler(_sampler, 0);

        foreach (SpriteBatch batch in _batches)
        {
            commandBuffer.SetTexture(batch.Texture, 0);
            commandBuffer.SetScissor(batch.Scissor);

            commandBuffer.Draw((uint)batch.VertexCount, firstVertex: (uint)batch.StartVertex);
        }

        commandBuffer.SetScissor(CreateScissor(0, 0, ScreenSize.X, ScreenSize.Y));

        _vertices.Clear();
        _batches.Clear();
    }

    /// <summary>
    /// Adds one quad (two triangles) and attaches it to a batch in the same step,
    /// so vertices and batches can never get out of sync.
    /// Corner order: a = top-left, b = top-right, c = bottom-left, d = bottom-right.
    /// </summary>
    private static void AppendQuad(ITexture texture, in SpriteVertex a, in SpriteVertex b, in SpriteVertex c, in SpriteVertex d)
    {
        int start = _vertices.Count;

        // Triangle 1
        _vertices.Add(a);
        _vertices.Add(b);
        _vertices.Add(c);

        // Triangle 2
        _vertices.Add(c);
        _vertices.Add(b);
        _vertices.Add(d);

        // Extend the previous batch when texture and scissor match
        if (_batches.Count > 0)
        {
            ref SpriteBatch last = ref CollectionsMarshal.AsSpan(_batches)[^1];

            if (ReferenceEquals(last.Texture, texture) && SameScissor(last.Scissor, ActiveScissor))
            {
                last.VertexCount += 6;
                return;
            }
        }

        _batches.Add(new SpriteBatch
        {
            StartVertex = start,
            VertexCount = 6,
            Texture = texture,
            Scissor = ActiveScissor
        });
    }

    private static bool SameScissor(Scissor a, Scissor b) =>
        a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;

    private static ITexture GetOrCreateTexture(byte[] pixels, uint width, uint height)
    {
        TextureKey key = new(pixels, width, height);

        if (_textureCache.TryGetValue(key, out ITexture? texture))
            return texture;

        texture = _device.CreateTexture(new TextureDescription
        {
            Width = width,
            Height = height,
            Depth = 1,

            MipLevels = 1,
            ArrayLayers = 1,

            Format = TextureFormat.R8G8B8A8Unorm,
            Usage = TextureUsage.Sampled,
            Type = TextureType.Texture2D,

            Samples = SampleCount.X1
        });

        texture.Upload(pixels, width * 4);

        _textureCache.Add(key, texture);

        return texture;
    }

    private static void EnsureVertexBufferCapacity(uint required)
    {
        if (required <= _vertexBufferCapacity)
            return;

        uint capacity = _vertexBufferCapacity;

        while (capacity < required)
            capacity = checked(capacity * 2);

        IBuffer newBuffer = _device.CreateBuffer(new()
        {
            Size = capacity * _vertexStride,
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });

        _vertexBuffer.Dispose();
        _vertexBuffer = newBuffer;
        _vertexBufferCapacity = capacity;
    }

    private static Scissor CreateScissor(float x, float y, float width, float height)
    {
        int left = (int)MathF.Floor(Math.Clamp(x, 0, ScreenSize.X));
        int top = (int)MathF.Floor(Math.Clamp(y, 0, ScreenSize.Y));
        int right = (int)MathF.Ceiling(Math.Clamp(x + width, 0, ScreenSize.X));
        int bottom = (int)MathF.Ceiling(Math.Clamp(y + height, 0, ScreenSize.Y));

        return new Scissor
        {
            X = left,
            Y = top,
            Width = (uint)Math.Max(0, right - left),
            Height = (uint)Math.Max(0, bottom - top)
        };
    }

    private static void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("SpriteRenderer.Initialize must be called before use.");
    }

    public static void Dispose()
    {
        foreach (ITexture texture in _textureCache.Values)
            texture.Dispose();

        _textureCache.Clear();

        _vertexBuffer?.Dispose();
        _vertexShader?.Dispose();
        _fragmentShader?.Dispose();
        _vertexLayout?.Dispose();
        _rasterizerState?.Dispose();
        _depthStencilState?.Dispose();
        _blendState?.Dispose();
        _pipeline?.Dispose();
        _sampler?.Dispose();

        _vertices.Clear();
        _batches.Clear();

        _initialized = false;
    }
}