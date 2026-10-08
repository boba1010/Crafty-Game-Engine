using Crafty.SDK.Client;
using Crafty.SDK.World;

namespace Crafty.ChunkGeneration.World;

internal class ChunkGenerator : IChunkGenerator
{
    private readonly Dictionary<BlockCategory, uint> _blockIds = new()
    {
        [BlockCategory.Nature] = 0,
        [BlockCategory.Stone] = 5_000,
        [BlockCategory.Wood] = 15_000,
        [BlockCategory.Ore] = 20_000,
        [BlockCategory.Building] = 30_000,
        [BlockCategory.Decoration] = 40_000,
        [BlockCategory.Technical] = 50_000
    };

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
                    uint blockId = y switch
                    {
                        _ when y < (GroundHeight - 3) => _blockIds[BlockCategory.Stone] + 0,
                        _ when y < GroundHeight => _blockIds[BlockCategory.Nature] + 2,
                        GroundHeight => _blockIds[BlockCategory.Nature] + 1,
                        _ => _blockIds[BlockCategory.Nature] + 0
                    };

                    chunk.Blocks.Add(new(blockId, 0));
                }
            }
        }

        return chunk;
    }
}
