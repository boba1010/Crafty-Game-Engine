using System.Numerics;
using CraftyNative.HUD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.UI;

namespace Crafty.UI.Inventory;

public sealed class TabPage : Page
{
    public const int Columns = 9;
    public const int Rows = 6;
    public const int PageSize = Columns * Rows;

    private readonly SlotGrid _slots = new();
    private readonly List<(Mesh? Mesh, uint Id)> _items = [];
    private int _scrollRow;

    private int TotalRows => (_items.Count + Columns - 1) / Columns;
    private int MaxScrollRow => Math.Max(0, TotalRows - Rows);
    private const float ScrollBarWidth = 6f;
    private const float ScrollBarPadding = 4f;
    private const float MinThumbHeight = 24f;

    private bool _isDraggingScrollBar;
    private float _dragStartY;
    private float _dragStartThumbY;

    private float ScrollTrackHeight => Math.Max(0f, ActualSize.Y - ScrollBarPadding * 2f);

    private float ScrollThumbHeight
    {
        get
        {
            if (TotalRows == 0)
                return ScrollTrackHeight;

            float height = ScrollTrackHeight * Rows / (float)TotalRows;

            return Math.Min(ScrollTrackHeight, Math.Max(MinThumbHeight, height));
        }
    }

    private float ScrollThumbTravel => Math.Max(0f, ScrollTrackHeight - ScrollThumbHeight);

    private float ScrollThumbY =>
        ScrollBarPadding + (MaxScrollRow == 0 ? 0f : ScrollThumbTravel * _scrollRow / MaxScrollRow);

    public TabPage()
    {
        Initialize();
        BackgroundColor = Styles.PanelBackground;
        VerticalAlignment = VerticalAlignment.Bottom;
        HorizontalAlignment = HorizontalAlignment.Center;
    }

    private void Initialize()
    {
        var root = new Grid()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            BackgroundColor = Vector4.Zero
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

    public void SetSlot(Mesh? mesh, uint itemId)
    {
        _items.Add((mesh, itemId));
        RefreshSlots();
    }

    public bool OnMouseWheel(Vector2 cursor, float delta)
    {
        if (!IsVisible || delta == 0f || MaxScrollRow == 0)
            return false;

        Vector2 p = AbsolutePosition;
        bool inside =
            cursor.X >= p.X && cursor.X < p.X + ActualSize.X &&
            cursor.Y >= p.Y && cursor.Y < p.Y + ActualSize.Y;

        if (!inside)
            return false;

        int next = Math.Clamp(_scrollRow - Math.Sign(delta), 0, MaxScrollRow);

        if (next != _scrollRow)
        {
            _scrollRow = next;
            RefreshSlots();
        }

        return true;
    }

    public void ResetScroll()
    {
        _scrollRow = 0;
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        _scrollRow = Math.Clamp(_scrollRow, 0, MaxScrollRow);

        int firstItem = _scrollRow * Columns;

        for (int i = 0; i < PageSize; i++)
        {
            int itemIndex = firstItem + i;
            Slot slot = _slots.Children[i];

            if (itemIndex < _items.Count)
            {
                var (mesh, id) = _items[itemIndex];
                SlotInteraction.Apply(slot, mesh, id, 1);
                slot.IsVisible = true;
            }
            else
            {
                SlotInteraction.Apply(slot, null, 0, 0);
                slot.IsVisible = false;
            }
        }
    }

    public override void Draw()
    {
        base.Draw();
        DrawScrollBar();
    }

    private void DrawScrollBar()
    {
        if (MaxScrollRow == 0)
            return;

        Vector2 position = AbsolutePosition;

        float x = position.X + ActualSize.X - ScrollBarWidth;
        float trackY = position.Y + ScrollBarPadding;
        float thumbY = position.Y + ScrollThumbY;

        UIRenderer.DrawRectangle(new(x, trackY), new(ScrollBarWidth, ScrollTrackHeight), new(0.15f, 0.15f, 0.15f, 0.7f));

        UIRenderer.DrawRectangle(new(x, thumbY), new(ScrollBarWidth, ScrollThumbHeight), Styles.Accent);
    }

    private bool TryBeginScrollBarDrag(Vector2 cursor)
    {
        if (MaxScrollRow == 0)
            return false;

        Vector2 position = AbsolutePosition;

        float thumbX = position.X + ActualSize.X - ScrollBarWidth;
        float thumbY = position.Y + ScrollThumbY;

        bool insideThumb =
            cursor.X >= thumbX &&
            cursor.X <= thumbX + ScrollBarWidth &&
            cursor.Y >= thumbY &&
            cursor.Y <= thumbY + ScrollThumbHeight;

        if (!insideThumb)
            return false;

        _isDraggingScrollBar = true;
        _dragStartY = cursor.Y;
        _dragStartThumbY = ScrollThumbY;

        return true;
    }

    private void UpdateScrollBarDrag(Vector2 cursor)
    {
        if (!_isDraggingScrollBar || ScrollThumbTravel <= 0f)
            return;

        float delta = cursor.Y - _dragStartY;

        float thumbY = Math.Clamp(
            _dragStartThumbY + delta,
            ScrollBarPadding,
            ScrollBarPadding + ScrollThumbTravel);

        float progress = (thumbY - ScrollBarPadding) / ScrollThumbTravel;

        _scrollRow = (int)MathF.Round(progress * MaxScrollRow);

        RefreshSlots();
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        if (!IsVisible)
            return;

        if (_isDraggingScrollBar)
        {
            if (mousePressed)
                UpdateScrollBarDrag(cursorPosition);
            else
                _isDraggingScrollBar = false;

            return;
        }

        if (mousePressed && TryBeginScrollBarDrag(cursorPosition))
            return;

        base.Update(cursorPosition, windowSize, mousePressed);
    }

    private void OnSlotClick(Slot sender) => SlotInteraction.OnCreativeClick(sender);
}