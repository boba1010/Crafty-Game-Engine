namespace Crafty.SDK.World;

public readonly struct BlockPlacement(uint id, byte state)
{
    public uint Id { get; } = id;
    public byte State { get; } = state;
}
