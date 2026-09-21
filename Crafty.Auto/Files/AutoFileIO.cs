using Crafty.Auto.Json;
using Crafty.Auto.Mappers;

namespace Crafty.Auto.Files;

public sealed class AutoFileIO<TData, TDto, TSerializer>(
    string path,
    TSerializer serializer,
    IMapper<TData, TDto> mapper)
    where TSerializer : ISerializer<TDto>
{
    public async Task<TData?> LoadAsync()
    {
        await using var stream = File.OpenRead(path);

        var dto = await serializer.DeserializeAsync(stream);

        return dto is null ? default : mapper.FromDto(dto);
    }

    public async Task SaveAsync(TData data)
    {
        var dto = mapper.ToDto(data);

        await using var stream = File.Create(path);

        await serializer.SerializeAsync(stream, dto);
    }
}
