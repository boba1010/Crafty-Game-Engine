using CraftyNative;
using System.Numerics;

namespace Crafty.UI;

public static class HudDeathScreen
{
    private static bool _isOpen;

    public static void Open()
    {
        _isOpen = true;
        SystemAPI.Input.CursorMode = Cursor.Normal;
    }
    public static void Close() => _isOpen = false;

    public static event Action? RespawnClicked;

    private static DeathScreenPage _deathScreenPage = null!;

    public static void Initialize(Action quitRequested)
    {
        _deathScreenPage = new();

        _deathScreenPage.RespawnRequested += DeathScreenPage_RespawnRequested;
        _deathScreenPage.QuitRequested += quitRequested;
    }

    private static void DeathScreenPage_RespawnRequested()
    {
        RespawnClicked?.Invoke();
    }

    public static void Render(Vector2 currentWindowSize)
    {
        if (!_isOpen)
            return;

        _deathScreenPage.Measure(currentWindowSize);
        _deathScreenPage.Arrange(currentWindowSize);
        _deathScreenPage.Update(InputManager.MousePosition, currentWindowSize, SystemAPI.Input.IsMouseButtonDown(MouseButton.Left));
        _deathScreenPage.Draw();
    }
}
