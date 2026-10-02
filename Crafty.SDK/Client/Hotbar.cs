namespace Crafty.SDK.Client;

public sealed class Hotbar
{
    public const int SlotCount = 9;

    public int[] InventorySlots { get; } = new int[SlotCount];

    public int SelectedSlot { get; set; }
}
