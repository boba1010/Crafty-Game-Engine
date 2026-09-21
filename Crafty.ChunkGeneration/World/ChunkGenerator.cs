using Crafty.SDK.World;

namespace Crafty.ChunkGeneration.World;

internal class ChunkGenerator : IChunkGenerator
{
    public IChunk Generate(in ChunkGenerationContext context)
    {
        const int GroundHeight = 64;

        var chunk = new Chunk()
        {
            X = context.ChunkX,
            Z = context.ChunkZ,
        };

        for (int x = 0; x < Chunk.Size; x++)
        {
            for (int z = 0; z < Chunk.Size; z++)
            {
                for (int y = context.MinY; y < context.MaxY; y++)
                {
                    ushort blockId = y switch
                    {
                        GroundHeight => 1,
                        _ when y < GroundHeight => 2,
                        _ when y < (GroundHeight - 3) => 3,
                        _ => 0
                    };

                    chunk.SetBlock(new(blockId, (byte)x, (ushort)y, (byte)z));
                }
            }
        }

        return chunk;
    }
}
