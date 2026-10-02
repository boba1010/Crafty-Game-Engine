using Crafty.ChunkGeneration.World;
using Crafty.Engine.Core;
using Crafty.Engine.Helpers;
using Crafty.SDK.Client.Blocks;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.World;

namespace Crafty.Engine;



public static class MeshBuilder
{
    private enum Face
    {
        Left,
        Right,
        Bottom,
        Top,
        Front,
        Back
    }

    private struct MeshData
    {
        public List<float> Vertices;
        public List<uint> Indices;

        public MeshData()
        {
            Vertices = [];
            Indices = [];
        }
    }

    public static Mesh BuildSectionMesh(World world, int sectionX, int sectionY, int sectionZ, int chunkX, int chunkZ)
    {
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

        var mesh = new MeshData();

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
                        AddBlockFace(ref mesh, localX, localY, localZ, Face.Left, model, atlas);

                    if (world.GetBlock(x + 1, y, z).Id == 0)
                        AddBlockFace(ref mesh, localX, localY, localZ, Face.Right, model, atlas);

                    if (y == 0 || world.GetBlock(x, y - 1, z).Id == 0)
                        AddBlockFace(ref mesh, localX, localY, localZ, Face.Bottom, model, atlas);

                    if (world.GetBlock(x, y + 1, z).Id == 0)
                        AddBlockFace(ref mesh, localX, localY, localZ, Face.Top, model, atlas);

                    if (world.GetBlock(x, y, z - 1).Id == 0)
                        AddBlockFace(ref mesh, localX, localY, localZ, Face.Front, model, atlas);

                    if (world.GetBlock(x, y, z + 1).Id == 0)
                        AddBlockFace(ref mesh, localX, localY, localZ, Face.Back, model, atlas);
                }
            }
        }

        return new Mesh(mesh.Vertices, mesh.Indices, atlas.Image, vertexStride: 20);
    }

    private static void AddBlockFace(ref MeshData mesh, int x, int y, int z, Face face, BlockModel model, ChunkTextureAtlas atlas)
    {
        var direction = ToBlockFaceDirection(face);
        var modelFace = GetModelFace(model, direction);
        var region = atlas.Get(modelFace.Texture);

        AddFace(x, y, z, face, region, ref mesh);
    }

    private static void AddFace(int x, int y, int z, Face face, AtlasRegion region, ref MeshData mesh)
    {
        var indices = mesh.Indices;

        var i = (uint)(mesh.Vertices.Count / 5);

        switch (face)
        {
            case Face.Left:
                AddVertex(ref mesh, x, y, z, U(1, region), V(1, region));
                AddVertex(ref mesh, x, y + 1, z, U(1, region), V(0, region));
                AddVertex(ref mesh, x, y + 1, z + 1, U(0, region), V(0, region));
                AddVertex(ref mesh, x, y, z + 1, U(0, region), V(1, region));
                break;

            case Face.Right:
                AddVertex(ref mesh, x + 1, y, z + 1, U(1, region), V(1, region));
                AddVertex(ref mesh, x + 1, y + 1, z + 1, U(1, region), V(0, region));
                AddVertex(ref mesh, x + 1, y + 1, z, U(0, region), V(0, region));
                AddVertex(ref mesh, x + 1, y, z, U(0, region), V(1, region));
                break;

            case Face.Bottom:
                AddVertex(ref mesh, x, y, z + 1, U(0, region), V(1, region));
                AddVertex(ref mesh, x, y, z, U(1, region), V(1, region));
                AddVertex(ref mesh, x + 1, y, z, U(1, region), V(0, region));
                AddVertex(ref mesh, x + 1, y, z + 1, U(0, region), V(0, region));
                break;

            case Face.Top:
                AddVertex(ref mesh, x, y + 1, z, U(0, region), V(1, region));
                AddVertex(ref mesh, x, y + 1, z + 1, U(1, region), V(1, region));
                AddVertex(ref mesh, x + 1, y + 1, z + 1, U(1, region), V(0, region));
                AddVertex(ref mesh, x + 1, y + 1, z, U(0, region), V(0, region));
                break;

            case Face.Front:
                AddVertex(ref mesh, x + 1, y, z, U(1, region), V(1, region));
                AddVertex(ref mesh, x + 1, y + 1, z, U(1, region), V(0, region));
                AddVertex(ref mesh, x, y + 1, z, U(0, region), V(0, region));
                AddVertex(ref mesh, x, y, z, U(0, region), V(1, region));
                break;

            case Face.Back:
                AddVertex(ref mesh, x, y, z + 1, U(1, region), V(1, region));
                AddVertex(ref mesh, x, y + 1, z + 1, U(1, region), V(0, region));
                AddVertex(ref mesh, x + 1, y + 1, z + 1, U(0, region), V(0, region));
                AddVertex(ref mesh, x + 1, y, z + 1, U(0, region), V(1, region));
                break;
        }

        indices.Add(i);
        indices.Add(i + 1);
        indices.Add(i + 2);
        indices.Add(i);
        indices.Add(i + 2);
        indices.Add(i + 3);
    }

    public static Mesh BuildBlockMesh(ushort blockId)
    {
        var model = GameAPIs.BlockRegistry.Get(blockId).Model;

        var textures = new HashSet<string>();
        foreach (var face in model.Faces)
            textures.Add(face.Texture);

        if (textures.Count == 0)
            return new Mesh([], [], vertexStride: 20);

        var atlas = ChunkTextureAtlasBuilder.Build(textures, 32);
        var mesh = new MeshData();

        // One block at the origin, all six faces (no neighbours to cull against)
        foreach (var face in Enum.GetValues<Face>())
            AddBlockFace(ref mesh, 0, 0, 0, face, model, atlas);

        return new Mesh(mesh.Vertices, mesh.Indices, atlas.Image, vertexStride: 20);
    }

    private static void AddVertex(ref MeshData mesh, float x, float y, float z, float u, float v)
    {
        var vertices = mesh.Vertices;

        vertices.Add(x);
        vertices.Add(y);
        vertices.Add(z);
        vertices.Add(u);
        vertices.Add(v);
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
