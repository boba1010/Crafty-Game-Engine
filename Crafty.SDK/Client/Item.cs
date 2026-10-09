namespace Crafty.SDK.Client;

public sealed class Item
{
    public string Name { get; init; } = null!;
    public string? BlockId { get; set; }
    public ushort MaxStackSize { get; init; } = 64;
    public ItemTexture? Texture { get; set; }
    public bool HiddenItem { get; set; }
}
