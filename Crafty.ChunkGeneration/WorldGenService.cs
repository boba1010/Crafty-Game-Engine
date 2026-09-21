using Crafty.ChunkGeneration.IO;
using Crafty.ChunkGeneration.World;

namespace Crafty.ChunkGeneration;

public class WorldGenService(WorldIOManager worldIOManager) : IWorldGenService
{
    public World.World GenerateWorld(ulong seed)
    {
        WorldGenerator worldGenerator = new(seed);
        return worldGenerator.GenerateWorld();
    }

    public Chunk LoadChunk(int x, int z)
    {
        return worldIOManager.LoadChunk(x, z);
    }

    public World.World LoadWorld(string path) => worldIOManager.LoadWorld(path);
}
