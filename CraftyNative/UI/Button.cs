using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace CraftyNative.UI;

public sealed class Button : UIElement
{
    public string Text { get; set; } = string.Empty;
    public bool IsHovered { get; private set; }
    public event Action? Clicked;

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        var position = AbsolutePosition;

        IsHovered = cursorPosition.X >= position.X &&
                    cursorPosition.X <= position.X + ActualSize.X &&
                    cursorPosition.Y >= position.Y &&
                    cursorPosition.Y <= position.Y + ActualSize.Y;

        if (IsHovered && mousePressed)
            Clicked?.Invoke();
    }

    public override void Draw()
    {
        var color = IsHovered ? new Vector4(0.25f, 0.25f, 0.25f, 1f) : new Vector4(0f, 0f, 0f, 1f);

        UIRenderer.DrawRectangle(AbsolutePosition, ActualSize, color);

        UITextRenderer.DrawTextCentered(Text, AbsolutePosition, ActualSize, 0.8f, Vector4.One);
    }

    public override Vector2 Measure(Vector2 availableSize)
    {
        return Size;
    }

    public override void Arrange(Vector2 finalSize)
    {
        ActualSize = finalSize;
    }
}