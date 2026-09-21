namespace Crafty.InstancesManager.Dtos;

public class GameInstanceSnapshotDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Directory { get; set; } = null!;
}
