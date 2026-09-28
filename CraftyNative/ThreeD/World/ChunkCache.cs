using System.Collections.Concurrent;

namespace CraftyNative.ThreeD.World;

public static class ChunkCache
{
    private static readonly ConcurrentDictionary<(int x, int z), byte[]> _chunks = [];

    public static int Count => _chunks.Count;

    public static void Set(int x, int z, byte[] data)
    {
        _chunks[(x, z)] = data;
    }

    public static bool TryGet(int x, int z, out byte[]? data)
    {
        return _chunks.TryGetValue((x, z), out data);
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
