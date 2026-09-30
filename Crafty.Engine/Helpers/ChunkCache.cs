using Crafty.ChunkGeneration.World;
using System.Collections.Concurrent;

namespace CraftyNative.ThreeD.World;

public static class ChunkCache
{
    private static readonly ConcurrentDictionary<(int x, int z), Chunk> _chunks = [];

    public static int Count => _chunks.Count;

    public static void Set(int x, int z, Chunk chunk)
    {
        _chunks[(x, z)] = chunk;
    }

    public static bool TryGet(int x, int z, out Chunk? chunk)
    {
        return _chunks.TryGetValue((x, z), out chunk);
    }

    public static bool Remove(int x, int z)
    {
        return _chunks.Remove((x, z), out _);
    }

    public static void Clear()
    {
        _chunks.Clear();
    }
}
