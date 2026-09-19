namespace CraftyNative.TwoD.Controls;

public sealed class NavigationView : UIElement
{
    public UIElementCollection MenuItems { get; }

    public Frame Frame { get; set; } = null!;

    public int SelectedIndex { get; set; }
    public Brush Background { get; set; } = new(new(0, 0, 0, 1));

    public NavigationView()
    {
        MenuItems = new UIElementCollection(this);
    }

    internal override UIElement? FindHit(float x, float y)
    {
        foreach (var item in MenuItems)
        {
            var hit = item.FindHit(x, y);

            if (hit is not null)
                return hit;
        }

        return Frame.FindHit(x, y);
    }

    internal override void Render(double deltaTime)
    {
        //var device = CraftyNative2D.Device;

        //using var brush = device.CreateSolidColorBrush(
        //    new(Background.Color.R, Background.Color.G, Background.Color.B, Background.Color.A));

        //device.FillRectangle(brush, new(Bounds.X, Bounds.Y, Bounds.Height, Bounds.Width));

        Frame.Render(deltaTime);
    }

    protected override void OnLayout()
    {
        // arrange navigation + content
    }
}
