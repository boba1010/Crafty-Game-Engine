namespace Crafty.SDK.Client.Blocks;

public sealed class BlockStateDefinition
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Properties { get; init; } = new Dictionary<string, IReadOnlyList<string>>();
}
