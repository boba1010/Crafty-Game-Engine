using CraftyNative.HUD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.UI;

namespace Crafty.Engine.UI.Inventory;

/// <summary>
/// Creative-style item palette: a fixed 9x5 grid of slots that shows a scrollable window over the tab's item list.
/// </summary>
public sealed class TabPage : Page
{
    public const int Columns = 9;
    public const int Rows = 6;
    public const int PageSize = Columns * Rows;
    
    private readonly SlotGrid _slots = new()
    {
    };

    private readonly List<(Mesh? Mesh, uint Id)> _items = [];
    private int _scrollRow;

    public TabPage()
    {
        Initialize();
        BackgroundColor = Styles.PanelBackground;
        VerticalAlignment = VerticalAlignment.Bottom;
        HorizontalAlignment = HorizontalAlignment.Center;
    }

    /// <summary>Total number of scrollable rows beyond the visible ones.</summary>
    public int MaxScrollRow => Math.Max(0, (_items.Count + Columns - 1) / Columns - Rows);

    private void Initialize()
    {
        var root = new Grid()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            BackgroundColor = System.Numerics.Vector4.Zero
        };

        for (int i = 0; i < PageSize; i++)
        {
            Slot slot = new()
            {
                Size = new(Styles.SlotSize),
                IsMouseEnabled = true,
                IsHoverEnabled = true,
                CornerRadius = 0f,
                HoverBorderColor = Styles.Accent,
                HoverColor = new(1f, 1f, 1f, 0.1f),
            };
            slot.Clicked += OnSlotClick;

            _slots.Add(slot);
            SlotInteraction.Register(slot);
        }

        root.Add(_slots);
        Content = root;
    }

    /// <summary>Adds an item to this tab. Clicking it hands a stack to the cursor.</summary>
    public void SetSlot(Mesh? mesh, uint itemId)
    {
        _items.Add((mesh, itemId));
        Refresh();
    }

    /// <summary>Scrolls the palette by whole rows (negative = up, positive = down).</summary>
    public void Scroll(int rows)
    {
        int next = Math.Clamp(_scrollRow + rows, 0, MaxScrollRow);
        if (next == _scrollRow)
            return;

        _scrollRow = next;
        Refresh();
    }

    private void Refresh()
    {
        _scrollRow = Math.Clamp(_scrollRow, 0, MaxScrollRow);
        int start = _scrollRow * Columns;

        for (int i = 0; i < PageSize; i++)
        {
            var slot = _slots.Children[i];
            int itemIndex = start + i;

            if (itemIndex < _items.Count)
            {
                var (mesh, id) = _items[itemIndex];
                SlotInteraction.Apply(slot, mesh, id, 1);
            }
            else
            {
                SlotInteraction.Apply(slot, null, 0, 0);
            }
        }
    }

    private void OnSlotClick(Slot sender) => SlotInteraction.OnCreativeClick(sender);
}