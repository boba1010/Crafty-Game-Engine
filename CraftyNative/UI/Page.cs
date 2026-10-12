using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace CraftyNative.UI;

public abstract class Page : UIElement
{
    public Vector4 BackgroundColor { get; set; } = Vector4.One;
    public string Title { get; set; } = null!;

    private UIElement? _content;
    public UIElement? Content 
    {
        get => _content;
        set
        {
            _content = value;
            _content?.Parent = this;
        }
    }

    public override Vector2 Measure(Vector2 availableSize)
    {
        AvailableSize = Vector2.Max(availableSize, Vector2.Zero);

        Vector2 contentAvailable = Vector2.Max(AvailableSize - new Vector2(Padding.Horizontal, Padding.Vertical), Vector2.Zero);

        Content?.Measure(contentAvailable);

        return Vector2.Min(Vector2.Max(Size, Vector2.Zero), AvailableSize);
    }

    public override void Arrange(Vector2 finalSize)
    {
        ActualSize = Vector2.Max(finalSize, Vector2.Zero);

        if (Content is null)
            return;

        Vector2 contentSize = Vector2.Max(ActualSize - new Vector2(Padding.Horizontal, Padding.Vertical), Vector2.Zero);

        Content.Position = new Vector2(Padding.Left, Padding.Top);
        Content.Arrange(contentSize);
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed) 
    {
        Content?.Update(cursorPosition, windowSize, mousePressed);
    }

    public override void Draw()
    {
        if (!IsVisible)
            return;

        UIRenderer.DrawRectangle(AbsolutePosition, ActualSize, BackgroundColor);
        if (Title is not null)
            UITextRenderer.DrawTextTopLeft(Title, AbsolutePosition + new Vector2(10f, 5f), 0.8f, Vector4.One);
        Content?.Draw();
    }
}