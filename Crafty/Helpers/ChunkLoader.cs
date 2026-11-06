using Crafty.ChunkGeneration;
using Crafty.ChunkGeneration.World;

namespace Crafty.Helpers;

public static class ChunkLoader
{
    public static IWorldGenService WorldGenService { get; set; } = null!;

    public static Chunk Load(int x, int z, ulong seed)
    {
        return WorldGenService.LoadChunk(x, z, seed);
    }
}
