using Crafty.InstancesManager.Enums;

namespace Crafty.InstancesManager.Models;

public sealed class GameInstance
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Version { get; set; } = null!;
    public string Directory { get; set; } = null!;
    public string Resolution { get; set; } = null!;
    public Channel Channel { get; set; }
}
