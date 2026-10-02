using Crafty.SDK.Client;
using CraftyNative;

namespace Crafty.Engine.Gameplay;

public static class HotbarManager
{
    private static Player _player = null!;

    public static void Initialize(Player player)
    {
        _player = player;
        SystemAPI.Input.KeyDown += Input_KeyDown;
    }

    private static void Input_KeyDown(Key key)
    {
        if (_player is null)
            return;

        int slot = key switch
        {
            Key.Number1 => 0,
            Key.Number2 => 1,
            Key.Number3 => 2,
            Key.Number4 => 3,
            Key.Number5 => 4,
            Key.Number6 => 5,
            Key.Number7 => 6,
            Key.Number8 => 7,
            Key.Number9 => 8,
            _ => -1
        };

        if (slot >= 0)
            _player.Hotbar.SelectedSlot = slot;
    }
}
