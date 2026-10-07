using Crafty.SDK.Client;

namespace Crafty.Engine.Core;

public sealed class ItemRegistry : IItemRegistry
{
    private static readonly Dictionary<ushort, Item> _items = [];
    private ushort _nextId;

    public void Register(Item item)
    {
        _items[_nextId++] = item;
    }

    public Item Get(ushort id)
    {
        if (!_items.TryGetValue(id, out var item))
            throw new KeyNotFoundException($"Item ID {id} is not registered.");

        return item;
    }

    public ushort[] GetAllIds()
    {
        return [.. _items.Keys];
    }
}
