namespace Crafty.SDK.World;

public interface IChunk
{
    public int X { get; }
    public int Z { get; }

    public bool IsModified { get; set; }
    public bool IsRuntimeModified { get; set; }

    public BlockPlacement GetBlock(int x, int y, int z);
    public void SetBlock(int x, int y, int z, BlockPlacement block);
    byte GetSkyLight(int x, int y, int z);
    byte GetBlockLight(int x, int y, int z);
    void SetLight(int x, int y, int z, byte sky, byte block);
    //int GetHeight(int x, int z);
    //void SetBiome(int x, int z, BiomePlacement biome);
}
