using Crafty.SDK.Client;
using Crafty.SDK.Debugging;

namespace Crafty.SDK;

public interface IModContext
{
    public ILogger Logger { get; }
    public IBlockRegistry Blocks { get; }
}
