using Crafty.Auto.Mappers;
using Crafty.InstancesManager.Dtos;
using Crafty.InstancesManager.Models;

namespace Crafty.InstancesManager.Mappers;

public sealed class GameInstanceMapper : IMapper<GameInstance, GameInstanceDto>
{
    public GameInstance FromDto(GameInstanceDto dto)
    {
        return new()
        {
            Channel = dto.Channel,
            Directory = dto.Directory,
            Id = dto.Id,
            Name = dto.Name,
            Resolution = dto.Resolution,
            Version = dto.Version,
        };
    }

    public GameInstanceDto ToDto(GameInstance data)
    {
        return new()
        {
            Channel = data.Channel,
            Directory = data.Directory,
            Id = data.Id,
            Name = data.Name,
            Resolution = data.Resolution,
            Version = data.Version,
        };
    }
}
