using Crafty.SDK;
using Crafty.SDK.Client;
using Crafty.SDK.Debugging;

namespace Crafty.Engine.Core;

public class ModContext : IModContext
{
    public ILogger Logger => new Logger();
    public IBlockRegistry Blocks => new BlockRegistry();
    public IItemRegistry Items => new ItemRegistry();
}
