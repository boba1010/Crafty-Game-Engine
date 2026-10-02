namespace Crafty.SDK.Client;

public sealed class Item
{
    public required string Name { get; init; }
    public string? BlockId { get; set; }
    public ushort MaxStackSize { get; init; } = 64;
}
