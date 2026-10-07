using Crafty.ChunkGeneration.World;
using CraftyNative.ThreeD.World;

namespace Crafty.Engine.Helpers;

public static class ChunkHelper
{
    public static uint GetBlockIdByGlobalPosition(int x, int y, int z)
    {
        if ((uint)y >= 416)
            return 0;

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

        if (!ChunkCache.TryGet(chunkX, chunkZ, out var chunk))
            return 0;

        return chunk!.GetBlock(localX, y, localZ).Id;
    }
}