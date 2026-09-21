using Crafty.ChunkGeneration.World;

namespace Crafty.ChunkGeneration;

public interface IWorldGenService
{
    public World.World GenerateWorld(ulong seed);
    public World.World LoadWorld(string path);
    public Chunk LoadChunk(int x, int z);
}
