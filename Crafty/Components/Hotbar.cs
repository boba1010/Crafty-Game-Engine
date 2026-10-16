using CraftyNative.ECS;
using System.Runtime.CompilerServices;

namespace Crafty.Components;

public struct Hotbar() : IComponent
{
    public HotbarSlots Slots = new();
    public int SelectedSlot { get; set; }
}

[InlineArray(9)]
public struct HotbarSlots
{
    private InventorySlot _element;
}