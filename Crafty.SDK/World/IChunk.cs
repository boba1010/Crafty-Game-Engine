namespace Crafty.SDK.World;

public interface IChunk
{
    public int X { get; }
    public int Z { get; }

    public bool IsModified { get; set; }

    public BlockPlacement GetBlock(int x, int y, int z);
    public void SetBlock(int x, int y, int z, BlockPlacement block);
    //int GetHeight(int x, int z);
    //void SetBiome(int x, int z, BiomePlacement biome);
}
