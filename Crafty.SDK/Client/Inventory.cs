namespace Crafty.SDK.Client;

public sealed class Inventory(int size)
{
    public InventorySlot[] Slots { get; } = new InventorySlot[size];
}

public sealed class InventorySlot
{
    public ushort ItemId { get; set; }
    public uint Count { get; set; }
}