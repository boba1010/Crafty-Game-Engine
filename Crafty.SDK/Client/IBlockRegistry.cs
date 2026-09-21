using Crafty.SDK.Client.Blocks;

namespace Crafty.SDK.Client;

public interface IBlockRegistry
{
    ushort Register(Block block);
    Block Get(ushort id);
    bool TryGet(string id, out Block block);
}
