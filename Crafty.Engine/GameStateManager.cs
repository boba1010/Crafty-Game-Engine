using CraftyNative;

namespace Crafty.Engine;

public static class GameStateManager
{
    public static GameState Current { get; private set; } = GameState.Gameplay;

    public static bool IsGameplay => Current == GameState.Gameplay;
    public static bool IsInventory => Current == GameState.Inventory;
    public static bool IsPaused => Current == GameState.Paused;

    public static void Set(GameState state)
    {
        if (Current == state)
            return;

        Current = state;
    }

    public static void Initialize()
    {
        SystemAPI.Input.KeyDown += Input_KeyDown;
    }

    private static bool _isPaused;
    private static void Input_KeyDown(Key key)
    {
        if (IsInventory)
            return;

        if (key != Key.Escape)
            return;

        _isPaused = !_isPaused;
        if (_isPaused)
        {
            Set(GameState.Paused);
            SystemAPI.Input.CursorMode = Cursor.Normal;
            SystemAPI.Input.CenterMouse(SystemAPI.WindowSize);
        }
        else
        {
            SystemAPI.Input.CursorMode = Cursor.Raw;
            Set(GameState.Gameplay);
        }
    }
}

public enum GameState
{
    Paused,
    Gameplay,
    Inventory,
}