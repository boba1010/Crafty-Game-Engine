using Crafty.SDK.Client.Blocks;

namespace Crafty.Engine.MainMod;

public static class BlocksCreator
{
    public static BlockModel CreateOneSidedBlockModel(string textureId)
    {
        return new BlockModel()
        {
            Faces =
            [
                new()
                {
                    Texture = textureId,
                    Direction = BlockFaceDirection.West,
                },
                new()
                {
                    Texture = textureId,
                    Direction = BlockFaceDirection.East,
                },
                new()
                {
                    Texture = textureId,
                    Direction = BlockFaceDirection.Bottom,
                },
                new()
                {
                    Texture = textureId,
                    Direction = BlockFaceDirection.Top,
                },
                new()
                {
                    Texture = textureId,
                    Direction = BlockFaceDirection.South,
                },
                new()
                {
                    Texture = textureId,
                    Direction = BlockFaceDirection.North,
                },
            ]
        };
    }

    private static BlockModel CreateThreeSidedBlockModel(string textureId)
    {
        return new BlockModel()
        {
            Faces = 
            [
                new()
                {
                    Texture = textureId + ".side",
                    Direction = BlockFaceDirection.West,
                },
                new()
                {
                    Texture = textureId + ".side",
                    Direction = BlockFaceDirection.East,
                },
                new()
                {
                    Texture = textureId + ".bottom",
                    Direction = BlockFaceDirection.Bottom,
                },
                new()
                {
                    Texture = textureId + ".top",
                    Direction = BlockFaceDirection.Top,
                },
                new()
                {
                    Texture = textureId + ".side",
                    Direction = BlockFaceDirection.South,
                },
                new()
                {
                    Texture = textureId + ".side",
                    Direction = BlockFaceDirection.North,
                },
            ]
        };
    }

    private static CollisionShape CreateBlockCollider()
    {
        return CollisionShape.FullCube;
    }

    public static Block CreateOneSidedTextureBlock(string textureId)
    {
        var block = new Block()
        {
            Model = CreateOneSidedBlockModel(textureId),
            Collision = CreateBlockCollider(),
        };

        return block;
    }

    public static Block CreateAirBlock()
    {
        var block = new Block()
        {
            Model = CreateOneSidedBlockModel("crafty.air"),
            Collision = new([])
        };

        return block;
    }

    public static Block CreateThreeSidedTextureBlock(string textureId)
    {
        var block = new Block()
        {
            Model = CreateThreeSidedBlockModel(textureId),
            Collision = CreateBlockCollider(),
        };

        return block;
    }
}
