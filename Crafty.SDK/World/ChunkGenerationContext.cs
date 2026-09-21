namespace Crafty.SDK.World;

public readonly struct ChunkGenerationContext(ulong seed, int chunkX, int chunkZ, int minY, int maxY)
{

    public ulong Seed { get; } = seed;
    public int ChunkX { get; } = chunkX;
    public int ChunkZ { get; } = chunkZ;
    public int MinY { get; } = minY;
    public int MaxY { get; } = maxY;
}
