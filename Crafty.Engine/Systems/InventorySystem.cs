using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.HUD;
using CraftyNative.Scenes;

namespace Crafty.Engine.Systems;

public sealed class InventorySystem : ISystem
{
    private bool _isOpen;

    public InventorySystem()
    {
        SystemAPI.Input.KeyDown += Input_KeyDown;
    }

    private void Input_KeyDown(Key key)
    {
        if (GameStateManager.IsPaused)
            return;

        if (key == Key.E)
        {
            _isOpen = !_isOpen;

            if (_isOpen)
            {
                Inventory.Open();
                GameStateManager.Set(GameState.Inventory);
                SystemAPI.Input.CursorMode = Cursor.Normal;
            }
            else
            {
                Inventory.Close();
                GameStateManager.Set(GameState.Gameplay);
                SystemAPI.Input.CursorMode = Cursor.Raw;
            }
        }
        else if (key == Key.Escape && GameStateManager.IsInventory)
        {
            _isOpen = false;
            Inventory.Close();
            GameStateManager.Set(GameState.Gameplay);
            SystemAPI.Input.CursorMode = Cursor.Raw;
        }

    }

    public void Update(ref Scene scene, double deltaTime)
    {

    }
}
