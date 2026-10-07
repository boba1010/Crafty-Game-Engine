using Crafty.ChunkGeneration.World;
using Crafty.Engine.Core;
using Crafty.Engine.Helpers;
using Crafty.SDK.Client;
using Crafty.SDK.Client.Blocks;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.World;
using System.Numerics;

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

    public static Mesh BuildItemMesh(Item item)
    {
        if (item.Texture is not { } texture)
            return new Mesh([], [], vertexStride: 20);

        var atlas = ChunkTextureAtlasBuilder.Build(new HashSet<string> { texture.Path }, 32);
        var region = atlas.Get(texture.Path);
        var mesh = new MeshData();

        AddFace(0, 0, 0, Face.Front, region, ref mesh);
        AddFace(0, 0, 0, Face.Back, region, ref mesh);

        for (int i = 2; i < mesh.Vertices.Count; i += 5)
            mesh.Vertices[i] = 0.5f;

        return new Mesh(mesh.Vertices, mesh.Indices, atlas.Image, vertexStride: 20);
    }

    public static Mesh BuildPlayerMesh()
    {
        List<float> vertices = [];
        List<uint> indices = [];

        AddBox(vertices, indices, new(-0.25f, 1.5f, -0.25f), new(0.25f, 2.0f, 0.25f)); // Head
        AddBox(vertices, indices, new(-0.25f, 0.75f, -0.125f), new(0.25f, 1.5f, 0.125f)); // Torso
        AddBox(vertices, indices, new(-0.5f, 0.75f, -0.125f), new(-0.25f, 1.5f, 0.125f)); // Left arm
        AddBox(vertices, indices, new(0.25f, 0.75f, -0.125f), new(0.5f, 1.5f, 0.125f)); // Right arm
        AddBox(vertices, indices, new(-0.25f, 0.0f, -0.125f), new(0.0f, 0.75f, 0.125f)); // Left leg
        AddBox(vertices, indices, new(0.0f, 0.0f, -0.125f), new(0.25f, 0.75f, 0.125f)); // Right leg

        var pixels = new byte[64 * 64];

        return new(vertices, indices, new(pixels, 64, 64), 20);
    }

    public static Mesh BuildPlayerHandMesh()
    {
        var vertices = new List<float>();
        var indices = new List<uint>();

        // 0.25 x 0.25 cross-section, 0.75 long. Origin = shoulder end, arm extends forward (-Z).
        AddBox(vertices, indices, new(-0.125f, -0.125f, -0.75f), new(0.125f, 0.125f, 0f));

        var pixels = new byte[64 * 64];

        return new(vertices, indices, new(pixels, 64, 64), 20);
    }

    private static void AddBox(List<float> vertices, List<uint> indices, Vector3 min, Vector3 max)
    {
        uint start = (uint)(vertices.Count / 5);

        AddFace(
            vertices,
            min.X, min.Y, min.Z,
            max.X, min.Y, min.Z,
            max.X, max.Y, min.Z,
            min.X, max.Y, min.Z);

        AddFace(
            vertices,
            max.X, min.Y, max.Z,
            min.X, min.Y, max.Z,
            min.X, max.Y, max.Z,
            max.X, max.Y, max.Z);

        AddFace(
            vertices,
            min.X, min.Y, max.Z,
            min.X, min.Y, min.Z,
            min.X, max.Y, min.Z,
            min.X, max.Y, max.Z);

        AddFace(
            vertices,
            max.X, min.Y, min.Z,
            max.X, min.Y, max.Z,
            max.X, max.Y, max.Z,
            max.X, max.Y, min.Z);

        AddFace(
            vertices,
            min.X, max.Y, min.Z,
            max.X, max.Y, min.Z,
            max.X, max.Y, max.Z,
            min.X, max.Y, max.Z);

        AddFace(
            vertices,
            min.X, min.Y, max.Z,
            max.X, min.Y, max.Z,
            max.X, min.Y, min.Z,
            min.X, min.Y, min.Z);

        for (uint face = 0; face < 6; face++)
        {
            uint offset = start + face * 4;

            indices.AddRange([
                offset + 0, offset + 1, offset + 2,
            offset + 2, offset + 3, offset + 0
            ]);
        }
    }

    private static void AddFace(
        List<float> vertices,
        float x0, float y0, float z0,
        float x1, float y1, float z1,
        float x2, float y2, float z2,
        float x3, float y3, float z3)
    {
        vertices.AddRange([
            x0, y0, z0, 0f, 1f,
            x1, y1, z1, 1f, 1f,
            x2, y2, z2, 1f, 0f,
            x3, y3, z3, 0f, 0f
        ]);
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

public enum PlayerPart
{
    Head,
    Body,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg
}