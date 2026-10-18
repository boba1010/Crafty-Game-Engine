using System.Numerics;

namespace CraftyNative.UI;

public sealed class Label : UIElement
{
    public string Text { get; set; } = string.Empty;
    public Vector4 Foreground { get; set; } = Vector4.One;
    public float FontScale { get; set; } = 0.8f;
    public bool IsTextCentered { get; set; }

    public override Vector2 Measure(Vector2 availableSize)
    {
        return Size;
    }

    public override void Arrange(Vector2 finalSize)
    {
        ActualSize = finalSize;
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed) { }

    public override void Draw()
    {
        if (string.IsNullOrEmpty(Text))
            return;

        if (IsTextCentered)
            UITextRenderer.DrawTextCentered(Text, AbsolutePosition, ActualSize, FontScale, Foreground);
        else
            UITextRenderer.DrawText(Text, AbsolutePosition, FontScale, Foreground);
    }
}
