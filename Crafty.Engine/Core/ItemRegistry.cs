using Crafty.SDK.Client;

namespace Crafty.Engine.Core;

public sealed class ItemRegistry : IItemRegistry
{
    private static readonly Dictionary<uint, Item> _items = [];
    private readonly Dictionary<ItemCategory, uint> _nextIds = new()
    {
        [ItemCategory.Block] = 0,
        [ItemCategory.Food] = 60_000,
        [ItemCategory.Material] = 65_000,
        [ItemCategory.Tool] = 70_000,
        [ItemCategory.Utility] = 75_000,
        [ItemCategory.Weapon] = 80_000,
        [ItemCategory.Armor] = 85_000,
        [ItemCategory.Consumable] = 90_000,
    };

    public void Register(ItemCategory category, Item item)
    {
        var id = _nextIds[category]++;

        _items.Add(id, item);
    }

    public Item Get(uint id)
    {
        if (!_items.TryGetValue(id, out var item))
            throw new KeyNotFoundException($"Item ID {id} is not registered.");

        return item;
    }

    public uint[] GetAllIds()
    {
        return [.. _items.Keys];
    }
}
