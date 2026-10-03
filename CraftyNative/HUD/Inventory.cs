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
    private static Mesh? _heldMesh;
    private static string _heldKey = string.Empty;
    public static bool HasHeldItem => _heldMesh != null;
    private static int _keyVersion;

    private static Vector2 _cursorPosition;

    public static bool IsOpen { get; private set; }

    public static int HoveredSlot => _hoveredSlot;

    public const int MaxTabs = 6;

    private const float TabWidth = 44f;
    private const float TabHeight = 28f;
    private const float TabGap = 3f;
    private const float TabIconSize = 20f;

    private static readonly Mesh?[] _tabMeshes = new Mesh?[MaxTabs];
    private static readonly string[] _tabKeys = new string[MaxTabs];
    private static int _tabCount;
    private static int _activeTab;
    private static int _hoveredTab = -1;

    // The bar only shows with 2+ tabs, so a survival-only setup stays clean
    private static int VisibleTabs => _tabCount >= 2 ? _tabCount : 0;

    public static int ActiveTab => _activeTab;

    public static int TabCount
    {
        get => _tabCount;
        set
        {
            value = Math.Clamp(value, 0, MaxTabs);
            if (value == _tabCount) return;

            _tabCount = value;
            if (_activeTab >= value) _activeTab = 0;
            _dirty = true;
        }
    }

    public const int PaletteSlots = Columns * StorageRows; // 27 per page

    private static readonly Mesh?[] _paletteMeshes = new Mesh?[PaletteSlots];
    private static readonly string[] _paletteKeys = new string[PaletteSlots];

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

    public static void SetPaletteSlot(int index, Mesh? mesh, string? materialKey = null)
    {
        if ((uint)index >= PaletteSlots) return;
        _paletteMeshes[index] = mesh;
        _paletteKeys[index] = materialKey ?? $"palette_{index}_{++_keyVersion}";
    }

    public static void Open()
    {
        IsOpen = true;
    }

    public static void Close()
    {
        IsOpen = false;

        if (_hoveredSlot != -1 || _hoveredTab != -1)
        {
            _hoveredSlot = -1;
            _hoveredTab = -1;
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

    public static int HitTest(Vector2 px)
    {
        if (!IsOpen)
            return -1;

        for (int i = 0; i < SlotCount; i++)
        {
            var o = GetSlotOrigin(i);

            if (px.X >= o.X && px.X < o.X + SlotSize &&
                px.Y >= o.Y && px.Y < o.Y + SlotSize)
                return i;
        }

        return -1;
    }

    public static void SetCursor(Vector2 cursorPx)
    {
        _cursorPosition = cursorPx;

        int hovered = HitTest(cursorPx);
        int hoveredTab = HitTestTab(cursorPx);

        if (hovered == _hoveredSlot && hoveredTab == _hoveredTab)
            return;

        _hoveredSlot = hovered;
        _hoveredTab = hoveredTab;
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

    public static void SetActiveTab(int tab)
    {
        if ((uint)tab >= (uint)VisibleTabs || tab == _activeTab) return;
        _activeTab = tab;
        _dirty = true;
    }

    public static void SetTabIcon(int tab, Mesh? mesh, string? materialKey = null)
    {
        if ((uint)tab >= MaxTabs) return;
        _tabMeshes[tab] = mesh;
        _tabKeys[tab] = materialKey ?? $"tab_{tab}_{++_keyVersion}";
    }

    // The active tab is raised, idle tabs are a bit shorter
    private static float TabH(int t) => t == _activeTab ? TabHeight : TabHeight - 4f;

    private static Vector2 GetTabOrigin(int t)
    {
        var panel = GetPanelOrigin();
        return new Vector2(
            MathF.Round(panel.X + t * (TabWidth + TabGap)),
            MathF.Round(panel.Y - TabH(t)));
    }

    private static Vector2 GetTabCenter(int t)
    {
        var o = GetTabOrigin(t);
        return new Vector2(o.X + TabWidth * 0.5f, o.Y + TabH(t) * 0.5f);
    }

    public static int HitTestTab(Vector2 px)
    {
        if (!IsOpen) return -1;

        for (int t = 0; t < VisibleTabs; t++)
        {
            var o = GetTabOrigin(t);
            if (px.X >= o.X && px.X < o.X + TabWidth &&
                px.Y >= o.Y && px.Y < o.Y + TabH(t))
                return t;
        }

        return -1;
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

        for (int t = 0; t < MaxTabs; t++)
        {
            if (t >= VisibleTabs)
            {
                Rect(0, 0, 0, 0, PanelBorder);
                Rect(0, 0, 0, 0, PanelFill);
                Rect(0, 0, 0, 0, Accent);
                continue;
            }

            var o = GetTabOrigin(t);
            float h = TabH(t);
            bool selected = t == _activeTab;
            bool hovered = t == _hoveredTab && !selected;

            var border = selected ? PanelBorder : hovered ? Accent : SlotBorder;
            var fill = selected ? PanelFill : hovered ? SlotHoverFill : SlotFill;

            Rect(o.X, o.Y, TabWidth, h, border);
            Rect(o.X + 1f, o.Y + 1f, TabWidth - 2f, h - 1f, fill);   // open at the bottom so it joins the panel
            Rect(o.X + 1f, o.Y + 1f, TabWidth - 2f, selected ? AccentBarHeight : 0f, Accent);
        }

        vertices = [.. list];
    }

    private static void Rebuild()
    {
        BuildVertices();
        _vertexBuffer.Upload(MemoryMarshal.AsBytes(vertices.AsSpan()));
        _dirty = false;
    }

    public static void SetHeldItem(Mesh? mesh, string? materialKey = null)
    {
        _heldMesh = mesh;
        _heldKey = materialKey ?? $"inventory_held_{++_keyVersion}";
    }

    public static void ClearHeldItem()
    {
        _heldMesh = null;
        _heldKey = string.Empty;
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

        bool hasHeld = _heldMesh is { } h && h.Vertices.Count > 0 && h.Indices.Count > 0;
        bool any = false;

        for (int i = 0; i < SlotCount; i++)
        {
            bool palette = _activeTab != 0 && i >= Columns;
            var mesh = palette ? _paletteMeshes[i - Columns] : _slotMeshes[i];
            var key = palette ? _paletteKeys[i - Columns] : _slotKeys[i];

            if (mesh == null || mesh.Vertices.Count == 0 || mesh.Indices.Count == 0)
                continue;

            if (!any)
            {
                commandBuffer.ClearDepth(1f);
                any = true;
            }

            Hotbar.DrawMeshInRect(commandBuffer, mesh, key, GetSlotCenter(i), MeshSize);
        }

        for (int i = 0; i < SlotCount; i++)
        {
            bool palette = _activeTab != 0 && i >= Columns;
            var mesh = palette ? _paletteMeshes[i - Columns] : _slotMeshes[i];
            var key = palette ? _paletteKeys[i - Columns] : _slotKeys[i];

            if (mesh == null || mesh.Vertices.Count == 0 || mesh.Indices.Count == 0)
                continue;

            if (!any)
            {
                commandBuffer.ClearDepth(1f);
                any = true;
            }

            Hotbar.DrawMeshInRect(commandBuffer, mesh, key, GetSlotCenter(i), MeshSize);
        }

        if (hasHeld)
        {
            if (!any)
                commandBuffer.ClearDepth(1f);

            // Held item draws last, on top of the slot meshes
            commandBuffer.ClearDepth(1f);
            Hotbar.DrawMeshInRect(commandBuffer, _heldMesh!, _heldKey, _cursorPosition, MeshSize);
        }

        if (any || hasHeld)
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