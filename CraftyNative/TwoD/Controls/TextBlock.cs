using Vulcan.UI;

namespace CraftyNative.TwoD.Controls;

public sealed class TextBlock : UIElement
{
    public string Text { get; set; } = string.Empty;
    public Brush Foreground { get; set; } = new(new(0, 0, 0, 1));
    public float FontSize { get; set; } = 16;

    internal override void Render(double deltaTime)
    {
        var device = CraftyNative2D.Device;

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

        device.DrawText(textBrush, Text, new(Bounds.X, Bounds.Y, Bounds.Height, Bounds.Width), FontSize, horizontalAlignment, verticalAlignment);
    }
}
