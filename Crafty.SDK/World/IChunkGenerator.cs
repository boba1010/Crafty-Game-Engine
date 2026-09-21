namespace Crafty.SDK.World;

public interface IChunkGenerator
{
    public IChunk Generate(in ChunkGenerationContext context);
}
