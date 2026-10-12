using System.Numerics;

namespace CraftyNative.UI;

public sealed class Seperator : UIElement
{
    public bool IsVertical { get; set; }
    public float Thickness { get; set; } = 1f;
    public Vector4 Color { get; set; } = new(0.35f, 0.35f, 0.35f, 1f);

    public override Vector2 Measure(Vector2 availableSize)
    {
        AvailableSize = availableSize;
        return IsVertical
            ? new Vector2(Thickness + Margin.Horizontal, availableSize.Y + Margin.Vertical)
            : new Vector2(availableSize.X + Margin.Horizontal, Thickness + Margin.Vertical);
    }

    public override void Arrange(Vector2 finalSize)
    {
        AvailableSize = finalSize;
        ActualSize = IsVertical ? new Vector2(Thickness, finalSize.Y) : new Vector2(finalSize.X, Thickness);
    }

    public override void Draw()
    {
        if (!IsVisible) return;
        UIRenderer.DrawRectangle(AbsolutePosition, ActualSize, Color);
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed) { }
}