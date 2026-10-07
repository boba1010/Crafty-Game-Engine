using System.Numerics;

namespace CraftyNative.UI;

public sealed class Pane : UIElement
{
    private readonly List<UIElement> _children = [];
    public float Spacing { get; set; }

    public void Add(UIElement element)
    {
        element.Parent = this;
        _children.Add(element);
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        foreach (var button in _children)
            button.Update(cursorPosition, windowSize, mousePressed);
    }

    public override void Draw()
    {
        foreach (var button in _children)
            button.Draw();
    }

    public override Vector2 Measure(Vector2 availableSize)
    {
        float width = 0;
        float height = 0;

        foreach (var child in _children)
        {
            var desiredSize = child.Measure(availableSize);

            width = MathF.Max(width, desiredSize.X);
            height += desiredSize.Y;
        }

        if (_children.Count > 1)
            height += Spacing * (_children.Count - 1);

        return new Vector2(width, height);
    }

    public override void Arrange(Vector2 finalSize)
    {
        ActualSize = finalSize;

        float y = 0;

        foreach (var child in _children)
        {
            child.Position = new Vector2(0, y);

            child.Arrange(child.Size);

            y += child.ActualSize.Y + Spacing;
        }
    }
}