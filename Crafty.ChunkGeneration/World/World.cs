namespace Crafty.ChunkGeneration.World;

public sealed class World
{
    public string Name { get; set; } = null!;
    public string Directory { get; set; } = null!;
    public ulong Seed { get; set; }
}
