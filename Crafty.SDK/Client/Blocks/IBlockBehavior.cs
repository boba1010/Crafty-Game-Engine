namespace Crafty.SDK.Client.Blocks;

public interface IBlockBehavior
{
    void OnPlaced(BlockContext context);
    void OnBroken(BlockContext context);
    void OnNeighborChanged(BlockContext context);
    void OnUse(BlockContext context);
}
