using CraftyNative;
using CraftyNative.TwoD;
using CraftyNative.TwoD.Animations;
using CraftyNative.TwoD.Controls;

namespace Crafty.Engine.UI;

public sealed class MainWindow : Window
{
    public MainWindow()
    {
        Initialize();

        Activated += Window_Activated;
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        var navView = new NavigationView()
        {
            Frame = new Frame(),
        };

        var mainMenuPage = new MainMenu();

        var button = new Button
        {
            Content = "Play",
            Width = 200,
            Height = 100,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        button.Enter += Button_Enter;
        button.Exit += Button_Exit;

        if (mainMenuPage.Content is StackLayout stackLayout)
            stackLayout.Children.Add(button);

        navView.Frame.Navigate(mainMenuPage);

        Root = navView;
    }

    private void Button_Exit(object? sender, EventArgs e)
    {
        var btn = (Button?)sender;
        if (btn is null)
            return;

        var start = btn.Background.Color;
        var end = new Color(0, 0, 0, 1);

        var animation = new Animation(0.15f, progress =>
        {
            var color = new Color(
                start.R + (end.R - start.R) * progress,
                start.G + (end.G - start.G) * progress,
                start.B + (end.B - start.B) * progress,
                start.A + (end.A - start.A) * progress);

            btn.Background.Color = color;
        });

        AnimationsManager.Add(animation);
    }

    private void Button_Enter(object? sender, EventArgs e)
    {
        var btn = (Button?)sender;
        if (btn is null)
            return;

        var start = btn.Background.Color;
        var end = new Color(1, 0, 0, 1);

        var animation = new Animation(0.15f, progress =>
        {
            var color = new Color(
                start.R + (end.R - start.R) * progress,
                start.G + (end.G - start.G) * progress,
                start.B + (end.B - start.B) * progress,
                start.A + (end.A - start.A) * progress);

            btn.Background = new(color);
        });

        AnimationsManager.Add(animation);
    }
}
