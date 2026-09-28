namespace Crafty.SDK;

/// <summary>
/// The interface required to build a mod
/// </summary>
public interface IMod
{
    /// <summary>
    /// The mod namespace
    /// </summary>
    public string Id { get; }
    /// <summary>
    /// The mod name
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The mod version
    /// </summary>
    public string Version { get; }
    /// <summary>
    /// The mod assets directory
    /// </summary>
    public string AssetsDirectory { get; }

    /// <summary>
    /// The entry point
    /// </summary>
    /// <param name="context"></param>
    public void Initilaize(IModContext context);
}
