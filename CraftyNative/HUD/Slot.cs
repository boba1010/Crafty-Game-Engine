using CraftyNative.UI;
using System.Numerics;

namespace CraftyNative.HUD;

public sealed class Slot : UIElement
{
    public float CornerRadius { get; set; } = 20f;
    public bool IsSelected { get; set; }
    public bool IsHovered { get; private set; }

    public bool ShowBackground { get; set; } = true;
    public bool ShowBorder { get; set; } = true;
    public Vector4 BackgroundColor { get; set; } = new(1f, 1f, 1f, 0.1f);
    public Vector4 HoverColor { get; set; } = new(0.22f, 0.22f, 0.22f, 1f);
    public Vector4 HoverBorderColor { get; set; } = new(0.19f, 0.19f, 0.22f, 1f);
    public Vector4 BorderColor { get; set; } = new(0.35f, 0.35f, 0.35f, 1f);
    public Vector4 SelectedBorderColor { get; set; } = new(1f, 1f, 1f, 1f);

    public string? ToolTip { get; set; }
    public Vector4? ToolTipBackground { get; set; }
    public Vector4? ToolTipBorder { get; set; }
    public Vector4 ToolTipForeground { get; set; } = Vector4.One;
    //public TooltipShape ToolTipShape { get; set; }

    public ModelView? Content { get; set; }
    public int Count { get; set; }

    public event Action<Slot>? Clicked;

    public override Vector2 Measure(Vector2 availableSize)
    {
        return new Vector2(Math.Clamp(Size.X, 0f, availableSize.X), Math.Clamp(Size.Y, 0f, availableSize.Y));
    }

    public override void Arrange(Vector2 finalSize)
    {
        ActualSize = Vector2.Max(finalSize, Vector2.Zero);
    }

    private bool _wasMousePressed;
    public override void Update(Vector2 cursorPosition, Vector2 windowSize, bool mousePressed)
    {
        if (!IsVisible || !IsMouseEnabled || !IsHoverEnabled)
        {
            IsHovered = false;
            _wasMousePressed = mousePressed;
            return;
        }

        Vector2 position = AbsolutePosition;

        IsHovered =
            cursorPosition.X >= position.X &&
            cursorPosition.X < position.X + ActualSize.X &&
            cursorPosition.Y >= position.Y &&
            cursorPosition.Y < position.Y + ActualSize.Y;

        if (IsHovered && mousePressed && !_wasMousePressed)
            Clicked?.Invoke(this);

        _wasMousePressed = mousePressed;
    }

    public override void Draw()
    {
        if (!IsVisible)
            return;

        Vector2 position = AbsolutePosition;
        Vector2 size = ActualSize;

        float radius = Math.Clamp(CornerRadius, 0f, MathF.Min(size.X, size.Y) * 0.5f);

        Vector4 borderColor = IsSelected ? SelectedBorderColor : IsHovered ? HoverBorderColor : BorderColor;

        Vector4 backgroundColor = IsSelected ? BackgroundColor : IsHovered && IsHoverEnabled ? HoverColor : BackgroundColor;

        if (ShowBackground && backgroundColor.W > 0f)
            UIRenderer.DrawRoundedRectangle(position, size, radius, backgroundColor);

        if (ShowBorder)
            UIRenderer.DrawRoundedBorder(position, size, radius, IsSelected ? 3f : 1f, borderColor);

        if (Content is not null)
        {
            Content.Parent = this;

            const float scale = 0.75f;
            Vector2 contentSize = size * scale;

            Content.Position = (size - contentSize) * 0.5f;
            Content.Measure(contentSize);
            Content.Arrange(contentSize);
            Content.Draw();
        }

        if (IsHovered && !string.IsNullOrEmpty(ToolTip))
        {
            const float padding = 8f;
            const float borderThickness = 2f;
            const float scale = 1f;

            Vector2 textSize = UITextRenderer.MeasureText(ToolTip, scale);
            Vector2 tipSize = textSize + new Vector2(padding * 2f);

            Vector2 tipPos = new(position.X + size.X + 8f, position.Y);

            var windowSize = SystemAPI.WindowSize;

            // Keep the tooltip inside the window.
            if (tipPos.X + tipSize.X > windowSize.X)
                tipPos.X = position.X - tipSize.X - 8f;

            if (tipPos.Y + tipSize.Y > windowSize.Y)
                tipPos.Y = windowSize.Y - tipSize.Y;

            Vector4 background = ToolTipBackground ?? new Vector4(0.06f, 0.05f, 0.10f, 0.96f);
            Vector4 border = ToolTipBorder ?? BorderColor;
            Vector4 foreground = ToolTipForeground == default ? Vector4.One : ToolTipForeground;

            UIRenderer.DrawRectangle(tipPos - new Vector2(borderThickness), tipSize + new Vector2(borderThickness * 2f), border);
            UIRenderer.DrawRectangle(tipPos, tipSize, background);

            UITextRenderer.DrawTextCentered(ToolTip, tipPos, tipSize, scale, foreground);
        }
    }
}