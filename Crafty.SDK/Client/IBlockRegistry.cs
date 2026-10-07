using Crafty.SDK.Client.Blocks;

namespace Crafty.SDK.Client;

public interface IBlockRegistry
{
    void Register(Block block);
    Block Get(ushort id);
    ushort[] GetAllIds();
}
