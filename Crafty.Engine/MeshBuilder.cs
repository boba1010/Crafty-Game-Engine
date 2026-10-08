using Crafty.ChunkGeneration.World;
using Crafty.Engine.Core;
using Crafty.Engine.Helpers;
using Crafty.SDK.Client;
using Crafty.SDK.Client.Blocks;
using CraftyNative.ThreeD.Meshes;
using CraftyNative.ThreeD.World;
using System.Collections.Concurrent;
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

    // ------------------------------------------------------------------
    // Global atlas: built once, shared by every section, block icon and
    // therefore by exactly one GPU material.
    // ------------------------------------------------------------------

    private static ChunkTextureAtlas? _atlas;
    private static readonly object _atlasLock = new();

    public static ChunkTextureAtlas GlobalAtlas
    {
        get
        {
            if (_atlas is { } existing)
                return existing;

            lock (_atlasLock)
                return _atlas ??= BuildGlobalAtlas();
        }
    }

    private static ChunkTextureAtlas BuildGlobalAtlas()
    {
        var textures = new HashSet<string>();

        foreach (var block in GameAPIs.BlockRegistry.All)
            foreach (var element in block.Model.Elements)
                foreach (var face in element.Faces)
                    textures.Add(face.Texture);

        return ChunkTextureAtlasBuilder.Build(textures, 32);
    }

    // ------------------------------------------------------------------
    // Per-block render info (cached: the world calls this a lot)
    // ------------------------------------------------------------------

    private static readonly ConcurrentDictionary<uint, (bool Opaque, bool Translucent)> _renderInfo = new();

    private static (bool Opaque, bool Translucent) RenderInfo(uint id)
    {
        if (id == 0)
            return (false, false);

        return _renderInfo.GetOrAdd(id, static blockId =>
        {
            var properties = GameAPIs.BlockRegistry.Get(blockId).Properties;
            return (properties.Opaque, properties.Transparent);
        });
    }

    /// <summary>
    /// Should the face of block <paramref name="selfId"/> that touches
    /// <paramref name="neighborId"/> be drawn?
    /// </summary>
    private static bool ShouldDrawFace(uint selfId, uint neighborId, bool isTransparent)
    {
        if (neighborId == 0)
            return true;

        if (isTransparent)
            return true;

        if (!RenderInfo(neighborId).Opaque)
            return selfId != neighborId;

        return false;
    }

    // ------------------------------------------------------------------
    // Section meshing
    // ------------------------------------------------------------------

    public static SectionMeshes BuildSectionMesh(World world, int sectionX, int sectionY, int sectionZ, int chunkX, int chunkZ)
    {
        int startX = chunkX * Chunk.Size + sectionX * SectionCoordinate.SectionSize;
        int startY = sectionY * SectionCoordinate.SectionSize;
        int startZ = chunkZ * Chunk.Size + sectionZ * SectionCoordinate.SectionSize;

        int endX = startX + SectionCoordinate.SectionSize;
        int endY = startY + SectionCoordinate.SectionSize;
        int endZ = startZ + SectionCoordinate.SectionSize;

        var atlas = GlobalAtlas;

        var opaque = new MeshData();
        var translucent = new MeshData();

        for (var x = startX; x < endX; x++)
        {
            for (var z = startZ; z < endZ; z++)
            {
                for (var y = startY; y < endY; y++)
                {
                    var block = world.GetBlock(x, y, z);

                    if (block.Id == 0)
                        continue;

                    var blockDef = GameAPIs.BlockRegistry.Get(block.Id);
                    var model = blockDef.Model;

                    int localX = x - startX;
                    int localY = y - startY;
                    int localZ = z - startZ;

                    ref var target = ref (RenderInfo(block.Id).Translucent ? ref translucent : ref opaque);

                    foreach (var element in model.Elements)
                    {
                        if (ShouldDrawFace(block.Id, world.GetBlock(x - 1, y, z).Id, blockDef.Properties.Transparent))
                            AddElementFace(ref target, localX, localY, localZ, Face.Left, element, block.State, atlas);
                        if (ShouldDrawFace(block.Id, world.GetBlock(x + 1, y, z).Id, blockDef.Properties.Transparent))
                            AddElementFace(ref target, localX, localY, localZ, Face.Right, element, block.State, atlas);
                        if (y == 0 || ShouldDrawFace(block.Id, world.GetBlock(x, y - 1, z).Id, blockDef.Properties.Transparent))
                            AddElementFace(ref target, localX, localY, localZ, Face.Bottom, element, block.State, atlas);
                        if (ShouldDrawFace(block.Id, world.GetBlock(x, y + 1, z).Id, blockDef.Properties.Transparent))
                            AddElementFace(ref target, localX, localY, localZ, Face.Top, element, block.State, atlas);
                        if (ShouldDrawFace(block.Id, world.GetBlock(x, y, z - 1).Id, blockDef.Properties.Transparent))
                            AddElementFace(ref target, localX, localY, localZ, Face.Front, element, block.State, atlas);
                        if (ShouldDrawFace(block.Id, world.GetBlock(x, y, z + 1).Id, blockDef.Properties.Transparent))
                            AddElementFace(ref target, localX, localY, localZ, Face.Back, element, block.State, atlas);
                    }
                }
            }
        }

        return new SectionMeshes(Wrap(opaque, atlas), Wrap(translucent, atlas));
    }

    private static Mesh Wrap(MeshData data, ChunkTextureAtlas atlas)
    {
        if (data.Vertices.Count == 0)
            return new Mesh([], [], vertexStride: 20);

        return new Mesh(data.Vertices, data.Indices, atlas.Image, vertexStride: 20);
    }

    private static void AddElementFace(ref MeshData mesh, int x, int y, int z, Face face, BlockElement element, byte state, ChunkTextureAtlas atlas) 
    {
        var direction = ToBlockFaceDirection(face);
        var modelFace = GetModelFace(element, direction);
        var region = atlas.Get(modelFace.Texture);

        Vector3 min = element.Min;
        Vector3 max = element.Max;

        Vector3 v0;
        Vector3 v1;
        Vector3 v2;
        Vector3 v3;

        float u0;
        float v_0;
        float u1;
        float v_1;
        float u2;
        float v_2;
        float u3;
        float v_3;

        switch (face)
        {
            case Face.Left:
                v0 = new(min.X, min.Y, min.Z);
                v1 = new(min.X, max.Y, min.Z);
                v2 = new(min.X, max.Y, max.Z);
                v3 = new(min.X, min.Y, max.Z);

                u0 = 1f; v_0 = 1f;
                u1 = 1f; v_1 = 0f;
                u2 = 0f; v_2 = 0f;
                u3 = 0f; v_3 = 1f;
                break;

            case Face.Right:
                v0 = new(max.X, min.Y, max.Z);
                v1 = new(max.X, max.Y, max.Z);
                v2 = new(max.X, max.Y, min.Z);
                v3 = new(max.X, min.Y, min.Z);

                u0 = 1f; v_0 = 1f;
                u1 = 1f; v_1 = 0f;
                u2 = 0f; v_2 = 0f;
                u3 = 0f; v_3 = 1f;
                break;

            case Face.Bottom:
                v0 = new(min.X, min.Y, max.Z);
                v1 = new(min.X, min.Y, min.Z);
                v2 = new(max.X, min.Y, min.Z);
                v3 = new(max.X, min.Y, max.Z);

                u0 = 0f; v_0 = 1f;
                u1 = 1f; v_1 = 1f;
                u2 = 1f; v_2 = 0f;
                u3 = 0f; v_3 = 0f;
                break;

            case Face.Top:
                v0 = new(min.X, max.Y, min.Z);
                v1 = new(min.X, max.Y, max.Z);
                v2 = new(max.X, max.Y, max.Z);
                v3 = new(max.X, max.Y, min.Z);

                u0 = 0f; v_0 = 1f;
                u1 = 1f; v_1 = 1f;
                u2 = 1f; v_2 = 0f;
                u3 = 0f; v_3 = 0f;
                break;

            case Face.Front:
                v0 = new(max.X, min.Y, min.Z);
                v1 = new(max.X, max.Y, min.Z);
                v2 = new(min.X, max.Y, min.Z);
                v3 = new(min.X, min.Y, min.Z);

                u0 = 1f; v_0 = 1f;
                u1 = 1f; v_1 = 0f;
                u2 = 0f; v_2 = 0f;
                u3 = 0f; v_3 = 1f;
                break;

            case Face.Back:
                v0 = new(min.X, min.Y, max.Z);
                v1 = new(min.X, max.Y, max.Z);
                v2 = new(max.X, max.Y, max.Z);
                v3 = new(max.X, min.Y, max.Z);

                u0 = 1f; v_0 = 1f;
                u1 = 1f; v_1 = 0f;
                u2 = 0f; v_2 = 0f;
                u3 = 0f; v_3 = 1f;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(face), face, null);
        }

        // Only directional blocks should be rotated.
        if (state != 0)
        {
            v0 = RotateY(v0, state);
            v1 = RotateY(v1, state);
            v2 = RotateY(v2, state);
            v3 = RotateY(v3, state);
        }

        var offset = new Vector3(x, y, z);

        v0 += offset;
        v1 += offset;
        v2 += offset;
        v3 += offset;

        var i = (uint)(mesh.Vertices.Count / 5);

        AddVertex(ref mesh, v0.X, v0.Y, v0.Z, U(u0, region), V(v_0, region));
        AddVertex(ref mesh, v1.X, v1.Y, v1.Z, U(u1, region), V(v_1, region));
        AddVertex(ref mesh, v2.X, v2.Y, v2.Z, U(u2, region), V(v_2, region));
        AddVertex(ref mesh, v3.X, v3.Y, v3.Z, U(u3, region), V(v_3, region));

        mesh.Indices.AddRange(face switch
        {
            Face.Left or Face.Right or Face.Front or Face.Back =>
            [
                i, i + 2, i + 1,
                i, i + 3, i + 2
            ],

            Face.Bottom or Face.Top =>
            [
                i, i + 1, i + 2,
                i, i + 2, i + 3
            ],

            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        });
    }

    private static Vector3 RotateY(Vector3 position, byte state)
    {
        position -= new Vector3(0.5f);

        position = state switch
        {
            0 => position, // none
            1 => position, // north
            2 => new Vector3(-position.X, position.Y, -position.Z), // south
            3 => new Vector3(position.Z, position.Y, -position.X), // east
            4 => new Vector3(-position.Z, position.Y, position.X), // west

            _ => position
        };

        return position + new Vector3(0.5f);
    }

    public static Mesh BuildBlockMesh(uint blockId)
    {
        var model = GameAPIs.BlockRegistry.Get(blockId).Model;

        if (model.Elements.Count == 0)
            return new Mesh([], [], vertexStride: 20);

        var atlas = GlobalAtlas;
        var mesh = new MeshData();

        foreach (var element in model.Elements)
            AddElement(ref mesh, 0, 0, 0, element, 0, atlas);

        return new Mesh(mesh.Vertices, mesh.Indices, atlas.Image, vertexStride: 20);
    }

    private static void AddElement(ref MeshData mesh, int x, int y, int z, BlockElement element, byte state, ChunkTextureAtlas atlas)
    {
        foreach (var face in Enum.GetValues<Face>())
            AddElementFace(ref mesh, x, y, z, face, element, state, atlas);
    }

    public static Mesh BuildItemMesh(Item item)
    {
        if (item.Texture is not { } texture)
            return new Mesh([], [], vertexStride: 20);

        var atlas = ChunkTextureAtlasBuilder.Build(
            new HashSet<string> { texture.Path },
            32);

        var region = atlas.Get(texture.Path);
        var mesh = new MeshData();

        AddItemFace(
            ref mesh,
            -0.5f, -0.5f, 0f,
             0.5f, -0.5f, 0f,
             0.5f, 0.5f, 0f,
            -0.5f, 0.5f, 0f);

        AddItemFace(
            ref mesh,
            -0.5f, -0.5f, 0f,
            -0.5f, 0.5f, 0f,
             0.5f, 0.5f, 0f,
             0.5f, -0.5f, 0f);

        return new Mesh(mesh.Vertices, mesh.Indices, atlas.Image, vertexStride: 20);
    }

    private static void AddItemFace(
        ref MeshData mesh,
        float x0, float y0, float z0,
        float x1, float y1, float z1,
        float x2, float y2, float z2,
        float x3, float y3, float z3)
    {
        var i = (uint)(mesh.Vertices.Count / 5);

        mesh.Vertices.AddRange([
            x0, y0, z0, 0f, 1f,
            x1, y1, z1, 1f, 1f,
            x2, y2, z2, 1f, 0f,
            x3, y3, z3, 0f, 0f
        ]);

        mesh.Indices.AddRange([
            i, i + 1, i + 2,
            i, i + 2, i + 3
        ]);
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

    private static BlockFace GetModelFace(BlockElement element, BlockFaceDirection face)
    {
        return element.Faces.First(x => x.Direction == face);
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