using Crafty.SDK.Client.Blocks;
using System.Numerics;

namespace Crafty.Engine.MainMod;

public static class BlocksCreator
{
    private static BlockModel CreateOneSidedBlockModel(string textureId)
    {
        return new BlockModel()
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
            ],
        };
    }

    private static BlockModel CreateThreeSidedBlockModel(string textureId)
    {
        return new BlockModel()
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
                },
            ],
        };
    }

    private static BlockModel CreateOneTwoSidedBlockModel(string textureId)
    {
        return new BlockModel()
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
                            Texture = textureId + ".side",
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
            ],
        };
    }

    private static BlockModel CreateTwoSidedBlockModel(string textureId)
    {
        return new BlockModel()
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
                            Texture = textureId + ".top",
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
            ],
        };
    }

    private static BlockModel CreateGlassPaneModel(string textureId)
    {
        return new BlockModel()
        {
            Elements =
            [
                new()
                {
                    Min = new Vector3(0f, 0f, 0.46875f),
                    Max = new Vector3(1f, 1f, 0.53125f),

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
                            Texture = textureId + ".side",
                            Direction = BlockFaceDirection.Bottom,
                        },
                        new()
                        {
                            Texture = textureId + ".side",
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
            ]
        };
    }

    private static CollisionShape CreateBlockCollider()
    {
        return CollisionShape.FullCube;
    }

    private static CollisionShape CreateGlassPaneCollider()
    {
        return new([new BoundingBox(0.4375f, 0f, 0f, 0.5625f, 1f, 1f)]);
    }

    public static Block CreateGlassPaneBlock(string id, string name)
    {
        var block = new Block()
        {
            Id = id,
            Name = name,
            Model = CreateGlassPaneModel(id),
            Collision = CreateGlassPaneCollider(),
            Properties = new()
            {
                Opaque = false,
                Transparent = true
            },
            States = new()
            {
                Properties = new Dictionary<string, IReadOnlyList<string>>
                {
                    ["facing"] =
                    [
                        "none",
                        "north",
                        "south",
                        "east",
                        "west",
                    ]
                }
            }
        };

        return block;
    }

    public static Block CreateOneSidedTextureBlock(string id, string name)
    {
        var block = new Block()
        {
            Id = id,
            Name = name,
            Model = CreateOneSidedBlockModel(id),
            Collision = CreateBlockCollider(),
        };

        return block;
    }

    public static Block CreateOneTwoSidedTextureBlock(string id, string name)
    {
        var block = new Block()
        {
            Id = id,
            Name = name,
            Model = CreateOneTwoSidedBlockModel(id),
            Collision = CreateBlockCollider(),
        };

        return block;
    }

    public static Block CreateTwoSidedTextureBlock(string id, string name)
    {
        var block = new Block()
        {
            Id = id,
            Name = name,
            Model = CreateTwoSidedBlockModel(id),
            Collision = CreateBlockCollider(),
        };

        return block;
    }

    public static Block CreateAirBlock()
    {
        var block = new Block()
        {
            Name = "Air",
            Id = "crafty.air",
            Model = CreateOneSidedBlockModel("crafty.air"),
            Collision = new([])
        };

        return block;
    }

    public static Block CreateThreeSidedTextureBlock(string id, string name)
    {
        var block = new Block()
        {
            Id = id,
            Name = name,
            Model = CreateThreeSidedBlockModel(id),
            Collision = CreateBlockCollider(),
            States = new()
        };

        return block;
    }
}
