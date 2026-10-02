using Crafty.SDK.Client;
using Crafty.SDK.Client.Blocks;

namespace Crafty.Engine.Core;

public sealed class BlockRegistry : IBlockRegistry
{
    private static Dictionary<ushort, Block> blocks = [];
    private static ushort _nextId;

    public Block Get(ushort id)
    {
        return blocks[id];
    }

    public void Register(Block block)
    {
        blocks[_nextId++] = block;
    }
}
