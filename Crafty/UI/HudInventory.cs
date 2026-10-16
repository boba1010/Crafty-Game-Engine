using Crafty.UI.Inventory;
using CraftyNative;
using CraftyNative.HUD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.UI;
using System.Numerics;

namespace Crafty.UI;

public static class HudInventory
{
    private static bool _isOpen;

    private static readonly TabView _root = new()
    {
        Size = new Vector2(470, 480),
        HeaderHeight = 40f,
        TabWidth = 50f,
        IsMouseEnabled = true,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        TabColor = Styles.PanelBackground,
        Background = Vector4.Zero,
    };

    private static readonly Grid _globalViews = new()
    {
        BackgroundColor = Styles.PanelBackground,
    };

    private static readonly SlotGrid _hotbar = new()
    {
        Margin = new(0, 0, 0, 10f)
    };

    private static readonly Frame _frame = new()
    {
        Size = new(470f, 380f),
        VerticalAlignment = VerticalAlignment.Bottom,
    };
    private static readonly InventoryPage _inventoryPage = new();
    private static readonly CreativeInventoryPage _creativeInventoryPage = new();

    private static readonly IInventoryLayout[] _layouts = [_inventoryPage, _creativeInventoryPage];

    private static readonly Dictionary<string, (TabPage, Mesh?)> _tabs = [];

    private static readonly Grid _cursorHost = new()
    {
        BackgroundColor = Vector4.Zero,
        Size = new(Styles.SlotSize, Styles.SlotSize),
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top,
    };

    private static readonly Slot _cursorSlot = new()
    {
        Size = new(Styles.SlotSize),
        IsMouseEnabled = false,
        IsHoverEnabled = false,
        CornerRadius = 0f,
        ShowBackground = false,
        ShowBorder = false
    };

    private static Mesh? _cursorMesh;
    private static uint _cursorId;
    private static int _cursorCount;

    public static void SetSlot(int slotIndex, Mesh? mesh, uint itemId, int count = 1)
    {
        _syncing = true;

        try
        {
            if (slotIndex is >= 0 and < 9)
                SlotInteraction.Apply(_hotbar.Children[slotIndex], mesh, itemId, count);

            foreach (var layout in _layouts)
            {
                var slot = SlotIn(layout, slotIndex);
                if (slot is not null)
                    SlotInteraction.Apply(slot, mesh, itemId, count);
            }
        }
        finally 
        { 
            _syncing = false; 
        }
    }

    private static Slot? SlotIn(IInventoryLayout layout, int index) => index switch
    {
        >= 9 and < 36 => layout.Storage.Children[index - 9],
        >= 36 and < 40 => layout.ArmorSlots.Children[index - 36],
        >= 40 and < 44 => layout.CraftingSlots?.Children[index - 40],
        44 => layout.CraftingResultSlot?.Children[0],
        _ => null
    };

    public static event Action<int, uint, int>? SlotChanged;

    private static bool _syncing;

    private static void OnInteractionSlotChanged(Slot slot)
    {
        if (_syncing)
            return;

        int index = IndexOf(slot);
        if (index < 0)
            return;

        uint id = slot.Tag is uint t ? t : 0u;
        int count = Math.Max(slot.Count, 0);

        if (index >= 9)
        {
            _syncing = true;
            try
            {
                foreach (var layout in _layouts)
                {
                    var other = SlotIn(layout, index);
                    if (other is not null && !ReferenceEquals(other, slot))
                        SlotInteraction.Apply(other, slot.Content?.Model, id, count);
                }
            }
            finally 
            {
                _syncing = false; 
            }
        }

        SlotChanged?.Invoke(index, id, count);
    }

    private static int IndexOf(Slot slot)
    {
        int i = 0;
        foreach (var s in _hotbar.Children) 
        { 
            if (ReferenceEquals(s, slot)) 
                return i; 
            
            i++;
        }

        foreach (var layout in _layouts)
        {
            i = 9;
            foreach (var s in layout.Storage.Children) 
            { 
                if (ReferenceEquals(s, slot)) 
                    return i; 
                
                i++; 
            }

            i = 36;
            foreach (var s in layout.ArmorSlots.Children) 
            { 
                if (ReferenceEquals(s, slot)) 
                    return i; 

                i++; 
            }

            if (layout.CraftingSlots is not null)
            {
                i = 40;
                foreach (var s in layout.CraftingSlots.Children) 
                { 
                    if (ReferenceEquals(s, slot)) 
                        return i; 

                    i++; 
                }
            }

            if (layout.CraftingResultSlot is not null)
            {
                foreach (var s in layout.CraftingResultSlot.Children) 
                { 
                    if (ReferenceEquals(s, slot)) 
                        return 44; 
                }
            }
        }

        return -1;
    }

    public static void AddTab(string name, string? description = null, Mesh? icon = null)
    {
        var tab = new TabPage();
        _tabs.Add(name, (tab, icon));
        _root.AddTab(new TabItem(name, tab, icon, description));
    }

    public static void SetItem(string tabName, Mesh mesh, uint itemId)
    {
        if (!_tabs.TryGetValue(tabName, out var tab))
            return;

        tab.Item1.SetSlot(mesh, itemId);
    }

    public static void Initialize()
    {
        _root.SelectionChanged += OnTabSelectionChanged;
        SlotInteraction.SlotChanged += OnInteractionSlotChanged;

        _cursorHost.Columns.Add(new(Styles.SlotSize, GridUnit.Pixel));
        _cursorHost.Rows.Add(new(Styles.SlotSize, GridUnit.Pixel));
        _cursorHost.Add(_cursorSlot, 0, 0);
        SlotInteraction.Register(_cursorSlot);

        _globalViews.Rows.Add(new(380f, GridUnit.Pixel));
        _globalViews.Rows.Add(new(55f, GridUnit.Pixel));

        for (int i = 0; i < 9; i++)
        {
            Slot slot = new()
            {
                Size = new(Styles.SlotSize),
                IsMouseEnabled = true,
                IsHoverEnabled = true,
                CornerRadius = 0f,
                HoverBorderColor = Styles.Accent,
                HoverColor = new(1f, 1f, 1f, 0.1f)
            };
            slot.Clicked += OnSlotClick;
            _hotbar.Add(slot);
            SlotInteraction.Register(slot);
        }

        _frame.Page = _inventoryPage;

        _globalViews.Add(_frame, 0);
        _globalViews.Add(_hotbar, 1);

        _root.Content = _globalViews;
        _root.AddTab(new TabItem("Inv", _inventoryPage, tooltipDescription: "Inventory"));

        SystemAPI.Input.MouseWheel += Input_MouseWheel;
    }

    private static void Input_MouseWheel(float scroll)
    {
        if (!_isOpen || scroll == 0f)
            return;

        if (_frame.Page is TabPage tabPage)
            tabPage.OnMouseWheel(InputManager.MousePosition, scroll);
    }

    public static void SetCreativeMode(bool creative)
    {
        Page page = creative ? _creativeInventoryPage : _inventoryPage;
        _root.Tabs[0].Content = page;

        // if the inventory tab is the one currently showing, swap it live
        if (_frame.Page is InventoryPage or CreativeInventoryPage)
            _frame.Page = page;

        if (creative)
        {
            if (_tabs.Count > 0)
                return;

            foreach (var tab in _tabs)
                _root.AddTab(new(tab.Key, tab.Value.Item1, tab.Value.Item2, tab.Key));
        }
        else
        {
            var tabs = _root.Tabs.ToArray();

            for (int i = 1; i < tabs.Length; i++)
            {
                var tab = tabs[i];
                _root.RemoveTab(tab);
            }
        }
    }

    private static void OnTabSelectionChanged(TabItem? sender)
    {
        if (sender?.Content is Page page)
            _frame.Page = page;
    }

    private static void OnSlotClick(Slot sender) => SlotInteraction.OnClick(sender);

    public static void Render(Vector2 currentWindowSize)
    {
        if (!_isOpen)
            return;

        _root.Measure(currentWindowSize);
        _root.Arrange(currentWindowSize);
        _root.Update(InputManager.MousePosition, currentWindowSize, SystemAPI.Input.IsMouseButtonDown(MouseButton.Left));
        _root.Draw();

        DrawCursorItem(currentWindowSize);
    }

    private static void DrawCursorItem(Vector2 currentWindowSize)
    {
        if (!SlotInteraction.IsHolding)
            return;

        var mouse = InputManager.MousePosition;
        float half = Styles.SlotSize / 2f;

        if (_cursorMesh != SlotInteraction.HeldMesh || _cursorId != SlotInteraction.HeldId || _cursorCount != SlotInteraction.HeldCount)
        {
            _cursorMesh = SlotInteraction.HeldMesh;
            _cursorId = SlotInteraction.HeldId;
            _cursorCount = SlotInteraction.HeldCount;
            SlotInteraction.Apply(_cursorSlot, _cursorMesh, _cursorId, _cursorCount);
        }

        _cursorHost.Margin = new(mouse.X - half, mouse.Y - half, 0, 0);
        _cursorHost.Measure(currentWindowSize);
        _cursorHost.Arrange(currentWindowSize);
        _cursorHost.Draw();
    }

    public static void Open() => _isOpen = true;

    public static void Close()
    {
        if (SlotInteraction.IsHolding)
        {
            var slots = new List<Slot>();

            foreach (var s in _hotbar.Children) 
                slots.Add(s);

            foreach (var s in _inventoryPage.Storage.Children) 
                slots.Add(s);

            SlotInteraction.ReturnHeld(slots);
        }

        _isOpen = false;
    }
}