namespace Crafty.SDK.Client.Blocks;

public class Block
{
    public string Id { get; set; } = null!;
    public BlockProperties Properties { get; init; } = new();
    public BlockModel Model { get; init; } = BlockModel.Cube;
    public BlockStateDefinition? States { get; init; }
    public CollisionShape Collision { get; init; }
    public SelectionShape Selection { get; init; }
    public IBlockBehavior? Behavior { get; init; }
}
