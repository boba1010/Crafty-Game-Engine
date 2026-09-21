namespace Crafty.SDK.Client.Blocks;

public sealed class BlockProperties
{
    public bool Solid { get; init; } = true;
    public bool Opaque { get; init; } = true;
    public bool Transparent { get; init; }
    public float Hardness { get; init; } = 1f;
    public float BlockResistance { get; init; } = 1f;
    public bool RequireTool { get; init; }
    public byte LightLevel { get; init; }
    public bool Replacable { get; init; }
    public bool Flammable { get; init; }
}
