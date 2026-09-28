using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;

namespace Crafty.ChunkGeneration;

public class WorldGenService : IWorldGenService
{
    public WorldGenService(string worldPath)
    {
        WorldIOManager.WorldPath = worldPath;
    }

    public World.World GenerateWorld(ulong seed)
    {
        WorldGenerator worldGenerator = new(seed);
        return worldGenerator.GenerateWorld();
    }

    public Chunk LoadChunk(int x, int z)
    {
        return WorldIOManager.LoadChunk(x, z);
    }

    public World.World LoadWorld(string path) => WorldIOManager.LoadWorld(path);
}
