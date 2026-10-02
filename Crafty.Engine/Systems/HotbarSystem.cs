using Crafty.Engine.Components;
using CraftyNative;
using CraftyNative.ECS;
using CraftyNative.Scenes;

namespace Crafty.Engine.Systems;

public sealed class HotbarSystem : ISystem
{
    private int _selectedSlot;

    public HotbarSystem()
    {
        SystemAPI.Input.KeyDown += Input_KeyDown;
    }

    private void Input_KeyDown(Key key)
    {
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

        if (slot < 0 || slot == _selectedSlot)
            return;

        _selectedSlot = slot;
        CraftyNative.HUD.Hotbar.SetSelectedSlot(slot);
    }

    public void Update(ref Scene scene, double deltaTime)
    {
        foreach (var entity in scene.GetEntitiesWith<Hotbar>())
        {
            ref var hotbar = ref scene.GetComponent<Hotbar>(entity);
            hotbar.SelectedSlot = _selectedSlot;
        }
    }
}
