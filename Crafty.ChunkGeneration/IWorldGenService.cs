using Crafty.ChunkGeneration.World;

namespace Crafty.ChunkGeneration;

public interface IWorldGenService
{
    public World.World GenerateWorld();
    public World.World LoadWorld(string path);
    public Chunk LoadChunk(int x, int z);
    public byte[] LoadCompressedChunkBytes(int x, int z);
}
