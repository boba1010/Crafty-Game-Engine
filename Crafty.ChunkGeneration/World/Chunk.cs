using Crafty.SDK.World;

namespace Crafty.ChunkGeneration.World;

public sealed class Chunk : IChunk
{
    public const int Size = 32;
    public const int Height = 416;
    public int X { get; set; }
    public int Z { get; set; }
    public List<BlockPlacement> Blocks { get; set; } = [];
    public const int Volume = Size * Size * Height;
    public byte[] Light { get; } = new byte[Volume];
    public bool IsModified { get; set; }
    public bool IsRuntimeModified { get; set; }

    public static int GetIndex(int x, int y, int z)
    {
        return x * (Size * Height) + z * Height + y;
    }

    public BlockPlacement GetBlock(int x, int y, int z)
    {
        int index = GetIndex(x, y, z);

        try
        {
            return Blocks[index];
        }
        catch (Exception)
        {
            throw new Exception($"Blocks: {Blocks.Count} | Index: {index}");
        }
    }

    public void SetBlock(int x, int y, int z, BlockPlacement block)
    {
        int index = GetIndex(x, y, z);
        Blocks[index] = block;
    }

    public byte GetSkyLight(int x, int y, int z)
    {
        return (byte)(Light[GetIndex(x, y, z)] >> 4);
    }

    public byte GetBlockLight(int x, int y, int z)
    {
        return (byte)(Light[GetIndex(x, y, z)] & 0x0F);
    }

    public void SetLight(int x, int y, int z, byte sky, byte block)
    {
        Light[GetIndex(x, y, z)] = (byte)((Math.Min(sky, (byte)15) << 4) | Math.Min(block, (byte)15));
    }
}
