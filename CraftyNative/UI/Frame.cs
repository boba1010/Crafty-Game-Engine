using System.Numerics;

namespace CraftyNative.UI;

public sealed class Frame : UIElement
{
    public Vector4 Background { get; set; } = Vector4.Zero;
    private Page? _page;

    public Page? Page
    {
        get => _page;
        set
        {
            if (ReferenceEquals(_page, value))
                return;

            if (_page?.Parent == this)
                _page.Parent = null;

            _page = value;

            if (_page is not null)
            {
                _page.Parent = this;
                ArrangeContent();
            }
        }
    }

    public override Vector2 Measure(Vector2 availableSize)
    {
        AvailableSize = Vector2.Max(availableSize, Vector2.Zero);

        Vector2 desiredSize = Vector2.Min(Vector2.Max(Size, Vector2.Zero), AvailableSize);

        Vector2 contentSize = _page?.Measure(desiredSize) ?? Vector2.Zero;

        if (Size.X <= 0f)
            desiredSize.X = contentSize.X;

        if (Size.Y <= 0f)
            desiredSize.Y = contentSize.Y;

        return desiredSize + new Vector2(Margin.Left + Margin.Right, Margin.Top + Margin.Bottom);
    }

    public override void Arrange(Vector2 finalSize)
    {
        Vector2 availableSize = Vector2.Max(
            finalSize - new Vector2(Margin.Left + Margin.Right, Margin.Top + Margin.Bottom),
            Vector2.Zero);

        ActualSize = new Vector2(
            HorizontalAlignment == HorizontalAlignment.Stretch ? availableSize.X : Math.Min(Math.Max(Size.X, 0f), availableSize.X),
            VerticalAlignment == VerticalAlignment.Stretch ? availableSize.Y : Math.Min(Math.Max(Size.Y, 0f), availableSize.Y));

        ArrangeContent();
    }

    private void ArrangeContent()
    {
        if (_page is null)
            return;

        _page.Position = Vector2.Zero;
        _page.Arrange(ActualSize);
    }

    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        if (!IsVisible)
            return;

        if (_page is { IsVisible: true } content)
            content.Update(cursorPosition, windowSize, mousePressed);
    }

    public override void Draw()
    {
        if (!IsVisible)
            return;

        UIRenderer.DrawRectangle(AbsolutePosition, Size, Background);

        _page?.Draw();
    }
}
