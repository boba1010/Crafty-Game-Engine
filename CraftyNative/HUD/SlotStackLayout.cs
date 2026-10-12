using CraftyNative.UI;
using System.Numerics;

namespace CraftyNative.HUD;

public sealed class SlotStackLayout : UIElement
{
    private readonly List<Slot> _children = [];

    public bool IsHorizontal { get; set; } = true;
    public float Spacing { get; set; }

    public IReadOnlyList<Slot> Children => _children;

    public void Add(Slot element)
    {
        ArgumentNullException.ThrowIfNull(element);

        if (element.Parent is not null)
            throw new InvalidOperationException(
                "The element already belongs to a parent.");

        if (_children.Contains(element))
            return;

        element.Parent = this;
        _children.Add(element);
    }

    public override Vector2 Measure(Vector2 availableSize)
    {
        Vector2 desiredSize = Vector2.Zero;

        foreach (UIElement child in _children)
        {
            Vector2 childSize = child.Measure(availableSize);

            if (IsHorizontal)
            {
                desiredSize.X += childSize.X;
                desiredSize.Y = MathF.Max(desiredSize.Y, childSize.Y);
            }
            else
            {
                desiredSize.X = MathF.Max(desiredSize.X, childSize.X);
                desiredSize.Y += childSize.Y;
            }
        }

        if (_children.Count > 1)
        {
            float spacing = Spacing * (_children.Count - 1);

            if (IsHorizontal)
                desiredSize.X += spacing;
            else
                desiredSize.Y += spacing;
        }

        return desiredSize;
    }

    public override void Arrange(Vector2 finalSize)
    {
        ActualSize = finalSize;

        float offset = 0;

        foreach (Slot child in _children)
        {
            child.Position = IsHorizontal ? new Vector2(offset, 0) : new Vector2(0, offset);

            child.Arrange(child.Size);

            offset += (IsHorizontal ? child.ActualSize.X : child.ActualSize.Y) + Spacing;
        }
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        foreach (Slot child in _children)
        {
            if (child.IsVisible)
                child.Update(cursorPosition, windowSize, mousePressed);
        }
    }

    public override void Draw()
    {
        foreach (Slot child in _children)
        {
            if (child.IsVisible)
                child.Draw();
        }
    }
}
