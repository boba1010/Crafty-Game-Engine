using System.Numerics;

namespace CraftyNative.UI;

public abstract class UIElement
{
    public Vector2 Position { get; set; }
    public Vector2 ActualSize { get; protected set; }
    public Vector2 Size { get; set; }

    public UIElement? Parent { get; set; }

    protected Vector2 AbsolutePosition => Parent is null ? Position : Parent.AbsolutePosition + Position;

    public abstract void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed);
    public abstract void Draw();
    public abstract Vector2 Measure(Vector2 availableSize);
    public abstract void Arrange(Vector2 finalSize);
}
