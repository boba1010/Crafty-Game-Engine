using CraftyNative.TwoD.Maths;

namespace CraftyNative.TwoD.Controls;

public abstract class Layout : UIElement
{
    public UIElementCollection Children { get; }

    protected Layout()
    {
        Children = new(this);
    }

    internal override UIElement? FindHit(float x, float y)
    {
        if (!IsVisible || !IsEnabled)
            return null;

        for (var i = Children.Count - 1; i >= 0; i--)
        {
            var child = Children[i];

            if (!child.HitTest(x, y))
                continue;

            if (child is Layout layout)
                return layout.FindHit(x, y) ?? child;

            return child;
        }

        return base.HitTest(x, y) ? this : null;
    }

    protected abstract void ArrangeChildren(Rect bounds);

    protected override void OnLayout()
    {
        ArrangeChildren(Bounds);
    }
}
