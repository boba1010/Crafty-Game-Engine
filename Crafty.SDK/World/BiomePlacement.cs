namespace Crafty.SDK.World;

public readonly struct BiomePlacement(string biomeId, byte x, byte z)
{
    public string BiomeId { get; } = biomeId;
    public byte X { get; } = x;
    public byte Z { get; } = z;
}
