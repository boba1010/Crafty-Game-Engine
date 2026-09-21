namespace Crafty.SDK.Client.Blocks;

public sealed class BlockState
{
    public Block Block { get; init; } = null!;
    public IReadOnlyDictionary<string, string> Values { get; init; } = new Dictionary<string, string>();

    public string? Get(string property) => Values.TryGetValue(property, out var value) ? value : null;
}
