using Crafty.Auto.Files;
using Crafty.Auto.Json;
using Crafty.InstancesManager.Dtos;
using Crafty.InstancesManager.Mappers;
using Crafty.InstancesManager.Models;

namespace Crafty.InstancesManager;

public static class InstancesManager
{
    private static readonly AutoFileIO<InstancesData, InstancesDataDto, AutoJson<InstancesDataDto>> File = 
        new("instancesData.json", new AutoJson<InstancesDataDto>(), new InstancesDataMapper());

    public static InstancesData? Data { get; set; }

    public static async Task<bool> LoadInstancesDataAsync()
    {
        try
        {
            Data = await File.LoadAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static async Task<bool> SaveInstancesDataAsync()
    {
        try
        {
            await File.SaveAsync(Data!);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
