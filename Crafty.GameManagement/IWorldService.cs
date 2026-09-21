using Crafty.GameManagement.Models;

namespace Crafty.GameManagement;

public interface IWorldService
{
    public Task<WorldSnapshot> GetWorldAsync(string folderDir);
    public Task<List<WorldSnapshot>> GetWorldsAsync();

    public Task CreateWorldAsync(WorldSnapshot data);
    public Task DeleteWorldAsync(string folderDir);
    public Task RenameWorldAsync(string folderDir,  string newName);
}
