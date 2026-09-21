namespace Crafty.SDK.Client.Blocks;

public sealed class BlockContext
{
    public BlockState State { get; init; } = null!;
    public BlockPosition Position { get; init; }
    public IBlockWorld World { get; init; } = null!;
}
