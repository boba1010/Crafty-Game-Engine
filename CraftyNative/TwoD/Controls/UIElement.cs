using CraftyNative.TwoD.Maths;

namespace CraftyNative.TwoD.Controls;

public abstract class UIElement
{
    public float Width { get; set; }
    public float Height { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalContentAlignment VerticalContentAlignment { get; set; } = VerticalContentAlignment.Center;
    public HorizontalContentAlignment HorizontalContentAlignment { get; set; } = HorizontalContentAlignment.Center;
    public UIElement? Parent { get; internal set; }
    public Rect Bounds { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public event EventHandler? Pressed;
    public event EventHandler? Released;
    public event EventHandler? Enter;
    public event EventHandler? Exit;

    internal void RaisePressed()
    {
        Pressed?.Invoke(this, EventArgs.Empty);
    }

    internal void RaiseReleased()
    {
        Released?.Invoke(this, EventArgs.Empty);
    }

    internal void RaiseEnter()
    {
        Enter?.Invoke(this, EventArgs.Empty);
    }

    internal void RaiseExit()
    {
        Exit?.Invoke(this, EventArgs.Empty);
    }

    internal virtual UIElement? FindHit(float x, float y)
    {
        return HitTest(x, y) ? this : null;
    }

    internal virtual bool HitTest(float x, float y)
    {
        if (!IsVisible || !IsEnabled)
            return false;

        return x >= Bounds.X &&
               x <= Bounds.X + Bounds.Width &&
               y >= Bounds.Y &&
               y <= Bounds.Y + Bounds.Height;
    }

    internal abstract void Render(double deltaTime);

    protected virtual void OnInput() { }
    protected virtual void OnLayout() { }
    internal void Layout(Rect bounds)
    {
        Bounds = bounds;
        OnLayout();
    }
}
