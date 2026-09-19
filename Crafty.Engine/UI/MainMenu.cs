using CraftyNative.TwoD;
using CraftyNative.TwoD.Controls;

namespace Crafty.Engine.UI;

public sealed class MainMenu : Page
{
    public override void Initialize()
    {
        Background = new(new(1f, 1f, 1f, 1f));
        Content = new StackLayout();
        base.Initialize();
    }
}
