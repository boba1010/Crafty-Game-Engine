namespace Crafty.InstancesManager.Models;

public class GameInstanceSnapshot
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Directory { get; set; } = null!;
}
