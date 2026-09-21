namespace Crafty.SDK.Client.Blocks;

public interface IBlockWorld
{
    BlockState GetBlock(BlockPosition position);
    void SetBlock(BlockPosition position, BlockState state);
}
