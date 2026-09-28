using Crafty.ChunkGeneration.World;
using Crafty.Engine.ChunkBuilding;
using Crafty.SDK.Client.Blocks;
using Crafty.SDK.World;
using CraftyNative.ThreeD;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.World;
using Silk.NET.SDL;

namespace Crafty.Engine;

public sealed class MeshBuilder
{
    private readonly List<float> _vertices = [];
    private readonly List<uint> _indices = [];

    private enum Face
    {
        Left,
        Right,
        Bottom,
        Top,
        Front,
        Back
    }

    public Mesh BuildBlockMesh()
    {
        AddFace(0, 0, 0, Face.Left);
        AddFace(0, 0, 0, Face.Right);
        AddFace(0, 0, 0, Face.Bottom);
        AddFace(0, 0, 0, Face.Top);
        AddFace(0, 0, 0, Face.Front);
        AddFace(0, 0, 0, Face.Back);

        return new Mesh([.. _vertices], [.. _indices], vertexStride: 20);
    }

    public Mesh BuildSectionMesh(World world, int sectionX, int sectionY, int sectionZ, int chunkX, int chunkZ)
    {
        _vertices.Clear();
        _indices.Clear();

        int startX = chunkX * Chunk.Size + sectionX * SectionCoordinate.SectionSize;

        int startY = sectionY * SectionCoordinate.SectionSize;

        int startZ = chunkZ * Chunk.Size + sectionZ * SectionCoordinate.SectionSize;

        int endX = startX + SectionCoordinate.SectionSize;
        int endY = startY + SectionCoordinate.SectionSize;
        int endZ = startZ + SectionCoordinate.SectionSize;

        var textures = new HashSet<string>();

        for (var x = startX; x < endX; x++)
        {
            for (var z = startZ; z < endZ; z++)
            {
                for (var y = startY; y < endY; y++)
                {
                    var block = world.GetBlock(x, y, z);

                    if (block.Id == 0)
                        continue;

                    foreach (var face in GameAPIs.BlockRegistry.Get(block.Id).Model.Faces)
                        textures.Add(face.Texture);
                }
            }
        }

        if (textures.Count == 0)
            return new Mesh([], [], vertexStride: 20);

        var atlas = ChunkTextureAtlasBuilder.Build(textures, 32);

        for (var x = startX; x < endX; x++)
        {
            for (var z = startZ; z < endZ; z++)
            {
                for (var y = startY; y < endY; y++)
                {
                    var block = world.GetBlock(x, y, z);

                    if (block.Id == 0)
                        continue;

                    var model = GameAPIs.BlockRegistry.Get(block.Id).Model;

                    int localX = x - startX;
                    int localY = y - startY;
                    int localZ = z - startZ;

                    if (world.GetBlock(x - 1, y, z).Id == 0)
                        AddBlockFace(localX, localY, localZ, Face.Left, model, atlas);

                    if (world.GetBlock(x + 1, y, z).Id == 0)
                        AddBlockFace(localX, localY, localZ, Face.Right, model, atlas);

                    if (y == 0 || world.GetBlock(x, y - 1, z).Id == 0)
                        AddBlockFace(localX, localY, localZ, Face.Bottom, model, atlas);

                    if (world.GetBlock(x, y + 1, z).Id == 0)
                        AddBlockFace(localX, localY, localZ, Face.Top, model, atlas);

                    if (world.GetBlock(x, y, z - 1).Id == 0)
                        AddBlockFace(localX, localY, localZ, Face.Front, model, atlas);

                    if (world.GetBlock(x, y, z + 1).Id == 0)
                        AddBlockFace(localX, localY, localZ, Face.Back, model, atlas);
                }
            }
        }

        return new Mesh([.. _vertices], [.. _indices], material: Material.Create(atlas.Image), vertexStride: 20);
    }

    private void AddBlockFace(int x, int y, int z, Face face, BlockModel model, ChunkTextureAtlas atlas)
    {
        var direction = ToBlockFaceDirection(face);
        var modelFace = GetModelFace(model, direction);
        var region = atlas.Get(modelFace.Texture);

        AddFace(x, y, z, face, region);
    }

    private void AddFace(int x, int y, int z, Face face)
    {
        var i = (uint)(_vertices.Count / 5);

        switch (face)
        {
            case Face.Left:
                AddVertex(x, y, z, 0, 1);
                AddVertex(x, y + 1, z, 1, 1);
                AddVertex(x, y + 1, z + 1, 1, 0);
                AddVertex(x, y, z + 1, 0, 0);
                break;

            case Face.Right:
                AddVertex(x + 1, y, z + 1, 0, 1);
                AddVertex(x + 1, y + 1, z + 1, 1, 1);
                AddVertex(x + 1, y + 1, z, 1, 0);
                AddVertex(x + 1, y, z, 0, 0);
                break;

            case Face.Bottom:
                AddVertex(x, y, z + 1, 0, 1);
                AddVertex(x, y, z, 1, 1);
                AddVertex(x + 1, y, z, 1, 0);
                AddVertex(x + 1, y, z + 1, 0, 0);
                break;

            case Face.Top:
                AddVertex(x, y + 1, z, 0, 1);
                AddVertex(x, y + 1, z + 1, 1, 1);
                AddVertex(x + 1, y + 1, z + 1, 1, 0);
                AddVertex(x + 1, y + 1, z, 0, 0);
                break;

            case Face.Front:
                AddVertex(x + 1, y, z, 0, 1);
                AddVertex(x + 1, y + 1, z, 1, 1);
                AddVertex(x, y + 1, z, 1, 0);
                AddVertex(x, y, z, 0, 0);
                break;

            case Face.Back:
                AddVertex(x, y, z + 1, 0, 1);
                AddVertex(x, y + 1, z + 1, 1, 1);
                AddVertex(x + 1, y + 1, z + 1, 1, 0);
                AddVertex(x + 1, y, z + 1, 0, 0);
                break;
        }

        _indices.Add(i);
        _indices.Add(i + 1);
        _indices.Add(i + 2);
        _indices.Add(i);
        _indices.Add(i + 2);
        _indices.Add(i + 3);
    }

    private void AddFace(int x, int y, int z, Face face, AtlasRegion region)
    {
        var i = (uint)(_vertices.Count / 5);

        switch (face)
        {
            case Face.Left:
                AddVertex(x, y, z, U(1, region), V(1, region));
                AddVertex(x, y + 1, z, U(1, region), V(0, region));
                AddVertex(x, y + 1, z + 1, U(0, region), V(0, region));
                AddVertex(x, y, z + 1, U(0, region), V(1, region));
                break;

            case Face.Right:
                AddVertex(x + 1, y, z + 1, U(1, region), V(1, region));
                AddVertex(x + 1, y + 1, z + 1, U(1, region), V(0, region));
                AddVertex(x + 1, y + 1, z, U(0, region), V(0, region));
                AddVertex(x + 1, y, z, U(0, region), V(1, region));
                break;

            case Face.Bottom:
                AddVertex(x, y, z + 1, U(0, region), V(1, region));
                AddVertex(x, y, z, U(1, region), V(1, region));
                AddVertex(x + 1, y, z, U(1, region), V(0, region));
                AddVertex(x + 1, y, z + 1, U(0, region), V(0, region));
                break;

            case Face.Top:
                AddVertex(x, y + 1, z, U(0, region), V(1, region));
                AddVertex(x, y + 1, z + 1, U(1, region), V(1, region));
                AddVertex(x + 1, y + 1, z + 1, U(1, region), V(0, region));
                AddVertex(x + 1, y + 1, z, U(0, region), V(0, region));
                break;

            case Face.Front:
                AddVertex(x + 1, y, z, U(1, region), V(1, region));
                AddVertex(x + 1, y + 1, z, U(1, region), V(0, region));
                AddVertex(x, y + 1, z, U(0, region), V(0, region));
                AddVertex(x, y, z, U(0, region), V(1, region));
                break;

            case Face.Back:
                AddVertex(x, y, z + 1, U(1, region), V(1, region));
                AddVertex(x, y + 1, z + 1, U(1, region), V(0, region));
                AddVertex(x + 1, y + 1, z + 1, U(0, region), V(0, region));
                AddVertex(x + 1, y, z + 1, U(0, region), V(1, region));
                break;
        }

        _indices.Add(i);
        _indices.Add(i + 1);
        _indices.Add(i + 2);
        _indices.Add(i);
        _indices.Add(i + 2);
        _indices.Add(i + 3);
    }

    private void AddVertex(float x, float y, float z, float u, float v)
    {
        _vertices.Add(x);
        _vertices.Add(y);
        _vertices.Add(z);
        _vertices.Add(u);
        _vertices.Add(v);
    }

    private static float U(float u, AtlasRegion region)
    {
        return region.U0 + u * (region.U1 - region.U0);
    }

    private static float V(float v, AtlasRegion region)
    {
        return region.V0 + v * (region.V1 - region.V0);
    }

    private static BlockFace GetModelFace(BlockModel model, BlockFaceDirection face)
    {
        return model.Faces.First(x => x.Direction == face);
    }

    private static BlockFaceDirection ToBlockFaceDirection(Face face)
    {
        return face switch
        {
            Face.Left => BlockFaceDirection.West,
            Face.Right => BlockFaceDirection.East,
            Face.Bottom => BlockFaceDirection.Bottom,
            Face.Top => BlockFaceDirection.Top,
            Face.Front => BlockFaceDirection.North,
            Face.Back => BlockFaceDirection.South,
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
    }
}
