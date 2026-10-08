using System.Numerics;

namespace Crafty.SDK.Client.Blocks;

public sealed class BlockModel
{
    public static BlockModel Cube = new()
    {
        Elements = 
        [
            new()
                {
                    Min = Vector3.Zero,
                    Max = Vector3.One,
                    Faces =
                    [
                        new()
                        {
                            Texture = "crafty.missing",
                            Direction = BlockFaceDirection.West,
                        },
                        new()
                        {
                            Texture = "crafty.missing",
                            Direction = BlockFaceDirection.East,
                        },
                        new()
                        {
                            Texture = "crafty.missing",
                            Direction = BlockFaceDirection.Bottom,
                        },
                        new()
                        {
                            Texture = "crafty.missing",
                            Direction = BlockFaceDirection.Top,
                        },
                        new()
                        {
                            Texture = "crafty.missing",
                            Direction = BlockFaceDirection.South,
                        },
                        new()
                        {
                            Texture = "crafty.missing",
                            Direction = BlockFaceDirection.North,
                        },
                    ]
                }
        ],
    };

    public IReadOnlyList<BlockElement> Elements { get; init; } = [];
}
