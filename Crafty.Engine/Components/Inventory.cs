using CraftyNative.ECS;
using System.Runtime.CompilerServices;

namespace Crafty.Engine.Components;

public struct Inventory : IComponent
{
    public InventorySlots Slots;
}

public struct InventorySlot(ushort itemId, int count, ushort? blockId)
{
    public ushort ItemId = itemId;
    public ushort? BlockId = blockId;
    public int Count = count;
}

[InlineArray(27)]
public struct InventorySlots
{
    private InventorySlot _element;
}