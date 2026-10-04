using CraftyNative.ECS;
using System.Runtime.CompilerServices;

namespace Crafty.Engine.Components;

public struct Inventory : IComponent
{
    public InventorySlots Slots;
    public ArmorSlots Armor;
    public CraftingSlots Crafting;
}

public struct InventorySlot(ushort itemId, int count, ushort? blockId)
{
    public ushort ItemId = itemId;
    public ushort? BlockId = blockId;
    public int Count = count;
}

[InlineArray(4)]
public struct ArmorSlots
{
    private InventorySlot _element;
}

[InlineArray(5)]
public struct CraftingSlots
{
    private InventorySlot _element;
}

[InlineArray(36)]
public struct InventorySlots
{
    private InventorySlot _element;
}