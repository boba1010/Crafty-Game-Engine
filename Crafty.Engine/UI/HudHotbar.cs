using CraftyNative.HUD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.UI;
using System.Numerics;

namespace Crafty.Engine.UI;

public static class HudHotbar
{
    private static int _selectedSlot = 0;

    private static readonly SlotGrid _hotbar = new()
    {
        Columns = 9,
        HorizontalSpacing = 4,
        VerticalSpacing = 4,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new(0, 0, 0, 20)
    };

    public static void SetSelectedSlot(int slotIndex)
    {
        _selectedSlot = slotIndex;
        foreach (var slot in _hotbar.Children)
            slot.IsSelected = false;

        _hotbar.Children[slotIndex].IsSelected = true;
    }

    public static void SetSlot(int slotIndex, Mesh? mesh, int count = 1)
    {
        var slot = _hotbar.Children[slotIndex];
        slot.Content = new()
        {
            Model = mesh,
            Rotation = new(MathF.PI / 6, MathF.PI / 4f, 0f)
        };
        slot.Count = count;
    }

    public static void Initialize()
    {
        for (int i = 0; i < 9; i++)
        {
            var slot = new Slot { Size = new Vector2(44, 44), };
            _hotbar.Add(slot);
        }

        _hotbar.Children[0].IsSelected = true;
    }

    public static void Render(Vector2 currentWindowSize)
    {
        _hotbar.Measure(currentWindowSize);
        _hotbar.Arrange(currentWindowSize);

        _hotbar.Update(Vector2.Zero, currentWindowSize, false);

        _hotbar.Draw();
    }
}
