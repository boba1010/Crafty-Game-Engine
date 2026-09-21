using System.Text.Json;

namespace Crafty.Auto.Json;

public sealed class AutoJson<T> : ISerializer<T>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public async Task<T?> DeserializeAsync(Stream stream)
    {
        return await JsonSerializer.DeserializeAsync<T>(stream, Options);
    }

    public async Task SerializeAsync(Stream stream, T value)
    {
        await JsonSerializer.SerializeAsync(stream, value, Options);
    }
}