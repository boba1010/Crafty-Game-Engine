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
        var chunk = new Chunk()
        {
            X = context.ChunkX,
            Z = context.ChunkZ,
        };

        for (int x = 0; x < Chunk.Size; x++)
        {
            for (int z = 0; z < Chunk.Size; z++)
            {
                int start = Chunk.GetIndex(x, context.MinY, z);

                Array.Fill(chunk.Blocks, new BlockPlacement(5000, 0), start, 61);

                Array.Fill(chunk.Blocks, new BlockPlacement(2, 0), start + 61, 3);

                Array.Fill(chunk.Blocks, new BlockPlacement(1, 0), start + 64, 1);
            }
        }

        return chunk;
    }
}
