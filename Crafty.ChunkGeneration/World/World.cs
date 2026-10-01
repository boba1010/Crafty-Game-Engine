using Crafty.SDK.World;
using System.Collections.Concurrent;

namespace Crafty.ChunkGeneration.World;

public sealed class World
{
    public string Name { get; set; } = null!;
    public string Directory { get; set; } = null!;
    public ulong Seed { get; set; }
    public event Action<BlockChanged>? BlockChanged;

    private readonly ConcurrentDictionary<(int X, int Z), IChunk> _chunks = [];

    public IChunk? GetChunk(int x, int z)
    {
        return _chunks.GetValueOrDefault((x, z));
    }

    public void LoadChunk(IChunk chunk)
    {
        _chunks[(chunk.X, chunk.Z)] = chunk;
    }

    public void UnloadChunk(int x, int z)
    {
        _chunks.Remove((x, z), out _);
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

    public void SetBlock(int x, int y, int z, ushort id)
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

        ushort oldId = chunk.GetBlock(localX, y, localZ).Id;

        chunk.SetBlock(new(id, (byte)localX, (short)y, (byte)localZ));

        BlockChanged?.Invoke(new(x, y, z, oldId, id));
    }
}
