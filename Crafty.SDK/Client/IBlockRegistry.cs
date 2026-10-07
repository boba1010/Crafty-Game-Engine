using Crafty.SDK.Client.Blocks;

namespace Crafty.SDK.Client;

public interface IBlockRegistry
{
    public void Register(BlockCategory category, Block block);
    Block Get(uint id);
    uint[] GetAllIds();
}
