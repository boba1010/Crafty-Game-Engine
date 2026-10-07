using Crafty.SDK.World;

namespace Crafty.ChunkGeneration.World;

public sealed class Chunk : IChunk
{
    public const int Size = 32;
    public int X { get; set; }
    public int Z { get; set; }
    public List<BlockPlacement> Blocks { get; set; } = [];
    public bool IsModified { get; set; }

    public BlockPlacement GetBlock(int x, int y, int z)
    {
        int index = x * (Size * 416) + z * 416 + y;

        try
        {
            return Blocks[index];
        }
        catch (Exception)
        {
            throw new Exception($"Blocks: {Blocks.Count} | Index: {index}");
        }
    }

    //public int GetHeight(int x, int z)
    //{
    //    throw new NotImplementedException();
    //}

    public void SetBlock(BlockPlacement block)
    {
        int index = block.X * (Size * 416) + block.Z * 416 + block.Y;
        Blocks[index] = block;
    }
}
