using CraftyNative.TwoD.Maths;

namespace CraftyNative.TwoD.Controls;

public sealed class StackLayout : Layout
{
    public Orientation Orientation { get; set; }

    internal override void Render(double deltaTime)
    {
        foreach (var child in Children) 
            child.Render(deltaTime);
    }

    protected override void ArrangeChildren(Rect bounds)
    {
        var y = bounds.Y;

        foreach (var child in Children)
        {
            var width = child.HorizontalAlignment == HorizontalAlignment.Stretch
                ? bounds.Width
                : child.Width;

            var x = child.HorizontalAlignment switch
            {
                HorizontalAlignment.Center =>
                    bounds.X + (bounds.Width - width) / 2,

                HorizontalAlignment.Right =>
                    bounds.X + bounds.Width - width,

                _ => bounds.X
            };

            child.Layout(new Rect(x, y, child.Height, width));

            y += child.Height;
        }
    }
}
