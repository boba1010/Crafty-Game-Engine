using CraftyNative.HUD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.UI;
using System.Numerics;

namespace Crafty.Engine.UI;

public static class HudHotbar
{
    public const int Columns = 9;
    public const int SlotSize = 44;
    public const int Spacing = 4;
    public const int BottomMargin = 20;

    public static float Width => Columns * SlotSize + (Columns - 1) * Spacing;

    /// <summary>Top-left corner of the hotbar for a given window size (it is centred horizontally, bottom-aligned).</summary>
    public static Vector2 GetTopLeft(Vector2 windowSize) =>
        new((windowSize.X - Width) / 2f, windowSize.Y - BottomMargin - SlotSize);

    private static int _selectedSlot = 0;

    private static readonly SlotGrid _hotbar = new()
    {
        Columns = Columns,
        HorizontalSpacing = Spacing,
        VerticalSpacing = Spacing,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new(0, 0, 0, BottomMargin)
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
        for (int i = 0; i < Columns; i++)
        {
            var slot = new Slot { Size = new Vector2(SlotSize, SlotSize), };
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