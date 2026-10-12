using CraftyNative.HUD;

namespace Crafty.Engine.UI.Inventory;

/// <summary>
/// The slot groups an inventory page exposes, so HudInventory can sync any page (survival, creative, ...)
/// with the ECS. Groups a page doesn't have are null.
/// </summary>
public interface IInventoryLayout
{
    SlotGrid Storage { get; }
    SlotGrid ArmorSlots { get; }
    SlotGrid? CraftingSlots { get; }
    SlotGrid? CraftingResultSlot { get; }
}