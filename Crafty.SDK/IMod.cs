namespace Crafty.SDK;

public interface IMod
{
    public string Id { get; }
    public string Name { get; }
    public string Version { get; }
    public string AssetsDirectory { get; }

    public void Initilaize(IModContext context);
}
