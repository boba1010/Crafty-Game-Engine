using Crafty.SDK.World;
using System.Collections.Concurrent;

namespace Crafty.ChunkGeneration.World;

public sealed class World
{
    public string Name { get; set; } = null!;
    public string Directory { get; set; } = null!;
    public ulong Seed { get; set; }
    public event Action<BlockChanged>? BlockChanged;

    public readonly ConcurrentDictionary<(int X, int Z), IChunk> chunks = [];

    public IChunk? GetChunk(int x, int z)
    {
        return chunks.GetValueOrDefault((x, z));
    }

    public void LoadChunk(IChunk chunk)
    {
        chunks[(chunk.X, chunk.Z)] = chunk;
    }

    public void UnloadChunk(int x, int z)
    {
        chunks.Remove((x, z), out _);
    }

    public BlockPlacement GetBlock(int x, int y, int z)
    {
        int chunkX = Math.DivRem(x, Chunk.Size, out int localX);
        int chunkZ = Math.DivRem(z, Chunk.Size, out int localZ);

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

        IChunk? chunk = GetChunk(chunkX, chunkZ);

        if (chunk is null)
            return default;

        return chunk.GetBlock(localX, y, localZ);
    }

    public void SetBlock(int x, int y, int z, uint id, byte state)
    {
        int chunkX = Math.DivRem(x, Chunk.Size, out int localX);
        int chunkZ = Math.DivRem(z, Chunk.Size, out int localZ);

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

        IChunk? chunk = GetChunk(chunkX, chunkZ);

        if (chunk is null)
            return;

        uint oldId = chunk.GetBlock(localX, y, localZ).Id;

        chunk.SetBlock((byte)localX, (short)y, (byte)localZ, new(id, state));

        chunk.IsModified = true;

        BlockChanged?.Invoke(new(x, y, z, oldId, id));
    }
}
