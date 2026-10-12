
using System.Numerics;

namespace CraftyNative.UI;

public enum GridUnit
{
    Pixel,
    Auto,
    Star
}

public readonly record struct GridLength(float Value, GridUnit Unit)
{
    public static GridLength Auto => new(1f, GridUnit.Auto);
    public static GridLength Star => new(1f, GridUnit.Star);

    public static implicit operator GridLength(float pixels) => new(pixels, GridUnit.Pixel);
}

public sealed class Grid : UIElement
{
    private sealed class ChildEntry(UIElement element, int row, int column, int rowSpan, int columnSpan)
    {
        public UIElement Element { get; } = element;
        public int Row { get; } = row;
        public int Column { get; } = column;
        public int RowSpan { get; } = rowSpan;
        public int ColumnSpan { get; } = columnSpan;
    }

    private readonly List<ChildEntry> _children = [];

    private float[] _rowSizes = [];
    private float[] _columnSizes = [];

    public List<GridLength> Rows { get; } = [];
    public List<GridLength> Columns { get; } = [];

    public float RowSpacing { get; set; }
    public float ColumnSpacing { get; set; }

    public Vector4 BackgroundColor { get; set; } = new(1f, 1f, 1f, 1f);

    public IReadOnlyList<UIElement> Children => [.. _children.Select(entry => entry.Element)];

    public void Add(UIElement child, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (child.Parent is not null)
            throw new InvalidOperationException("The element already belongs to a parent.");

        if (_children.Any(entry => ReferenceEquals(entry.Element, child)))
            return;

        if (row < 0 || column < 0 || rowSpan < 1 || columnSpan < 1)
            throw new ArgumentOutOfRangeException(nameof(row), "Grid indices must be non-negative and spans positive.");

        EnsureTrackCount(Rows, row + rowSpan);
        EnsureTrackCount(Columns, column + columnSpan);

        child.Parent = this;

        _children.Add(new ChildEntry(
            child, row, column, rowSpan, columnSpan));
    }

    public bool Remove(UIElement child)
    {
        int index = _children.FindIndex(
            entry => ReferenceEquals(entry.Element, child));

        if (index < 0)
            return false;

        _children[index].Element.Parent = null;
        _children.RemoveAt(index);

        return true;
    }

    public void Clear()
    {
        foreach (ChildEntry entry in _children)
            entry.Element.Parent = null;

        _children.Clear();
    }

    public override Vector2 Measure(Vector2 availableSize)
    {
        AvailableSize = availableSize;

        EnsureTrackCount(Rows, 1);
        EnsureTrackCount(Columns, 1);

        _rowSizes = new float[Rows.Count];
        _columnSizes = new float[Columns.Count];

        var measuredChildren =
            new List<(ChildEntry Entry, Vector2 Size)>(_children.Count);

        foreach (ChildEntry entry in _children)
        {
            Vector2 childAvailable = new(
                Math.Max(0f, availableSize.X - Margin.Horizontal),
                Math.Max(0f, availableSize.Y - Margin.Vertical));

            Vector2 measured = entry.Element.Measure(childAvailable);

            Vector2 desired = new(
                entry.Element.Size.X > 0f
                    ? Math.Min(entry.Element.Size.X, childAvailable.X)
                    : measured.X,
                entry.Element.Size.Y > 0f
                    ? Math.Min(entry.Element.Size.Y, childAvailable.Y)
                    : measured.Y);

            desired += new Vector2(entry.Element.Margin.Horizontal, entry.Element.Margin.Vertical);

            measuredChildren.Add((entry, desired));
        }

        MeasureTracks(
            Columns,
            _columnSizes,
            [.. measuredChildren.Select(item => (item.Entry.Column, item.Entry.ColumnSpan, item.Size.X))], ColumnSpacing);

        MeasureTracks(
            Rows,
            _rowSizes,
            [.. measuredChildren.Select(item => (item.Entry.Row, item.Entry.RowSpan, item.Size.Y))], RowSpacing);

        Vector2 desiredSize = new(
            _columnSizes.Sum() + ColumnSpacing * Math.Max(0, Columns.Count - 1) + Margin.Horizontal,
            _rowSizes.Sum() + RowSpacing * Math.Max(0, Rows.Count - 1) + Margin.Vertical);

        if (Size.X > 0f)
            desiredSize.X = Size.X;

        if (Size.Y > 0f)
            desiredSize.Y = Size.Y;

        return Vector2.Min(
            Vector2.Max(desiredSize, Vector2.Zero),
            Vector2.Max(availableSize, Vector2.Zero));
    }

    public override void Arrange(Vector2 finalSize)
    {
        AvailableSize = finalSize;

        // A root Grid may use its explicit Size. Nested Grids use
        // the size allocated by their parent.
        ActualSize = Parent is null
            ? new Vector2(Size.X > 0f ? Size.X : finalSize.X, Size.Y > 0f ? Size.Y : finalSize.Y)
            : Vector2.Max(finalSize, Vector2.Zero);

        // Only root layouts position themselves against the supplied
        // available area. A parent positions nested children.
        if (Parent is null)
        {
            Position = new Vector2(
                Align(HorizontalAlignment, finalSize.X, ActualSize.X),
                Align(VerticalAlignment, finalSize.Y, ActualSize.Y));
        }

        EnsureTrackCount(Rows, 1);
        EnsureTrackCount(Columns, 1);

        _rowSizes = ResolveTracks(Rows, Math.Max(0f, ActualSize.Y - Margin.Vertical), _rowSizes, RowSpacing);

        _columnSizes = ResolveTracks(Columns, Math.Max(0f, ActualSize.X - Margin.Horizontal), _columnSizes, ColumnSpacing);

        float[] rowOffsets = GetOffsets(_rowSizes, RowSpacing, Margin.Top);

        float[] columnOffsets = GetOffsets(_columnSizes, ColumnSpacing, Margin.Left);

        foreach (ChildEntry entry in _children)
        {
            UIElement child = entry.Element;

            if (!child.IsVisible)
                continue;

            float cellWidth = SpanSize(_columnSizes, entry.Column, entry.ColumnSpan, ColumnSpacing);

            float cellHeight = SpanSize(_rowSizes, entry.Row, entry.RowSpan, RowSpacing);

            Thickness margin = child.Margin;

            Vector2 available = new(
                Math.Max(0f, cellWidth - margin.Horizontal),
                Math.Max(0f, cellHeight - margin.Vertical));

            Vector2 measured = child.Measure(available);

            Vector2 desired = new(
                child.Size.X > 0f ? child.Size.X : measured.X,
                child.Size.Y > 0f ? child.Size.Y : measured.Y);

            Vector2 childSize = Vector2.Min(Vector2.Max(desired, Vector2.Zero), available);

            float x = Align(child.HorizontalAlignment, available.X, childSize.X);

            float y = Align(child.VerticalAlignment, available.Y, childSize.Y);

            child.Position = new Vector2(
                columnOffsets[entry.Column] + margin.Left + x,
                rowOffsets[entry.Row] + margin.Top + y);

            child.Arrange(childSize);
        }
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        if (!IsVisible)
            return;

        foreach (ChildEntry entry in _children)
        {
            if (entry.Element.IsVisible)
                entry.Element.Update(cursorPosition, windowSize, mousePressed);
        }
    }

    public override void Draw()
    {
        if (!IsVisible)
            return;

        UIRenderer.DrawRectangle(AbsolutePosition, ActualSize, BackgroundColor);

        foreach (ChildEntry entry in _children)
        {
            if (entry.Element.IsVisible)
                entry.Element.Draw();
        }
    }

    private static void EnsureTrackCount(List<GridLength> tracks, int count)
    {
        while (tracks.Count < count)
            tracks.Add(GridLength.Star);
    }

    private static void MeasureTracks(List<GridLength> definitions, float[] sizes, List<(int Start, int Span, float Desired)> children, float spacing)
    {
        Array.Clear(sizes);

        for (int i = 0; i < definitions.Count; i++)
        {
            GridLength track = definitions[i];

            if (track.Unit == GridUnit.Pixel)
                sizes[i] = Math.Max(0f, track.Value);
        }

        foreach (var child in children)
        {
            if (child.Start < 0 || child.Start + child.Span > definitions.Count || child.Span != 1)
                continue;

            int index = child.Start;

            if (definitions[index].Unit == GridUnit.Auto)
                sizes[index] = Math.Max(sizes[index], child.Desired);
        }

        // Spanning children can increase Auto tracks.
        foreach (var child in children.Where(item => item.Span > 1))
        {
            int start = child.Start;
            int end = start + child.Span;

            if (start < 0 || end > definitions.Count)
                continue;

            float current = spacing * (child.Span - 1);

            for (int i = start; i < end; i++)
                current += sizes[i];

            float deficit = child.Desired - current;

            if (deficit <= 0f)
                continue;

            int autoCount = 0;

            for (int i = start; i < end; i++)
            {
                if (definitions[i].Unit == GridUnit.Auto)
                    autoCount++;
            }

            if (autoCount == 0)
                continue;

            float share = deficit / autoCount;

            for (int i = start; i < end; i++)
            {
                if (definitions[i].Unit == GridUnit.Auto)
                    sizes[i] += share;
            }
        }
    }

    private static float[] ResolveTracks(List<GridLength> definitions, float available, float[] desiredSizes, float spacing)
    {
        var result = new float[definitions.Count];

        float remaining = Math.Max(0f, available - spacing * Math.Max(0, definitions.Count - 1));

        float totalStars = 0f;

        for (int i = 0; i < definitions.Count; i++)
        {
            GridLength track = definitions[i];

            if (track.Unit == GridUnit.Pixel)
            {
                result[i] = Math.Max(0f, track.Value);
                remaining -= result[i];
            }
            else if (track.Unit == GridUnit.Auto)
            {
                result[i] = i < desiredSizes.Length ? Math.Max(0f, desiredSizes[i]) : 0f;

                remaining -= result[i];
            }
            else
            {
                totalStars += Math.Max(0f, track.Value);
            }
        }

        remaining = Math.Max(0f, remaining);

        if (totalStars > 0f)
        {
            float unit = remaining / totalStars;

            for (int i = 0; i < definitions.Count; i++)
            {
                if (definitions[i].Unit == GridUnit.Star)
                    result[i] = Math.Max(0f, definitions[i].Value) * unit; 
            }
        }

        return result;
    }

    private static float[] GetOffsets(float[] sizes, float spacing, float initialOffset)
    {
        var offsets = new float[sizes.Length];
        float position = initialOffset;

        for (int i = 0; i < sizes.Length; i++)
        {
            offsets[i] = position;
            position += sizes[i] + spacing;
        }

        return offsets;
    }

    private static float SpanSize(float[] sizes, int start, int span, float spacing)
    {
        if (start < 0 || start >= sizes.Length || span <= 0)
            return 0f;

        int count = Math.Min(span, sizes.Length - start);
        float result = 0f;

        for (int i = start; i < start + count; i++)
            result += sizes[i];

        return result + spacing * Math.Max(0, count - 1);
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
