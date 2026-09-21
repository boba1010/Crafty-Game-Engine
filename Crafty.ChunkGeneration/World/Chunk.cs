using Crafty.SDK.World;

namespace Crafty.ChunkGeneration.World;

public sealed class Chunk : IChunk
{
    public const int Size = 32;
    public int X { get; set; }
    public int Z { get; set; }
    public List<BlockPlacement> Blocks { get; set; } = [];

    public BlockPlacement GetBlock(int x, int y, int z)
    {
        int index = x * 12800 + z * 400 + y;

        try
        {
            return Blocks[index];
        }
        catch (Exception)
        {
            throw new Exception($"Blocks: {Blocks.Count} | Index: {index}");
        }

        //return Blocks.FirstOrDefault(b => b.X == x && b.Y == y && b.Z == z);
    }

    public int GetHeight(int x, int z)
    {
        throw new NotImplementedException();
    }

    public void SetBiome(int x, int z, BiomePlacement biome)
    {
        throw new NotImplementedException();
    }

    public void SetBlock(BlockPlacement block)
    {
        Blocks.Add(block);
    }
}
