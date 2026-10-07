using Crafty.SDK.Client;
using Crafty.SDK.Client.Blocks;

namespace Crafty.Engine.Core;

public sealed class BlockRegistry : IBlockRegistry
{
    private static readonly Dictionary<uint, Block> _blocks = [];
    private readonly Dictionary<BlockCategory, uint> _nextIds = new()
    {
        [BlockCategory.Nature] = 0,
        [BlockCategory.Stone] = 5_000,
        [BlockCategory.Wood] = 15_000,
        [BlockCategory.Ore] = 20_000,
        [BlockCategory.Building] = 30_000,
        [BlockCategory.Decoration] = 40_000,
        [BlockCategory.Technical] = 50_000
    };

    public Block Get(uint id)
    {
        if (!_blocks.TryGetValue(id, out var item))
            throw new KeyNotFoundException($"block ID {id} is not registered.");

        return item;
    }

    public uint[] GetAllIds()
    {
        return [.. _blocks.Keys];
    }

    public void Register(BlockCategory category, Block block)
    {
        var id = _nextIds[category]++;

        _blocks.Add(id, block);
    }
}
