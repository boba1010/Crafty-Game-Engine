using Crafty.InstancesManager.Models;

namespace Crafty.InstancesManager;

public interface IInstanceService
{
    public Task<bool> CreateInstanceAsync(GameInstance instance);
    public Task DownloadInstanceAsync();
    public Task InstallInstanceAsync();
    public Task DownloadAndInstallInstanceAsync();
    public Task DeleteInstanceAsync();
    public Task EditInstanceAsync();
    public Task<GameInstance?> LoadInstanceAsync(Guid id);
    public Task<List<GameInstance>> LoadInstancesAsync();
    public Task ImportInstanceAsync();
}
