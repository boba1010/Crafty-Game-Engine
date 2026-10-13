using System.Numerics;

namespace CraftyNative.UI;

public sealed class ScrollView : UIElement
{
    public UIElement? Content { get; set; }

    public float ScrollOffset { get; private set; }
    public float ScrollSpeed { get; set; } = 40f;
    public bool IsHovered { get; private set; }

    public override void Arrange(Vector2 finalSize)
    {
        ActualSize = Vector2.Max(finalSize, Vector2.Zero);

        if (Content is null)
            return;

        Content.Parent = this;

        Vector2 contentSize = Content.Measure(new Vector2(ActualSize.X, float.MaxValue));

        Content.Arrange(contentSize);
    }

    public override void Draw()
    {
        if (!IsVisible || Content is null)
            return;

        Content.Draw();
    }

    public override Vector2 Measure(Vector2 availableSize)
    {
        if (Content is null)
            return Vector2.Zero;

        Content.Measure(new Vector2(availableSize.X, float.MaxValue));

        return availableSize;
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        if (!IsVisible || !IsMouseEnabled || !IsHoverEnabled)
        {
            IsHovered = false;
            return;
        }

        Vector2 position = AbsolutePosition;

        IsHovered =
            cursorPosition.X >= position.X &&
            cursorPosition.X < position.X + ActualSize.X &&
            cursorPosition.Y >= position.Y &&
            cursorPosition.Y < position.Y + ActualSize.Y;

        if (Content is null)
            return;

        Content.Update(cursorPosition, windowSize, mousePressed);
    }
}
