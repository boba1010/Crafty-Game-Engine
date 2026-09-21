namespace Crafty.Auto.Json;

public interface ISerializer<T>
{
    Task<T?> DeserializeAsync(Stream stream);
    Task SerializeAsync(Stream stream, T value);
}
