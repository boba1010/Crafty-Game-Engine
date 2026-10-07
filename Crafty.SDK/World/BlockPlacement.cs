namespace Crafty.SDK.World;

public readonly struct BlockPlacement(uint id, byte x, short y, byte z)
{
    public uint Id { get; } = id;
    public byte X { get; } = x;
    public short Y { get; } = y;
    public byte Z { get; } = z;
}
