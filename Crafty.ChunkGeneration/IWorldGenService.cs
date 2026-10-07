using Crafty.ChunkGeneration.World;

namespace Crafty.ChunkGeneration;

public record struct WorldProgress(string Status, double Progress);

public interface IWorldGenService
{
    public World.World GenerateWorld(string worldDir, string worldName, ulong seed, IProgress<WorldProgress> progress = null!);
    public World.World LoadWorld(string path);
    public Chunk LoadChunk(int x, int z, ulong seed);
}
