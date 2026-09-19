using Vulcan.UI;

namespace CraftyNative.TwoD.Controls;

public sealed class Button : UIElement
{
    public object Content { get; set; } = null!;
    public Brush Background { get; set; } = new(new(0, 0, 0, 1));
    public Brush Foreground { get; set; } = new(new(1, 1, 1, 1));
    public event EventHandler? Click;

    public Button()
    {
        Released += OnReleased;
    }

    private void OnReleased(object? sender, EventArgs e)
    {
        Click?.Invoke(this, EventArgs.Empty);
    }

    internal override void Render(double deltaTime)
    {
        var device = CraftyNative2D.Device;

        using var brush = device.CreateSolidColorBrush(
            new(Background.Color.R, Background.Color.G, Background.Color.B, Background.Color.A));

        device.FillRoundedRectangle(brush, new(Bounds.X, Bounds.Y, Bounds.Height, Bounds.Width), 8);

        if (Content is string text)
        {
            using var textBrush = device.CreateSolidColorBrush(
                new(Foreground.Color.R, Foreground.Color.G, Foreground.Color.B, Foreground.Color.A));

            var verticalAlignment = VerticalContentAlignment switch
            {
                VerticalContentAlignment.Top => TextVerticalAlignment.Top,
                VerticalContentAlignment.Bottom => TextVerticalAlignment.Bottom,
                VerticalContentAlignment.Center => TextVerticalAlignment.Center,
                VerticalContentAlignment.Stretch => TextVerticalAlignment.Stretch,
                _ => TextVerticalAlignment.Center
            };

            var horizontalAlignment = HorizontalContentAlignment switch
            {
                HorizontalContentAlignment.Left => TextHorizontalAlignment.Left,
                HorizontalContentAlignment.Right => TextHorizontalAlignment.Right,
                HorizontalContentAlignment.Center => TextHorizontalAlignment.Center,
                HorizontalContentAlignment.Stretch => TextHorizontalAlignment.Stretch,
                _ => TextHorizontalAlignment.Center
            };

            device.DrawText(textBrush, text, new(Bounds.X, Bounds.Y, Bounds.Height, Bounds.Width), 16, horizontalAlignment, verticalAlignment);
        }
    }
}