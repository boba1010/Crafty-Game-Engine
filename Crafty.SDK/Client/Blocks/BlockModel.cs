namespace Crafty.SDK.Client.Blocks;

public sealed class BlockModel
{
    public static BlockModel Cube => new()
    {
        Faces = 
        [
            new(BlockFaceDirection.Top, "crafty:missing"),
            new(BlockFaceDirection.Bottom, "crafty:missing"),
            new(BlockFaceDirection.North, "crafty:missing"),
            new(BlockFaceDirection.South, "crafty:missing"),
            new(BlockFaceDirection.East, "crafty:missing"),
            new(BlockFaceDirection.West, "crafty:missing"),
        ]
    };

    public IReadOnlyList<BlockFace> Faces { get; init; } = [];
}
