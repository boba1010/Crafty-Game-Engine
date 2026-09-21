using Crafty.Auto.Files;
using Crafty.Auto.Json;
using Crafty.InstancesManager.Dtos;
using Crafty.InstancesManager.Mappers;
using Crafty.InstancesManager.Models;
using System.Text.Json;

namespace Crafty.InstancesManager;

public class InstanceService : IInstanceService
{
    private readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<bool> CreateInstanceAsync(GameInstance instance)
    {
        try
        {
            await InstancesManager.LoadInstancesDataAsync();

            var fullPath = Path.Combine(Path.GetFullPath(instance.Directory), instance.Name);

            Directory.CreateDirectory(fullPath);
            Directory.CreateDirectory(Path.Combine(fullPath, "Saves"));
            Directory.CreateDirectory(Path.Combine(fullPath, "Mods"));
            Directory.CreateDirectory(Path.Combine(fullPath, "Config"));
            Directory.CreateDirectory(Path.Combine(fullPath, "Settings"));
            Directory.CreateDirectory(Path.Combine(fullPath, "bin"));

            using var fs = File.Create($"{fullPath}\\{instance.Name}.json");

            JsonSerializer.Serialize(fs, instance, Options);

            InstancesManager.Data?.Snapshots.Add(new() { Directory = fullPath });

            await InstancesManager.SaveInstancesDataAsync();

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task DeleteInstanceAsync()
    {
    }

    public async Task DownloadAndInstallInstanceAsync()
    {
    }

    public async Task DownloadInstanceAsync()
    {
    }

    public async Task EditInstanceAsync()
    {
    }

    public async Task InstallInstanceAsync()
    {
    }

    public async Task ImportInstanceAsync()
    {

    }

    public async Task<GameInstance?> LoadInstanceAsync(Guid id)
    {
        await InstancesManager.LoadInstancesDataAsync();

        if (InstancesManager.Data?.Snapshots is not { })
            return null;

        GameInstance? instance = null;

        foreach (var snapshot in InstancesManager.Data.Snapshots)
        {
            var file = new AutoFileIO<GameInstance, GameInstanceDto, AutoJson<GameInstanceDto>>(
                Path.Combine(snapshot.Directory, $"{snapshot.Name}.json"),
                new AutoJson<GameInstanceDto>(),
                new GameInstanceMapper());

            var instance_ = await file.LoadAsync();
            if (instance_?.Id == id)
            {
                instance = instance_;
                break;
            }
        }

        return instance;
    }

    public async Task<List<GameInstance>> LoadInstancesAsync()
    {
        await InstancesManager.LoadInstancesDataAsync();

        if (InstancesManager.Data?.Snapshots is not { })
            return [];

        List<GameInstance> instances = [];

        foreach (var snapshot in InstancesManager.Data.Snapshots)
        {
            var file = new AutoFileIO<GameInstance, GameInstanceDto, AutoJson<GameInstanceDto>>(
                Path.Combine(snapshot.Directory, $"{snapshot.Name}.json"),
                new AutoJson<GameInstanceDto>(),
                new GameInstanceMapper());

            var instance = await file.LoadAsync();

            if (instance is not null)
                instances.Add(instance);
        }

        return instances;
    }
}
