using Crafty.SDK.Client;
using Crafty.SDK.Client.Blocks;

namespace Crafty.Engine.Core;

public sealed class BlockRegistry : IBlockRegistry
{
    private static readonly Dictionary<ushort, Block> _blocks = [];
    private ushort _nextId = 0;

    public Block Get(ushort id)
    {
        if (!_blocks.TryGetValue(id, out var item))
            throw new KeyNotFoundException($"block ID {id} is not registered.");

        return item;
    }

    public ushort[] GetAllIds()
    {
        return [.. _blocks.Keys];
    }

    public void Register(Block block)
    {
        _blocks[_nextId++] = block;
    }
}
