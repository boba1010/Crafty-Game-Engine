using CraftyNative.ThreeD.Meshes;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;


namespace CraftyNative.UI;

public struct UIVertex(Vector2 position, Vector4 color, Vector4 clip)
{
    public Vector2 Position = position;
    public Vector4 Color = color;
    public Vector4 Clip = clip;
}

public static class UIRenderer
{
    public static bool IsClipEmpty => GetActiveClip().IsEmpty;

    public static Scissor CurrentScissor =>
        _clipInitialized ? _activeScissor : CreateScissor(new ClipRect(0, 0, _screenSize.X, _screenSize.Y));

    private static IBuffer _vertexBuffer = null!;
    private static IShader _vertexShader = null!;
    private static IShader _fragmentShader = null!;
    private static IVertexLayout _vertexLayout = null!;
    private static IRasterizerState _rasterizerState = null!;
    private static IDepthStencilState _depthStencilState = null!;
    private static IBlendState _blendState = null!;
    private static IPipeline _pipeline = null!;
    private static IGraphicsDevice _device = null!;

    private static readonly List<UIVertex> _vertices = [];
    private static readonly List<UIVertex> _overlayVertices = [];

    private static Vector2 _screenSize;
    private static uint _vertexBufferCapacity = 256 * 6;
    private static readonly uint _vertexStride = (uint)Marshal.SizeOf<UIVertex>();

    private readonly record struct UIBatch(int StartVertex, int VertexCount, Scissor Scissor);

    private readonly record struct ClipRect(float X, float Y, float Width, float Height)
    {
        public float Right => X + Width;
        public float Bottom => Y + Height;

        public bool IsEmpty => Width <= 0 || Height <= 0;

        public static ClipRect Intersect(ClipRect a, ClipRect b)
        {
            float left = MathF.Max(a.X, b.X);
            float top = MathF.Max(a.Y, b.Y);
            float right = MathF.Min(a.Right, b.Right);
            float bottom = MathF.Min(a.Bottom, b.Bottom);

            return new ClipRect(left, top, MathF.Max(0, right - left), MathF.Max(0, bottom - top));
        }
    }

    private static readonly List<UIBatch> _batches = [];
    private static readonly Stack<ClipRect> _clipStack = [];

    private static int _batchStartVertex;
    private static Scissor _activeScissor;
    private static bool _clipInitialized;

    private static Vector4 ActiveClipVector()
    {
        ClipRect c = GetActiveClip();
        return new Vector4(c.X, c.Y, c.Right, c.Bottom);
    }

    public static Vector4 CurrentClipRect => ActiveClipVector();

    public static void Initialize(IGraphicsDevice device)
    {
        _device = device;

        var shaderSource = """
        struct VSInput
        {
            float2 Position : POSITION;
            float4 Color : COLOR;
            float4 Clip : TEXCOORD0;
        };

        struct VSOutput
        {
            float4 Position : SV_Position;
            float4 Color : COLOR;
            nointerpolation float4 Clip : TEXCOORD0;
        };

        VSOutput VSMain(VSInput input)
        {
            VSOutput output;
            output.Position = float4(input.Position, 0.0, 1.0);
            output.Color = input.Color;
            output.Clip = input.Clip;
            return output;
        }

        float4 PSMain(VSOutput input) : SV_Target
        {
            clip(float4(
                input.Position.x - input.Clip.x,
                input.Position.y - input.Clip.y,
                input.Clip.z - input.Position.x,
                input.Clip.w - input.Position.y));

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

    private static void EnsureVertexBufferCapacity(uint required)
    {
        if (required <= _vertexBufferCapacity)
            return;

        uint capacity = _vertexBufferCapacity;

        while (capacity < required)
            capacity *= 2;

        var newBuffer = _device.CreateBuffer(new()
        {
            Size = capacity * _vertexStride,
            Usage = BufferUsage.Vertex,
            MemoryUsage = MemoryUsage.Upload
        });

        _vertexBuffer.Dispose();
        _vertexBuffer = newBuffer;
        _vertexBufferCapacity = capacity;
    }

    public static void DrawRectangle(Vector2 position, Vector2 size, Vector4 color)
    {
        AppendRectangle(_vertices, position, size, color);
    }

    public static void DrawOverlayRectangle(Vector2 position, Vector2 size, Vector4 color)
    {
        AppendRectangle(_overlayVertices, position, size, color);
    }

    private static void AppendRectangle(List<UIVertex> vertices, Vector2 position, Vector2 size, Vector4 color)
    {
        if (size.X <= 0 || size.Y <= 0 || _screenSize.X <= 0 || _screenSize.Y <= 0)
            return;

        Vector4 clip = ReferenceEquals(vertices, _overlayVertices) ? new(0, 0, _screenSize.X, _screenSize.Y) : ActiveClipVector();

        float left = position.X / _screenSize.X * 2f - 1f;
        float right = (position.X + size.X) / _screenSize.X * 2f - 1f;
        float top = 1f - position.Y / _screenSize.Y * 2f;
        float bottom = 1f - (position.Y + size.Y) / _screenSize.Y * 2f;

        vertices.Add(new(new(left, top), color, clip));
        vertices.Add(new(new(right, top), color, clip));
        vertices.Add(new(new(left, bottom), color, clip));

        vertices.Add(new(new(left, bottom), color, clip));
        vertices.Add(new(new(right, top), color, clip));
        vertices.Add(new(new(right, bottom), color, clip));
    }

    public static void DrawRoundedRectangle(Vector2 position, Vector2 size, float radius, Vector4 color)
    {
        AppendRoundedRectangle(_vertices, position, size, radius, color);
    }

    public static void DrawOverlayRoundedRectangle(Vector2 position, Vector2 size, float radius, Vector4 color) 
    {
        AppendRoundedRectangle(_overlayVertices, position, size, radius, color);
    }

    private static void AppendRoundedRectangle(List<UIVertex> vertices, Vector2 position, Vector2 size, float radius, Vector4 color)
    {
        if (size.X <= 0 || size.Y <= 0 || _screenSize.X <= 0 || _screenSize.Y <= 0)
            return;

        Vector4 clip = ReferenceEquals(vertices, _overlayVertices) ? new(0, 0, _screenSize.X, _screenSize.Y) : ActiveClipVector();

        radius = Math.Clamp(radius, 0f, MathF.Min(size.X, size.Y) * 0.5f);

        if (radius <= 0f)
        {
            AppendRectangle(vertices, position, size, color);
            return;
        }

        const int segmentsPerCorner = 6;
        const float halfPi = MathF.PI / 2f;

        Span<Vector2> centers =
        [
            new(position.X + size.X - radius, position.Y + radius),
            new(position.X + size.X - radius, position.Y + size.Y - radius),
            new(position.X + radius, position.Y + size.Y - radius),
            new(position.X + radius, position.Y + radius)
        ];

        Span<float> startAngles = [-halfPi, 0f, halfPi, MathF.PI];
        Span<Vector2> points = stackalloc Vector2[segmentsPerCorner * 4];

        int pointCount = 0;

        for (int corner = 0; corner < 4; corner++)
        {
            for (int segment = 0; segment < segmentsPerCorner; segment++)
            {
                float angle = startAngles[corner] + halfPi * segment / (segmentsPerCorner - 1);

                points[pointCount++] = centers[corner] + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            }
        }

        Vector2 center = position + size * 0.5f;
        Vector2 clipCenter = ToClip(center);

        for (int i = 0; i < pointCount; i++)
        {
            Vector2 a = ToClip(points[i]);
            Vector2 b = ToClip(points[(i + 1) % pointCount]);

            vertices.Add(new(clipCenter, color, clip));
            vertices.Add(new(a, color, clip));
            vertices.Add(new(b, color, clip));
        }
    }

    public static void DrawRoundedBorder(Vector2 position, Vector2 size, float radius, float thickness, Vector4 color)
    {
        AppendRoundedBorder(_vertices, position, size, radius, thickness, color);
    }

    public static void DrawOverlayRoundedBorder(Vector2 position, Vector2 size, float radius, float thickness, Vector4 color)
    {
        AppendRoundedBorder(_overlayVertices, position, size, radius, thickness, color);
    }

    private static void AppendRoundedBorder(List<UIVertex> vertices, Vector2 position, Vector2 size, float radius, float thickness, Vector4 color)
    {
        if (size.X <= 0 || size.Y <= 0 || thickness <= 0 || _screenSize.X <= 0 || _screenSize.Y <= 0)
            return;

        Vector4 clip = ReferenceEquals(vertices, _overlayVertices) ? new(0, 0, _screenSize.X, _screenSize.Y) : ActiveClipVector();

        radius = Math.Clamp(radius, 0f, MathF.Min(size.X, size.Y) * 0.5f);
        thickness = MathF.Min(thickness, MathF.Min(size.X, size.Y) * 0.5f);

        const int segmentsPerCorner = 6;
        const float halfPi = MathF.PI / 2f;

        Span<Vector2> outerCenters = stackalloc Vector2[4]
        {
            new(position.X + size.X - radius, position.Y + radius),
            new(position.X + size.X - radius, position.Y + size.Y - radius),
            new(position.X + radius, position.Y + size.Y - radius),
            new(position.X + radius, position.Y + radius)
        };

        Span<float> startAngles = stackalloc float[4]
        {
            -halfPi, 0f, halfPi, MathF.PI
        };

        Span<Vector2> outer = stackalloc Vector2[segmentsPerCorner * 4];
        Span<Vector2> inner = stackalloc Vector2[segmentsPerCorner * 4];

        float innerRadius = MathF.Max(0f, radius - thickness);
        Vector2 innerSize = size - new Vector2(thickness * 2f);
        Vector2 innerPosition = position + new Vector2(thickness);

        Span<Vector2> innerCenters = stackalloc Vector2[4]
        {
            new(innerPosition.X + innerSize.X - innerRadius, innerPosition.Y + innerRadius),
            new(innerPosition.X + innerSize.X - innerRadius, innerPosition.Y + innerSize.Y - innerRadius),
            new(innerPosition.X + innerRadius, innerPosition.Y + innerSize.Y - innerRadius),
            new(innerPosition.X + innerRadius, innerPosition.Y + innerRadius)
        };

        int count = 0;

        for (int corner = 0; corner < 4; corner++)
        {
            for (int segment = 0; segment < segmentsPerCorner; segment++)
            {
                float angle = startAngles[corner] + halfPi * segment / (segmentsPerCorner - 1);

                Vector2 direction = new(MathF.Cos(angle), MathF.Sin(angle));

                outer[count] = outerCenters[corner] + direction * radius;
                inner[count] = innerCenters[corner] + direction * innerRadius;
                count++;
            }
        }

        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;

            Vector2 outerA = ToClip(outer[i]);
            Vector2 outerB = ToClip(outer[next]);
            Vector2 innerA = ToClip(inner[i]);
            Vector2 innerB = ToClip(inner[next]);

            vertices.Add(new(outerA, color, clip));
            vertices.Add(new(innerA, color, clip));
            vertices.Add(new(outerB, color, clip));

            vertices.Add(new(outerB, color, clip));
            vertices.Add(new(innerA, color, clip));
            vertices.Add(new(innerB, color, clip));
        }
    }

    private static Vector2 ToClip(Vector2 point) => new(point.X / _screenSize.X * 2f - 1f, 1f - point.Y / _screenSize.Y * 2f);

    public static void Render(ICommandBuffer commandBuffer, Vector2 size)
    {
        _screenSize = size;

        if (!_clipInitialized)
        {
            _activeScissor = CreateScissor(new ClipRect(0, 0, size.X, size.Y));
            _clipInitialized = true;
        }

        FinishBatch();

        if (_vertices.Count > 0)
        {
            commandBuffer.SetPipeline(_pipeline);

            EnsureVertexBufferCapacity((uint)_vertices.Count);

            _vertexBuffer.Upload(MemoryMarshal.AsBytes(_vertices.AsSpan()));

            commandBuffer.SetVertexBuffer(_vertexBuffer, _vertexStride);

            foreach (UIBatch batch in _batches)
            {
                commandBuffer.SetScissor(batch.Scissor);
                commandBuffer.Draw((uint)batch.VertexCount, firstVertex: (uint)batch.StartVertex);
            }
        }

        _vertices.Clear();
        _batches.Clear();
        _clipStack.Clear();

        _batchStartVertex = 0;
        _clipInitialized = false;

        _vertices.Clear();
        _batches.Clear();
        _clipStack.Clear();

        _batchStartVertex = 0;
        _clipInitialized = false;

        commandBuffer.SetScissor(new Scissor
        {
            X = 0,
            Y = 0,
            Width = (uint)size.X,
            Height = (uint)size.Y
        });

        UI3DModelsRenderer.Render(commandBuffer, size);
    }

    public static void RenderOverlay(ICommandBuffer commandBuffer, Vector2 size)
    {
        _screenSize = size;

        if (_overlayVertices.Count == 0)
            return;

        commandBuffer.SetPipeline(_pipeline);

        EnsureVertexBufferCapacity((uint)_overlayVertices.Count);

        _vertexBuffer.Upload(MemoryMarshal.AsBytes(_overlayVertices.AsSpan()));

        commandBuffer.SetVertexBuffer(_vertexBuffer, _vertexStride);

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

    public static void DrawMesh(Mesh mesh, Vector2 position, Vector3 rotation, Vector3 scale, Vector2 size)
    {
        if (size.X <= 0 || size.Y <= 0 || IsClipEmpty)
            return;

        UI3DModelsRenderer.DrawMesh(mesh, position, rotation, scale, size, CurrentClipRect);
    }

    private static Scissor CreateScissor(ClipRect rect)
    {
        float left = Math.Clamp(rect.X, 0, _screenSize.X);
        float top = Math.Clamp(rect.Y, 0, _screenSize.Y);
        float right = Math.Clamp(rect.Right, 0, _screenSize.X);
        float bottom = Math.Clamp(rect.Bottom, 0, _screenSize.Y);

        int x = (int)MathF.Floor(left);
        int y = (int)MathF.Floor(top);
        int r = (int)MathF.Ceiling(right);
        int b = (int)MathF.Ceiling(bottom);

        return new Scissor
        {
            X = x,
            Y = y,
            Width = (uint)Math.Max(0, r - x),
            Height = (uint)Math.Max(0, b - y)
        };
    }

    private static ClipRect GetActiveClip() => _clipStack.Count > 0 ? _clipStack.Peek() : new ClipRect(0, 0, _screenSize.X, _screenSize.Y);

    private static void FinishBatch()
    {
        int count = _vertices.Count - _batchStartVertex;

        if (count <= 0)
            return;

        _batches.Add(new UIBatch(_batchStartVertex, count, _activeScissor));

        _batchStartVertex = _vertices.Count;
    }

    private static void UpdateActiveScissor()
    {
        Scissor next = CreateScissor(GetActiveClip());

        if (_clipInitialized &&
            next.X == _activeScissor.X &&
            next.Y == _activeScissor.Y &&
            next.Width == _activeScissor.Width &&
            next.Height == _activeScissor.Height)
            return;

        FinishBatch();

        _activeScissor = next;
        _clipInitialized = true;
    }

    public static void PushClip(Vector2 position, Vector2 size)
    {
        ClipRect requested = new(position.X, position.Y, MathF.Max(0, size.X), MathF.Max(0, size.Y));

        requested = ClipRect.Intersect(requested, new ClipRect(0, 0, _screenSize.X, _screenSize.Y));

        if (_clipStack.Count > 0)
            requested = ClipRect.Intersect(_clipStack.Peek(), requested);

        _clipStack.Push(requested);
        UpdateActiveScissor();
    }

    public static void PopClip()
    {
        if (_clipStack.Count == 0)
            throw new InvalidOperationException("No active clip region to pop.");

        _clipStack.Pop();
        UpdateActiveScissor();
    }

    public static void BeginFrame(Vector2 size)
    {
        _screenSize = size;
        _clipStack.Clear();
        _batches.Clear();
        _batchStartVertex = 0;

        _activeScissor = CreateScissor(new ClipRect(0, 0, size.X, size.Y));

        _clipInitialized = true;
    }

    public static void Dispose()
    {
        UI3DModelsRenderer.Dispose();
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
