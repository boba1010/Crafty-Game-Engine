using CraftyNative;
using CraftyNative.UI;
using System.Numerics;

namespace Crafty.Engine.UI;

public static class PauseMenu
{
    private static readonly Pane _pane = new()
    {
        Spacing = 10,
    };

    public static void Initialize(Action resumeAction, Action quitAction)
    {
        var resumeButton = new Button()
        {
            Size = new(400, 50),
            Text = "Resume"
        };

        resumeButton.Clicked += resumeAction;

        _pane.Add(resumeButton);

        var quitButton = new Button()
        {
            Text = "Quit & Save",
            Size = new(400, 50),
        };

        quitButton.Clicked += quitAction;
        _pane.Add(quitButton);
    }

    public static void Render(Vector2 currentWindowSize)
    {
        var paneSize = _pane.Measure(currentWindowSize);

        _pane.Position = (currentWindowSize - paneSize) / 2f;
        _pane.Arrange(paneSize);

        _pane.Update(InputManager.MousePosition, currentWindowSize, SystemAPI.Input.IsMouseButtonDown(MouseButton.Left));

        _pane.Draw();
    }
}
