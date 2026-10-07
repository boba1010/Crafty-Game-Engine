using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;

namespace Crafty.ChunkGeneration;

public class WorldGenService : IWorldGenService
{
    internal WorldGenerator WorldGenerator { get; set; }

    public WorldGenService(string worldDir)
    {
        WorldGenerator = new();
        WorldIOManager.WorldDir = worldDir;
    }

    public World.World GenerateWorld(string worldDir, string worldName, ulong seed, IProgress<WorldProgress> progress = null!)
    {
        return WorldGenerator.GenerateWorld(worldDir, worldName, seed, progress);
    }

    public Chunk LoadChunk(int x, int z, ulong seed)
    {
        string path = Path.Combine(WorldIOManager.WorldDir, "chunks", $"{x}_{z}.chunk");

        if (!File.Exists(path))
        {
            var chunk = WorldGenerator.GenerateChunk(x, z, seed);
            WorldIOManager.SaveChunk((Chunk)chunk);
        }

        return WorldIOManager.LoadChunk(x, z);
    }

    public World.World LoadWorld(string path) => WorldIOManager.LoadWorld(path);
}
