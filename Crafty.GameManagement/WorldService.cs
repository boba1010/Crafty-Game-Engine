using Crafty.GameManagement.Models;

namespace Crafty.GameManagement;

public class WorldService : IWorldService
{
    public async Task CreateWorldAsync(WorldSnapshot data)
    {
    }

    public async Task DeleteWorldAsync(string folderDir)
    {
    }

    public async Task<WorldSnapshot> GetWorldAsync(string folderDir)
    {
        throw new NotImplementedException();
    }

    public async Task<List<WorldSnapshot>> GetWorldsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task RenameWorldAsync(string folderDir, string newName)
    {
    }
}
