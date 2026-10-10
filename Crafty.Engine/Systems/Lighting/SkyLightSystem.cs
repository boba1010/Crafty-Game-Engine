using Crafty.ChunkGeneration.World;
using Crafty.Engine.Core;
using Crafty.SDK.World;
using CraftyNative.ThreeD.Meshes;

namespace Crafty.Engine.Systems.Lighting;

public static class SkyLightSystem
{
    private const int Height = 416;
    private const int SectionSize = 16;
    private const byte MaxLight = 15;

    private static readonly (int X, int Y, int Z)[] Neighbors =
    [
        (1, 0, 0),
        (-1, 0, 0),
        (0, 1, 0),
        (0, -1, 0),
        (0, 0, 1),
        (0, 0, -1)
    ];

    public static void InitializeChunk(IChunk chunk)
    {
        for (int x = 0; x < Chunk.Size; x++)
        {
            for (int z = 0; z < Chunk.Size; z++)
                RecalculateColumn(chunk, x, z);
        }
    }

    public static void InitializeChunk(World world, IChunk chunk)
    {
        InitializeChunk(chunk);

        Queue<(int X, int Y, int Z)> queue = new();
        HashSet<(int X, int Y, int Z)> queued = [];

        int originX = chunk.X * Chunk.Size;
        int originZ = chunk.Z * Chunk.Size;

        for (int x = 0; x < Chunk.Size; x++)
        {
            for (int z = 0; z < Chunk.Size; z++)
            {
                int wx = originX + x;
                int wz = originZ + z;

                for (int y = 0; y < Height; y++)
                {
                    byte light = chunk.GetSkyLight(x, y, z);

                    if (light <= 1 || IsOpaque(chunk.GetBlock(x, y, z)))
                        continue;

                    bool needsPropagation = false;

                    foreach (var n in Neighbors)
                    {
                        int nx = wx + n.X;
                        int ny = y + n.Y;
                        int nz = wz + n.Z;

                        if (ny < 0 || ny >= Height)
                            continue;

                        if (!TryGetSkyLight(world, nx, ny, nz, out byte neighborLight))
                            continue;

                        if (IsOpaque(world.GetBlock(nx, ny, nz)))
                            continue;

                        if (neighborLight < light)
                        {
                            needsPropagation = true;
                            break;
                        }
                    }

                    if (needsPropagation)
                        Enqueue(wx, y, wz);
                }
            }
        }

        var dirtySections = new HashSet<(int X, int Y, int Z)>();

        ProcessRelighting(world, queue, queued, dirtySections);
        FlushDirtySections(dirtySections);

        return;

        void Enqueue(int x, int y, int z)
        {
            var position = (x, y, z);

            if (queued.Add(position))
                queue.Enqueue(position);
        }
    }

    public static void OnBlockChanged(World world, int x, int y, int z, uint oldId, uint newId)
    {
        if (y < 0 || y >= Height)
            return;

        bool wasOpaque = IsOpaque(oldId);
        bool isOpaque = IsOpaque(newId);

        if (wasOpaque == isOpaque)
            return;

        var dirtySections = new HashSet<(int X, int Y, int Z)>();
        Queue<(int X, int Y, int Z, byte Light)> removal = new();
        Queue<(int X, int Y, int Z)> relight = new();
        HashSet<(int X, int Y, int Z)> queued = [];

        byte oldBlockLight = GetSkyLight(world, x, y, z);
        bool directSkyAbove = HasDirectSkyAbove(world, x, y, z);

        if (isOpaque)
        {
            // The new opaque block can still receive direct sky light
            // on its exposed position, but cannot transmit it.
            byte newBlockLight = directSkyAbove ? MaxLight : (byte)0;

            SetSkyLight(world, x, y, z, newBlockLight, dirtySections);

            if (oldBlockLight > 0)
                removal.Enqueue((x, y, z, oldBlockLight));

            // Everything below this block loses direct skylight.
            for (int cy = y - 1; cy >= 0; cy--)
            {
                byte oldLight = GetSkyLight(world, x, cy, z);

                if (oldLight == 0)
                    continue;

                SetSkyLight(world, x, cy, z, 0, dirtySections);
                removal.Enqueue((x, cy, z, oldLight));
            }
        }
        else
        {
            // The block became transparent.
            // Restore direct sky down the column when exposed.
            if (directSkyAbove)
            {
                for (int cy = y; cy >= 0; cy--)
                {
                    var block = world.GetBlock(x, cy, z);

                    SetSkyLight(
                        world, x, cy, z,
                        MaxLight,
                        dirtySections);

                    EnqueueRelight(x, cy, z);

                    if (IsOpaque(block))
                        break;
                }
            }
            else
            {
                // No direct sky reaches this position.
                // Let surviving neighboring sources illuminate it.
                foreach (var n in Neighbors)
                {
                    int nx = x + n.X;
                    int ny = y + n.Y;
                    int nz = z + n.Z;

                    if (ny < 0 || ny >= Height)
                        continue;

                    if (GetSkyLight(world, nx, ny, nz) > 0)
                        EnqueueRelight(nx, ny, nz);
                }

                EnqueueRelight(x, y, z);
            }
        }

        // Remove light that depended on the old propagation path.
        while (removal.TryDequeue(out var removed))
        {
            foreach (var n in Neighbors)
            {
                int nx = removed.X + n.X;
                int ny = removed.Y + n.Y;
                int nz = removed.Z + n.Z;

                if (ny < 0 || ny >= Height)
                    continue;

                byte neighborLight = GetSkyLight(world, nx, ny, nz);

                if (neighborLight == 0)
                    continue;

                if (neighborLight < removed.Light)
                {
                    SetSkyLight(world, nx, ny, nz, 0, dirtySections);
                    removal.Enqueue((nx, ny, nz, neighborLight));
                }
                else
                {
                    // This light may have another valid source.
                    EnqueueRelight(nx, ny, nz);
                }
            }
        }

        // Restore light from remaining direct-sky and propagated sources.
        ProcessRelighting(world, relight, queued, dirtySections);
        FlushDirtySections(dirtySections);

        return;

        void EnqueueRelight(int px, int py, int pz)
        {
            var position = (px, py, pz);

            if (queued.Add(position))
                relight.Enqueue(position);
        }
    }

    private static void ProcessRelighting(World world, Queue<(int X, int Y, int Z)> queue, HashSet<(int X, int Y, int Z)> queued, HashSet<(int X, int Y, int Z)> dirtySections)
    {
        while (queue.TryDequeue(out var position))
        {
            queued.Remove(position);

            byte light = GetSkyLight(world, position.X, position.Y, position.Z);

            if (light <= 1 || IsOpaque(world.GetBlock(position.X, position.Y, position.Z)))
                continue;

            foreach (var n in Neighbors)
            {
                int nx = position.X + n.X;
                int ny = position.Y + n.Y;
                int nz = position.Z + n.Z;

                if (ny < 0 || ny >= Height)
                    continue;

                if (IsOpaque(world.GetBlock(nx, ny, nz)))
                    continue;

                byte candidate = (byte)(light - 1);
                byte existing = GetSkyLight(world, nx, ny, nz);

                if (candidate <= existing)
                    continue;

                if (!SetSkyLight(world, nx, ny, nz, candidate, dirtySections))
                    continue;

                var next = (nx, ny, nz);

                if (queued.Add(next))
                    queue.Enqueue(next);
            }
        }
    }

    private static bool RecalculateColumn(IChunk chunk, int x, int z)
    {
        bool blocked = false;
        bool changed = false;

        for (int y = Height - 1; y >= 0; y--)
        {
            var block = chunk.GetBlock(x, y, z);
            byte sky = blocked ? (byte)0 : MaxLight;
            byte blockLight = chunk.GetBlockLight(x, y, z);

            if (chunk.GetSkyLight(x, y, z) != sky)
            {
                chunk.SetLight(x, y, z, sky, blockLight);
                changed = true;
            }

            if (IsOpaque(block))
                blocked = true;
        }

        if (changed)
            chunk.IsRuntimeModified = true;

        return changed;
    }

    private static bool HasDirectSkyAbove(World world, int x, int y, int z)
    {
        for (int cy = Height - 1; cy > y; cy--)
        {
            if (IsOpaque(world.GetBlock(x, cy, z)))
                return false;
        }

        return true;
    }

    private static byte GetSkyLight(World world, int x, int y, int z)
    {
        return TryGetSkyLight(world, x, y, z, out byte light) ? light : (byte)0;
    }

    private static bool TryGetSkyLight(World world, int x, int y, int z, out byte light)
    {
        light = 0;

        if (y < 0 || y >= Height)
            return false;

        GetChunkCoordinates(x, z, out int cx, out int lx, out int cz, out int lz);

        var chunk = world.GetChunk(cx, cz);

        if (chunk is null)
            return false;

        light = chunk.GetSkyLight(lx, y, lz);
        return true;
    }

    private static bool SetSkyLight(World world, int x, int y, int z, byte sky, HashSet<(int X, int Y, int Z)> dirtySections)
    {
        if (y < 0 || y >= Height)
            return false;

        GetChunkCoordinates(x, z, out int cx, out int lx, out int cz, out int lz);

        var chunk = world.GetChunk(cx, cz);

        if (chunk is null)
            return false;

        byte oldSky = chunk.GetSkyLight(lx, y, lz);

        if (oldSky == sky)
            return false;

        chunk.SetLight(lx, y, lz, sky, chunk.GetBlockLight(lx, y, lz));
        chunk.IsRuntimeModified = true;

        dirtySections.Add((FloorDiv(x, SectionSize), y / SectionSize, FloorDiv(z, SectionSize)));

        return true;
    }

    private static void FlushDirtySections(HashSet<(int X, int Y, int Z)> dirtySections)
    {
        foreach (var section in dirtySections)
            WorldMeshManager.MarkBlockDirty(section.X * SectionSize, section.Y * SectionSize, section.Z * SectionSize);
    }

    private static bool IsOpaque(BlockPlacement block)
    {
        return block.Id != 0 && GameAPIs.BlockRegistry.Get(block.Id).Properties.Opaque;
    }

    private static bool IsOpaque(uint id)
    {
        return id != 0 && GameAPIs.BlockRegistry.Get(id).Properties.Opaque;
    }

    private static int FloorDiv(int value, int divisor)
    {
        int result = Math.DivRem(value, divisor, out int remainder);
        return remainder < 0 ? result - 1 : result;
    }

    private static void GetChunkCoordinates(int x, int z, out int chunkX, out int localX, out int chunkZ, out int localZ)
    {
        chunkX = Math.DivRem(x, Chunk.Size, out localX);
        chunkZ = Math.DivRem(z, Chunk.Size, out localZ);

        if (localX < 0)
        {
            chunkX--;
            localX += Chunk.Size;
        }

        if (localZ < 0)
        {
            chunkZ--;
            localZ += Chunk.Size;
        }
    }
}