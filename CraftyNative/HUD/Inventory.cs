using CraftyNative.ThreeD.Meshes;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulcan;
using Vulcan.Graphics;
using Renderer = CraftyNative.CraftyNative;

namespace CraftyNative.HUD;

/// <summary>
/// Sharp-cornered inventory panel, 9 columns wide.
/// Slot indices: 0-8 = hotbar (bottom row, same indices as <see cref="Hotbar"/>), 9-35 = storage,
/// 36-39 = armor, 40-43 = 2x2 crafting grid, 44 = crafting output.
/// Tab 0 shows the survival layout (top section + storage + hotbar). Other tabs show a creative palette
/// in place of storage, above the same hotbar row.
/// </summary>
public static class Inventory
{
    public const int Columns = 9;
    public const int StorageRows = 3;
    public const int SlotCount = Columns * (StorageRows + 1);

    public const int ArmorStart = SlotCount;        // 36-39: helmet, chest, legs, boots
    public const int CraftStart = ArmorStart + 4;   // 40-43: 2x2 grid
    public const int OutputSlot = CraftStart + 4;   // 44: craft result
    public const int TotalSlots = OutputSlot + 1;

    public const int CreativeRows = 6;
    public const int PaletteSlots = Columns * CreativeRows; // blocks per creative page
    public const int MaxTabs = 6;

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

    private const float CreativeHeight = CreativeRows * SlotSize + (CreativeRows - 1) * Gap;
    private const float CreativePanelHeight = Padding * 2f + CreativeHeight + SectionGap + SlotSize;

    // Top section (tab 0 only): armor, player model, crafting
    private const float TopHeight = Padding + 4 * SlotSize + 3 * Gap;
    private const float FullHeight = PanelHeight + TopHeight;
    private const float ModelWidth = 3 * SlotSize + 2 * Gap;
    private const float ModelHeight = 4 * SlotSize + 3 * Gap;

    // Tabs
    private const float TabWidth = 44f;
    private const float TabHeight = 40f;
    private const float TabGap = 3f;
    private const float TabIconSize = 20f;

    // Page buttons (creative tabs)
    private const float PageButtonW = 28f;
    private const float PageButtonH = 24f;

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
    private static int _hoveredTab = -1;
    private static int _keyVersion;
    private static Vector2 _cursorPosition;

    private static readonly Mesh?[] _slotMeshes = new Mesh?[TotalSlots];

    private static readonly Mesh?[] _paletteMeshes = new Mesh?[PaletteSlots];

    private static readonly Mesh?[] _tabMeshes = new Mesh?[MaxTabs];
    private static int _tabCount;
    private static int _activeTab;

    private static Mesh? _heldMesh;

    private static Mesh? _modelMesh;

    public static bool IsOpen { get; private set; }
    public static bool HasHeldItem => _heldMesh != null;

    /// <summary>Slot under the cursor (from the last <see cref="SetCursor"/>), or -1.</summary>
    public static int HoveredSlot => _hoveredSlot;

    public static int ActiveTab => _activeTab;

    // The tab bar only shows with 2+ tabs, so a survival-only setup stays clean
    private static int VisibleTabs => _tabCount >= 2 ? _tabCount : 0;
    private static bool ShowTop => _activeTab == 0;
    private static bool CreativeTab => _activeTab != 0;

    private static float VisiblePanelHeight => ShowTop ? FullHeight : CreativePanelHeight;
    private static bool PageButtonsVisible => _activeTab != 0 && VisibleTabs > 0;

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

    private const float GroupWidth = SlotSize + Gap + ModelWidth;   // armor column + model

    private static bool _craftingVisible = true;

    public static bool CraftingVisible
    {
        get => _craftingVisible;
        set
        {
            if (value == _craftingVisible) return;
            _craftingVisible = value;
            _dirty = true;
        }
    }

    // Shifts armor + model to the middle of the panel when there's no crafting on the right
    private static float GroupOffset => _craftingVisible ? 0f : (PanelWidth - Padding * 2f - GroupWidth) * 0.5f;

    private static readonly int[] _slotCounts = new int[TotalSlots];
    private static int _heldCount;
    private static readonly HudDigits _digits = new();

    private static bool SlotVisible(int i)
    {
        if (CreativeTab)
            return i < Columns + PaletteSlots;

        return i < SlotCount ||
               (ShowTop && (i < CraftStart || _craftingVisible));
    }

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

        _digits.Initialize(device);
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

    // ---------------------------------------------------------------- input

    public static int HitTest(Vector2 px)
    {
        if (!IsOpen)
            return -1;

        int count = ShowTop ? TotalSlots : Columns + PaletteSlots;

        for (int i = 0; i < count; i++)
        {
            if (!SlotVisible(i))
                continue;

            var o = GetSlotOrigin(i);

            if (px.X >= o.X && px.X < o.X + SlotSize &&
                px.Y >= o.Y && px.Y < o.Y + SlotSize)
                return i;
        }

        return -1;
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

    /// <summary>0 = previous page, 1 = next page, -1 = none.</summary>
    public static int HitTestPage(Vector2 px)
    {
        if (!IsOpen || !PageButtonsVisible) return -1;

        for (int b = 0; b < 2; b++)
        {
            var o = GetPageButtonOrigin(b);
            if (px.X >= o.X && px.X < o.X + PageButtonW &&
                px.Y >= o.Y && px.Y < o.Y + PageButtonH)
                return b;
        }

        return -1;
    }

    /// <summary>Call with the mouse position in pixels (origin top-left) while the inventory is open.</summary>
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

    // ---------------------------------------------------------------- tabs

    public static void SetActiveTab(int tab)
    {
        if ((uint)tab >= (uint)VisibleTabs || tab == _activeTab) return;

        _activeTab = tab;
        _dirty = true;

        // Layout changed under the cursor, so recompute hover now
        SetCursor(_cursorPosition);
    }

    public static void SetTabIcon(int tab, Mesh? mesh)
    {
        if ((uint)tab >= MaxTabs) return;

        _tabMeshes[tab] = mesh;
    }

    // ---------------------------------------------------------------- slot contents

    // materialKey: stable key per block type so identical blocks share one cached texture.
    public static void SetSlot(int slot, Mesh? mesh, int count = 1)
    {
        if ((uint)slot >= TotalSlots)
            return;

        _slotMeshes[slot] = mesh;
        _slotCounts[slot] = count;
    }

    /// <summary>One entry of the current creative page (0..PaletteSlots-1), shown in place of storage.</summary>
    public static void SetPaletteSlot(int index, Mesh? mesh)
    {
        if ((uint)index >= PaletteSlots) return;

        _paletteMeshes[index] = mesh;
    }

    public static void SetPlayerModel(Mesh? mesh)
    {
        _modelMesh = mesh;
    }

    public static void SetHeldItem(Mesh? mesh, int count = 1)
    {
        _heldMesh = mesh;
        _heldCount = count;
    }

    public static void ClearHeldItem()
    {
        _heldMesh = null;
        _heldCount = 0;
    }

    // ---------------------------------------------------------------- layout

    // Top-left of the panel. Centered on the full (tab 0) height so the tab bar never moves.
    private static Vector2 GetPanelOrigin() => new(
        MathF.Floor((_size.X - PanelWidth) * 0.5f),
        MathF.Floor((_size.Y - FullHeight) * 0.5f));

    // Where storage + hotbar start: below the top section on tab 0, at the panel top otherwise
    private static Vector2 GetContentOrigin()
    {
        var p = GetPanelOrigin();
        return new Vector2(p.X, ShowTop ? p.Y + TopHeight : p.Y);
    }

    private static Vector2 GetModelOrigin()
    {
        var p = GetPanelOrigin();
        return new Vector2(MathF.Round(p.X + Padding + GroupOffset + SlotSize + Gap), MathF.Round(p.Y + Padding));
    }

    // Top-left of a slot in pixels, snapped to whole pixels so edges stay crisp
    private static Vector2 GetSlotOrigin(int slot)
    {
        float step = SlotSize + Gap;

        //
        // CREATIVE TABS
        //
        if (CreativeTab)
        {
            var panel = GetPanelOrigin();

            // hotbar stays at the bottom
            if (slot < Columns)
            {
                return new Vector2(MathF.Round(panel.X + Padding + slot * step), MathF.Round(panel.Y + Padding + CreativeHeight + SectionGap));
            }

            int palette = slot - Columns;

            int row = palette / Columns;
            int col = palette % Columns;

            return new Vector2(MathF.Round(panel.X + Padding + col * step), MathF.Round(panel.Y + Padding + row * step));
        }

        //
        // SURVIVAL TAB
        //
        float x;
        float y;

        if (slot >= ArmorStart)
        {
            var p = GetPanelOrigin();
            float top = p.Y + Padding;

            if (slot < CraftStart)
            {
                x = p.X + Padding + GroupOffset;
                y = top + (slot - ArmorStart) * step;
            }
            else if (slot < OutputSlot)
            {
                int c = slot - CraftStart;

                x = p.X + Padding + (5 + c % 2) * step;
                y = top + (1 + c / 2) * step;
            }
            else
            {
                x = p.X + Padding + 8 * step;
                y = top + 1.5f * step;
            }
        }
        else
        {
            var panel = GetContentOrigin();

            if (slot < Columns)
            {
                x = panel.X + Padding + slot * step;
                y = panel.Y + Padding + StorageHeight + SectionGap;
            }
            else
            {
                int s = slot - Columns;

                x = panel.X + Padding + (s % Columns) * step;
                y = panel.Y + Padding + (s / Columns) * step;
            }
        }

        return new Vector2(MathF.Round(x), MathF.Round(y));
    }

    private static Vector2 GetSlotCenter(int slot)
    {
        var origin = GetSlotOrigin(slot);
        return new Vector2(origin.X + SlotSize * 0.5f, origin.Y + SlotSize * 0.5f);
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

    // b: 0 = previous, 1 = next (right-aligned to the panel)
    private static Vector2 GetPageButtonOrigin(int b)
    {
        var panel = GetPanelOrigin();
        float x = panel.X + PanelWidth - (2 - b) * PageButtonW - (1 - b) * TabGap;
        return new Vector2(MathF.Round(x), MathF.Round(panel.Y - PageButtonH));
    }

    // ---------------------------------------------------------------- geometry

    // NOTE: the vertex count must be identical on every tab and state (the buffer is sized once),
    // so anything hidden is emitted as zero-size shapes instead of being skipped.
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

        // Triangle in pixels (origin top-left)
        void Tri(float x0, float y0, float x1, float y1, float x2, float y2, Vector4 c)
        {
            V(x0 / _size.X * 2f - 1f, 1f - y0 / _size.Y * 2f, c);
            V(x1 / _size.X * 2f - 1f, 1f - y1 / _size.Y * 2f, c);
            V(x2 / _size.X * 2f - 1f, 1f - y2 / _size.Y * 2f, c);
        }

        var panel = GetPanelOrigin();
        var content = GetContentOrigin();

        // Panel: 1px border, dark fill, accent bar along the top edge
        Rect(panel.X, panel.Y, PanelWidth, VisiblePanelHeight, PanelBorder);
        Rect(panel.X + 1f, panel.Y + 1f, PanelWidth - 2f, VisiblePanelHeight - 2f, PanelFill);
        Rect(panel.X + 1f, panel.Y + 1f, PanelWidth - 2f, AccentBarHeight, Accent);

        // Divider between storage and the hotbar row
        float contentHeight = ShowTop ? StorageHeight : CreativeHeight;
        float dividerY = MathF.Round(content.Y + Padding + contentHeight + SectionGap * 0.5f - 1f);
        Rect(content.X + Padding, dividerY, PanelWidth - Padding * 2f, 2f, Divider);

        // Top section extras: model frame, crafting arrow, divider above storage
        if (ShowTop)
        {
            float step = SlotSize + Gap;
            var mo = GetModelOrigin();

            Rect(mo.X, mo.Y, ModelWidth, ModelHeight, SlotBorder);
            Rect(mo.X + 1f, mo.Y + 1f, ModelWidth - 2f, ModelHeight - 2f, SlotFill);

            float ax = panel.X + Padding + 7 * step + SlotSize * 0.5f;
            float ay = panel.Y + Padding + 1.5f * step + SlotSize * 0.5f;

            if (_craftingVisible)
                Tri(ax - 8f, ay - 9f, ax - 8f, ay + 9f, ax + 9f, ay, Accent);
            else
                Tri(0, 0, 0, 0, 0, 0, Accent);

            Rect(panel.X + Padding, content.Y + Padding * 0.5f - 1f, PanelWidth - Padding * 2f, 2f, Divider);
        }
        else
        {
            Rect(0, 0, 0, 0, SlotBorder);
            Rect(0, 0, 0, 0, SlotFill);
            Tri(0, 0, 0, 0, 0, 0, Accent);
            Rect(0, 0, 0, 0, Divider);
        }

        // Slots
        int slotVisualCount = Math.Max(TotalSlots, Columns + PaletteSlots);
        for (int i = 0; i < slotVisualCount; i++)
        {
            if (!SlotVisible(i))
            {
                Rect(0, 0, 0, 0, SlotBorder);
                Rect(0, 0, 0, 0, SlotFill);
                continue;
            }

            var o = GetSlotOrigin(i);

            bool hovered = i == _hoveredSlot;

            var outline = hovered ? Accent : SlotBorder;
            var fill = hovered ? SlotHoverFill : SlotFill;
            float border = hovered ? 2f : 1f;

            Rect(o.X, o.Y, SlotSize, SlotSize, outline);
            Rect(o.X + border, o.Y + border, SlotSize - border * 2f, SlotSize - border * 2f, fill);
        }

        // Tabs
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

        // Page buttons (creative tabs)
        for (int b = 0; b < 2; b++)
        {
            if (!PageButtonsVisible)
            {
                Rect(0, 0, 0, 0, SlotBorder);
                Rect(0, 0, 0, 0, SlotFill);
                Tri(0, 0, 0, 0, 0, 0, Accent);
                continue;
            }

            var o = GetPageButtonOrigin(b);
            Rect(o.X, o.Y, PageButtonW, PageButtonH, SlotBorder);
            Rect(o.X + 1f, o.Y + 1f, PageButtonW - 2f, PageButtonH - 2f, SlotFill);

            float cx = o.X + PageButtonW * 0.5f;
            float cy = o.Y + PageButtonH * 0.5f;

            if (b == 0)
                Tri(cx + 4f, cy - 6f, cx + 4f, cy + 6f, cx - 5f, cy, Accent);
            else
                Tri(cx - 4f, cy - 6f, cx - 4f, cy + 6f, cx + 5f, cy, Accent);
        }

        vertices = [.. list];
    }

    private static void Rebuild()
    {
        BuildVertices();
        _vertexBuffer.Upload(MemoryMarshal.AsBytes(vertices.AsSpan()));
        _dirty = false;
    }

    // ---------------------------------------------------------------- rendering

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

        // Slots (top-section slots only exist on tab 0; creative tabs swap storage for the palette)
        int count = ShowTop ? TotalSlots : Columns + PaletteSlots;

        for (int i = 0; i < count; i++)
        {
            if (!SlotVisible(i))
                continue;

            bool palette = _activeTab != 0 && i >= Columns;
            var mesh = palette ? _paletteMeshes[i - Columns] : _slotMeshes[i];

            if (mesh == null || mesh.Vertices.Count == 0 || mesh.Indices.Count == 0)
                continue;

            if (!any)
            {
                commandBuffer.ClearDepth(1f);
                any = true;
            }

            Hotbar.DrawMeshInRect(commandBuffer, mesh, GetSlotCenter(i), MeshSize);
        }

        // Player model
        if (ShowTop && _modelMesh is { } model && model.Vertices.Count > 0 && model.Indices.Count > 0)
        {
            if (!any)
            {
                commandBuffer.ClearDepth(1f);
                any = true;
            }

            DrawModel(commandBuffer, model);
        }

        // Tab icons
        for (int t = 0; t < VisibleTabs; t++)
        {
            var mesh = _tabMeshes[t];
            if (mesh == null || mesh.Vertices.Count == 0 || mesh.Indices.Count == 0)
                continue;

            if (!any)
            {
                commandBuffer.ClearDepth(1f);
                any = true;
            }

            Hotbar.DrawMeshInRect(commandBuffer, mesh, GetTabCenter(t), TabIconSize);
        }

        if (hasHeld)
        {
            // Held item draws last, on top of the slot and tab meshes
            commandBuffer.ClearDepth(1f);
            Hotbar.DrawMeshInRect(commandBuffer, _heldMesh!, _cursorPosition, MeshSize);
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

        _digits.Begin(_size);

        for (int i = 0; i < count; i++)
        {
            bool palette = _activeTab != 0 && i >= Columns;   // palette entries show no counts

            if (palette || !SlotVisible(i) || _slotMeshes[i] == null)
                continue;

            var o = GetSlotOrigin(i);
            _digits.AddCount(_slotCounts[i], o.X + SlotSize - 3f, o.Y + SlotSize - 3f);
        }

        if (hasHeld)
            _digits.AddCount(_heldCount, _cursorPosition.X + MeshSize * 0.5f, _cursorPosition.Y + MeshSize * 0.5f);

        _digits.Flush(commandBuffer, _pipeline);
    }

    private static void DrawModel(ICommandBuffer commandBuffer, Mesh mesh)
    {
        var o = GetModelOrigin();

        commandBuffer.SetViewport(new Viewport
        {
            X = o.X,
            Y = o.Y,
            Width = ModelWidth,
            Height = ModelHeight,
            MinDepth = 0,
            MaxDepth = 1
        });

        var (center, scale) = Hotbar.GetBounds(mesh);

        float yaw = Math.Clamp((_cursorPosition.X - (o.X + ModelWidth * 0.5f)) / 150f, -1f, 1f) * 0.6f;
        float pitch = Math.Clamp((_cursorPosition.Y - (o.Y + ModelHeight * 0.3f)) / 150f, -1f, 1f) * 0.3f;

        var m =
            Matrix4x4.CreateTranslation(-center) *
            Matrix4x4.CreateScale(scale) *
            Matrix4x4.CreateRotationY(yaw) *
            Matrix4x4.CreateRotationX(pitch) *
            Matrix4x4.CreateTranslation(0f, 0f, -5f);

        var proj = Matrix4x4.CreateOrthographic(1.2f * ModelWidth / ModelHeight, 1.2f, 0.1f, 10f);

        Renderer.DrawHudMesh(mesh, m * proj);
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