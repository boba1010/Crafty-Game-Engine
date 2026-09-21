using Crafty.Auto.Mappers;
using Crafty.InstancesManager.Dtos;
using Crafty.InstancesManager.Models;

namespace Crafty.InstancesManager.Mappers;

public class InstancesDataMapper : IMapper<InstancesData, InstancesDataDto>
{
    public InstancesData FromDto(InstancesDataDto dto)
    {
        return new()
        {
            Snapshots = 
            [.. 
                dto.Snapshots.Select(
                snapshot => new GameInstanceSnapshot() 
                { 
                    Id = snapshot.Id, 
                    Directory = snapshot.Directory,
                    Name = snapshot.Name
                })
            ]
        };
    }

    public InstancesDataDto ToDto(InstancesData data)
    {
        return new()
        {
            Snapshots =
            [..
                data.Snapshots.Select(
                snapshot => new GameInstanceSnapshotDto()
                {
                    Id = snapshot.Id,
                    Directory = snapshot.Directory,
                    Name = snapshot.Name
                })
            ]
        };
    }
}
