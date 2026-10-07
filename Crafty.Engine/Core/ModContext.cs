using Crafty.SDK;
using Crafty.SDK.Client;
using Crafty.SDK.Debugging;

namespace Crafty.Engine.Core;

public class ModContext(ILogger logger, IBlockRegistry blockRegistry, IItemRegistry itemRegistry) : IModContext
{
    public ILogger Logger => logger;
    public IBlockRegistry Blocks => blockRegistry;
    public IItemRegistry Items => itemRegistry;
}
