using Crafty.SDK.Client.Blocks;

namespace Crafty.Engine.MainMod;

public sealed class BlocksCreator
{
    public Block CreateOneSidedTextureBlock(string textureId)
    {
        var block = new Block()
        {
            Model = new()
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
            }
        };

        return block;
    }
    public Block CreateThreeSidedTextureBlock(string textureId)
    {
        var block = new Block()
        {
            Model = new()
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
            }
        };

        return block;
    }
}
