using CraftyNative.ThreeD.Meshes;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;
using Renderer = CraftyNative.CraftyNative;

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
    private static Vector2 _size;
    private static int _selectedSlot = 0;
    private static readonly Mesh?[] _slotMeshes = new Mesh?[SlotCount];
    private static readonly string[] _slotKeys = new string[SlotCount];
    private static readonly Dictionary<Mesh, (Vector3 Center, float Scale)> _bounds = new();
    private static int _keyVersion;

    private const int FloatsPerVertex = 6; // x, y, r, g, b, a
    private const int SlotCount = 9;

    private const float Margin = 20f;
    private const float Width = 400f;
    private const float Height = 50f;
    private const float Radius = Height / 2f;
    private const int Segments = 24;
    private const float SlotSize = 40f;
    private const float MeshSize = SlotSize * 0.8f;

    public static int SelectedSlot => _selectedSlot;

    public static void Initialize(IGraphicsDevice device, Vector2 size)
    {
        _size = size;
        BuildVertices();

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

    private static void BuildVertices()
    {
        float sx = 2f / _size.X;
        float sy = 2f / _size.Y;

        float halfWidth = Width * 0.5f * sx;
        float halfHeight = Height * 0.5f * sy;
        float radiusX = Radius * sx;
        float radiusY = Radius * sy;

        float centerY = -1f + halfHeight + Margin * sy;

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
            for (int i = 0; i < Segments; i++)
            {
                float a0 = startAngle + MathF.PI * i / Segments;
                float a1 = startAngle + MathF.PI * (i + 1) / Segments;

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
        float slotHalfX = SlotSize * 0.5f * sx;
        float slotHalfY = SlotSize * 0.5f * sy;

        for (int i = 0; i < SlotCount; i++)
        {
            float t = i / (float)(SlotCount - 1);
            float cx = leftCenter + (rightCenter - leftCenter) * t;
            var color = i == _selectedSlot ? selectedColor : slotColor;

            Circle(cx, centerY, slotHalfX, slotHalfY, color);
        }

        vertices = [.. list];
    }

    // Slot center in pixels, origin top-left (matches BuildVertices layout)
    private static Vector2 GetSlotCenterPx(int slot)
    {
        float t = slot / (float)(SlotCount - 1);
        float left = _size.X * 0.5f - Width * 0.5f + Radius;
        float right = _size.X * 0.5f + Width * 0.5f - Radius;

        return new Vector2(
            left + (right - left) * t,
            _size.Y - Margin - Height * 0.5f);
    }

    // Center + uniform scale so any mesh fits a unit cube.
    // Assumes position is the first 3 floats of each vertex.
    private static (Vector3 Center, float Scale) GetBounds(Mesh mesh)
    {
        if (_bounds.TryGetValue(mesh, out var cached))
            return cached;

        int floatsPerVertex = Math.Max(3, (int)mesh.VertexStride / sizeof(float));
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (int i = 0; i + 2 < mesh.Vertices.Count; i += floatsPerVertex)
        {
            var p = new Vector3(mesh.Vertices[i], mesh.Vertices[i + 1], mesh.Vertices[i + 2]);
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        var size = max - min;
        float extent = MathF.Max(size.X, MathF.Max(size.Y, size.Z));

        var result = ((min + max) * 0.5f, extent > 0f ? 1f / extent : 1f);
        _bounds[mesh] = result;
        return result;
    }

    // Isometric view of the mesh, normalized to a unit cube centered on the slot.
    private static Matrix4x4 BuildSlotMvp(Mesh mesh)
    {
        var (center, scale) = GetBounds(mesh);

        var model =
            Matrix4x4.CreateTranslation(-center) *
            Matrix4x4.CreateScale(scale) *
            Matrix4x4.CreateRotationY(MathF.PI / 4f) *
            Matrix4x4.CreateRotationX(MathF.PI / 6f) *
            Matrix4x4.CreateTranslation(0f, 0f, -5f);

        var proj = Matrix4x4.CreateOrthographic(1.8f, 1.8f, 0.1f, 10f);

        // Same row-vector convention as the main pass, so no transpose
        return model * proj;
    }

    public static void Resize(Vector2 size)
    {
        if (size == _size || _vertexBuffer == null)
            return;

        _size = size;
        BuildVertices();
        _vertexBuffer.Upload(MemoryMarshal.AsBytes(vertices.AsSpan()));
    }

    public static void SetSelectedSlot(int slot)
    {
        if ((uint)slot >= SlotCount || slot == _selectedSlot)
            return;

        _selectedSlot = slot;

        if (_vertexBuffer == null)
            return;

        BuildVertices();
        _vertexBuffer.Upload(MemoryMarshal.AsBytes(vertices.AsSpan()));
    }

    // materialKey: pass a stable key per block type so identical blocks share one cached texture.
    // If omitted, a unique key is generated per assignment (safe, but uploads a texture each time).
    public static void SetSlot(int slot, Mesh? mesh, string? materialKey = null)
    {
        if ((uint)slot >= SlotCount)
            return;

        _slotMeshes[slot] = mesh;
        _slotKeys[slot] = materialKey ?? $"hotbar_{slot}_{++_keyVersion}";
    }

    // For the scroll wheel: +1 / -1, wraps around
    public static void CycleSlot(int delta)
    {
        int next = ((_selectedSlot + delta) % SlotCount + SlotCount) % SlotCount;
        SetSelectedSlot(next);
    }

    public static void Render(ICommandBuffer commandBuffer)
    {
        commandBuffer.SetPipeline(_pipeline);
        commandBuffer.SetVertexBuffer(_vertexBuffer, sizeof(float) * FloatsPerVertex);
        commandBuffer.Draw((uint)(vertices.Length / FloatsPerVertex));

        bool any = false;

        for (int i = 0; i < SlotCount; i++)
        {
            var mesh = _slotMeshes[i];
            if (mesh == null || mesh.Vertices.Count == 0 || mesh.Indices.Count == 0)
                continue;

            if (!any)
            {
                commandBuffer.ClearDepth(1f);
                any = true;
            }

            var c = GetSlotCenterPx(i);

            commandBuffer.SetViewport(new Viewport
            {
                X = c.X - MeshSize * 0.5f,
                Y = c.Y - MeshSize * 0.5f,
                Width = MeshSize,
                Height = MeshSize,
                MinDepth = 0,
                MaxDepth = 1
            });

            Renderer.DrawHudMesh(mesh, BuildSlotMvp(mesh), _slotKeys[i]);
        }

        if (any)
        {
            commandBuffer.SetViewport(new Viewport
            {
                X = 0,
                Y = 0,
                Width = _size.X,
                Height = _size.Y,
                MinDepth = 0,
                MaxDepth = 1
            });
        }
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

        _bounds.Clear();
    }
}