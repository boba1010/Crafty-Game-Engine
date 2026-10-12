using CraftyNative.UI;
using System.Numerics;

namespace CraftyNative.HUD;

public sealed class SlotGrid : UIElement
{
    private readonly List<Slot> _children = [];

    public int Columns { get; set; } = 9;
    public float HorizontalSpacing { get; set; } = 6f;
    public float VerticalSpacing { get; set; } = 6f;
    public bool IsVertical { get; set; }

    public IReadOnlyList<Slot> Children => _children;

    private int LayoutColumns => IsVertical ? 1 : Math.Max(1, Columns);
    private int LayoutRows => IsVertical ? _children.Count : (_children.Count + LayoutColumns - 1) / LayoutColumns;

    public void Add(Slot element)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (element.Parent is not null)
            throw new InvalidOperationException("The element already belongs to a parent.");

        if (_children.Contains(element))
            return;

        element.Parent = this;
        _children.Add(element);
    }

    public override Vector2 Measure(Vector2 availableSize)
    {
        AvailableSize = availableSize;

        int columns = LayoutColumns;
        float cellWidth = 0f;
        float cellHeight = 0f;

        foreach (Slot child in _children)
        {
            Vector2 measured = child.Measure(availableSize);

            float width = child.Size.X > 0f ? child.Size.X : measured.X;

            float height = child.Size.Y > 0f ? child.Size.Y : measured.Y;

            cellWidth = MathF.Max(cellWidth, width + child.Margin.Horizontal);

            cellHeight = MathF.Max(cellHeight, height + child.Margin.Vertical);
        }

        int actualColumns = Math.Min(columns, _children.Count);
        int rows = LayoutRows;

        return new Vector2(
            cellWidth * actualColumns + HorizontalSpacing * Math.Max(0, actualColumns - 1) + Margin.Horizontal,
            cellHeight * rows + VerticalSpacing * Math.Max(0, rows - 1) + Margin.Vertical);
    }

    public override void Arrange(Vector2 finalSize)
    {
        AvailableSize = finalSize;

        int columns = LayoutColumns;
        float cellWidth = 0f;
        float cellHeight = 0f;

        foreach (Slot child in _children)
        {
            cellWidth = MathF.Max(cellWidth, child.Size.X + child.Margin.Horizontal);

            cellHeight = MathF.Max(cellHeight, child.Size.Y + child.Margin.Vertical);
        }

        int actualColumns = Math.Min(columns, _children.Count);
        int rows = LayoutRows;

        Vector2 contentSize = new(
            cellWidth * actualColumns + HorizontalSpacing * Math.Max(0, actualColumns - 1),
            cellHeight * rows + VerticalSpacing * Math.Max(0, rows - 1));

        Vector2 desiredSize = contentSize + new Vector2(Margin.Horizontal, Margin.Vertical);

        // Use the size allocated by the parent. A root SlotGrid
        // instead keeps its content size and aligns in finalSize.
        if (Parent is null)
        {
            ActualSize = desiredSize;

            Position = new Vector2(
                Align(HorizontalAlignment, finalSize.X, ActualSize.X),
                Align(VerticalAlignment, finalSize.Y, ActualSize.Y));
        }
        else
        {
            ActualSize = Vector2.Max(finalSize, Vector2.Zero);
        }

        for (int i = 0; i < _children.Count; i++)
        {
            Slot child = _children[i];

            if (!child.IsVisible)
                continue;

            float cellX = Margin.Left + (i % columns) * (cellWidth + HorizontalSpacing);

            float cellY = Margin.Top + (i / columns) * (cellHeight + VerticalSpacing);

            Vector2 childAvailable = new(
                Math.Max(0f, cellWidth - child.Margin.Horizontal),
                Math.Max(0f, cellHeight - child.Margin.Vertical));

            Vector2 measured = child.Measure(childAvailable);

            Vector2 desired = new(child.Size.X > 0f ? child.Size.X : measured.X, child.Size.Y > 0f ? child.Size.Y : measured.Y);

            Vector2 childSize = Vector2.Min(Vector2.Max(desired, Vector2.Zero), childAvailable);

            float x = Align(child.HorizontalAlignment, childAvailable.X, childSize.X);

            float y = Align(child.VerticalAlignment, childAvailable.Y, childSize.Y);

            child.Position = new Vector2(cellX + child.Margin.Left + x, cellY + child.Margin.Top + y);

            child.Arrange(childSize);
        }
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        if (!IsVisible)
            return;

        foreach (Slot child in _children)
        {
            if (child.IsVisible)
                child.Update(cursorPosition, windowSize, mousePressed);
        }
    }

    public override void Draw()
    {
        if (!IsVisible)
            return;

        foreach (Slot child in _children)
        {
            if (child.IsVisible)
                child.Draw();
        }
    }

    private static float Align(HorizontalAlignment alignment, float available, float desired)
        => alignment switch
        {
            HorizontalAlignment.Center => (available - desired) * 0.5f,
            HorizontalAlignment.Right => available - desired,
            _ => 0f
        };

    private static float Align(VerticalAlignment alignment, float available, float desired) 
        => alignment switch
        {
            VerticalAlignment.Center => (available - desired) * 0.5f,
            VerticalAlignment.Bottom => available - desired,
            _ => 0f
        };
}
