using Crafty.SDK;
using Crafty.SDK.Client;
using Crafty.SDK.Debugging;

namespace Crafty.Engine;

public class ModContext : IModContext
{
    public ILogger Logger => new Logger();
    public IBlockRegistry Blocks => new BlockRegistry();
}
