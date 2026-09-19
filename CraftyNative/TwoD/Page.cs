using CraftyNative.TwoD;
using CraftyNative.TwoD.Controls;
using CraftyNative.TwoD.Maths;

namespace CraftyNative.TwoD;

public abstract class Page
{
    public UIElement Content { get; set; } = null!;
    public Brush Background { get; set; } = new(new());

    public Page()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        Content.Layout(new Rect(0, 0, CraftyNative2D.Height, CraftyNative2D.Width));
    }

    public virtual void Render(double deltaTime) 
    {
        var color = new Vulcan.UI.Color(Background.Color.R, Background.Color.G, Background.Color.B, Background.Color.A);
        CraftyNative2D.Device.Clear(color);

        Content.Render(deltaTime); 
    }
}
