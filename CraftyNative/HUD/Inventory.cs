using CraftyNative.ThreeD.Meshes;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;

namespace CraftyNative.HUD;

/// <summary>
/// Sharp-cornered inventory panel, 9 columns x 4 rows.
/// Slot indices: 0-8 = hotbar (shown as the bottom row, same indices as <see cref="Hotbar"/>), 9-35 = storage.
/// </summary>
public static class Inventory
{
    public const int Columns = 9;
    public const int StorageRows = 3;
    public const int SlotCount = Columns * (StorageRows + 1);

    private const int FloatsPerVertex = 6; // x, y, r, g, b, a

    // Layout, in pixels
    private const float SlotSize = 44f;
    private const float Gap = 6f;
    private const float Padding = 16f;
    private const float SectionGap = 14f;   // extra space between storage and the hotbar row
    private const float AccentBarHeight = 3f;
    private const float MeshSize = SlotSize * 0.75f;

    private const float PanelWidth = Padding * 2f + Columns * SlotSize + (Columns - 1) * Gap;
    private const float StorageHeight = StorageRows * SlotSize + (StorageRows - 1) * Gap;
    private const float PanelHeight = Padding * 2f + StorageHeight + SectionGap + SlotSize;

    // Palette
    private static readonly Vector4 PanelBorder = new(0.22f, 0.22f, 0.25f, 1f);
    private static readonly Vector4 PanelFill = new(0.08f, 0.08f, 0.09f, 1f);
    private static readonly Vector4 Accent = new(0.25f, 0.65f, 1.00f, 1f);
    private static readonly Vector4 Divider = new(0.18f, 0.18f, 0.20f, 1f);
    private static readonly Vector4 SlotBorder = new(0.20f, 0.20f, 0.22f, 1f);
    private static readonly Vector4 SlotFill = new(0.12f, 0.12f, 0.14f, 1f);
    private static readonly Vector4 SlotHoverFill = new(0.19f, 0.19f, 0.22f, 1f);

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
    private static bool _dirty;
    private static int _hoveredSlot = -1;

    private static readonly Mesh?[] _slotMeshes = new Mesh?[SlotCount];
    private static readonly string[] _slotKeys = new string[SlotCount];
    private static int _keyVersion;

    public static bool IsOpen { get; private set; }

    /// <summary>Slot under the cursor (from the last <see cref="SetCursor"/>), or -1.</summary>
    public static int HoveredSlot => _hoveredSlot;

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

    public static void Open()
    {
        IsOpen = true;

        for (int i = 0; i < Columns; i++)
            SetSlot(i, Hotbar.GetSlotMesh(i), Hotbar.GetSlotKey(i));
    }

    public static void Close()
    {
        IsOpen = false;

        if (_hoveredSlot != -1)
        {
            _hoveredSlot = -1;
            _dirty = true;
        }
    }

    public static void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public static void Resize(Vector2 size)
    {
        if (size == _size)
            return;

        _size = size;
        _dirty = true;
    }

    /// <summary>Call with the mouse position in pixels (origin top-left) while the inventory is open.</summary>
    public static void SetCursor(Vector2 cursorPx)
    {
        int hovered = -1;

        if (IsOpen)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                var o = GetSlotOrigin(i);

                if (cursorPx.X >= o.X && cursorPx.X < o.X + SlotSize &&
                    cursorPx.Y >= o.Y && cursorPx.Y < o.Y + SlotSize)
                {
                    hovered = i;
                    break;
                }
            }
        }

        if (hovered == _hoveredSlot)
            return;

        _hoveredSlot = hovered;
        _dirty = true;
    }

    // materialKey: stable key per block type so identical blocks share one cached texture.
    public static void SetSlot(int slot, Mesh? mesh, string? materialKey = null)
    {
        if ((uint)slot >= SlotCount)
            return;

        _slotMeshes[slot] = mesh;
        _slotKeys[slot] = materialKey ?? $"inventory_{slot}_{++_keyVersion}";
    }

    private static Vector2 GetPanelOrigin() => new(
        MathF.Floor((_size.X - PanelWidth) * 0.5f),
        MathF.Floor((_size.Y - PanelHeight) * 0.5f));

    // Top-left of a slot in pixels, snapped to whole pixels so edges stay crisp
    private static Vector2 GetSlotOrigin(int slot)
    {
        var panel = GetPanelOrigin();
        float x;
        float y;

        if (slot < Columns)
        {
            x = panel.X + Padding + slot * (SlotSize + Gap);
            y = panel.Y + Padding + StorageHeight + SectionGap;
        }
        else
        {
            int s = slot - Columns;
            x = panel.X + Padding + (s % Columns) * (SlotSize + Gap);
            y = panel.Y + Padding + (s / Columns) * (SlotSize + Gap);
        }

        return new Vector2(MathF.Round(x), MathF.Round(y));
    }

    private static Vector2 GetSlotCenter(int slot)
    {
        var origin = GetSlotOrigin(slot);
        return new Vector2(origin.X + SlotSize * 0.5f, origin.Y + SlotSize * 0.5f);
    }

    private static void BuildVertices()
    {
        var list = new List<float>();

        void V(float x, float y, Vector4 c) => list.AddRange([x, y, c.X, c.Y, c.Z, c.W]);

        // Axis-aligned rectangle in pixels (origin top-left)
        void Rect(float x, float y, float w, float h, Vector4 c)
        {
            float x0 = x / _size.X * 2f - 1f;
            float x1 = (x + w) / _size.X * 2f - 1f;
            float y0 = 1f - y / _size.Y * 2f;
            float y1 = 1f - (y + h) / _size.Y * 2f;

            V(x0, y0, c); V(x1, y0, c); V(x1, y1, c);
            V(x0, y0, c); V(x1, y1, c); V(x0, y1, c);
        }

        var panel = GetPanelOrigin();

        // Panel: 1px border, dark fill, accent bar along the top edge
        Rect(panel.X, panel.Y, PanelWidth, PanelHeight, PanelBorder);
        Rect(panel.X + 1f, panel.Y + 1f, PanelWidth - 2f, PanelHeight - 2f, PanelFill);
        Rect(panel.X + 1f, panel.Y + 1f, PanelWidth - 2f, AccentBarHeight, Accent);

        // Divider between storage and the hotbar row
        float dividerY = MathF.Round(panel.Y + Padding + StorageHeight + SectionGap * 0.5f - 1f);
        Rect(panel.X + Padding, dividerY, PanelWidth - Padding * 2f, 2f, Divider);

        for (int i = 0; i < SlotCount; i++)
        {
            var o = GetSlotOrigin(i);

            bool hovered = i == _hoveredSlot;

            var outline = hovered ? Accent : SlotBorder;
            var fill = hovered ? SlotHoverFill : SlotFill;
            float border = hovered ? 2f : 1f;

            Rect(o.X, o.Y, SlotSize, SlotSize, outline);
            Rect(o.X + border, o.Y + border, SlotSize - border * 2f, SlotSize - border * 2f, fill);
        }

        vertices = [.. list];
    }

    private static void Rebuild()
    {
        BuildVertices();
        _vertexBuffer.Upload(MemoryMarshal.AsBytes(vertices.AsSpan()));
        _dirty = false;
    }

    public static void Render(ICommandBuffer commandBuffer)
    {
        if (!IsOpen)
            return;

        // Vertex count is constant (only positions and colors change), so re-uploading is safe
        if (_dirty)
            Rebuild();

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
                // Wipe depth so the panel's meshes aren't clipped by the hotbar's meshes or the world
                commandBuffer.ClearDepth(1f);
                any = true;
            }

            var center = GetSlotCenter(i);

            Console.WriteLine($"Slot {i}: {center} | Window: {_size}");

            Hotbar.DrawMeshInRect(commandBuffer, mesh, _slotKeys[i], center, MeshSize);
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
    }
}