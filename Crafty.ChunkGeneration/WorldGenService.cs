using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;

namespace Crafty.ChunkGeneration;

public class WorldGenService : IWorldGenService
{
    internal WorldGenerator WorldGenerator { get; set; }

    public WorldGenService(string worldPath, ulong seed)
    {
        WorldGenerator = new(seed);
        WorldIOManager.WorldPath = worldPath;
    }

    public World.World GenerateWorld()
    {
        return WorldGenerator.GenerateWorld();
    }

    public Chunk LoadChunk(int x, int z)
    {
        return WorldIOManager.LoadChunk(x, z);
    }

    public byte[] LoadCompressedChunkBytes(int x, int z)
    {
        string path = Path.Combine(WorldIOManager.WorldPath, "chunks", $"{x}_{z}.chunk");

        if (!File.Exists(path))
        {
            var chunk = WorldGenerator.GenerateChunk(x, z);
            WorldIOManager.SaveChunk((Chunk)chunk);
        }

        return WorldIOManager.LoadCompressedChunkBytes(x, z);
    }

    public World.World LoadWorld(string path) => WorldIOManager.LoadWorld(path);
}
