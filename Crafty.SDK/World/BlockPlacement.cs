namespace Crafty.SDK.World;

public readonly struct BlockPlacement(ushort id, byte x, ushort y, byte z)
{
    public ushort Id { get; } = id;
    public byte X { get; } = x;
    public ushort Y { get; } = y;
    public byte Z { get; } = z;
}
