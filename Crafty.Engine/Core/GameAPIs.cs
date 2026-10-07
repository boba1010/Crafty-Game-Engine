using Crafty.SDK.Client;
using CraftyNative.ThreeD;

namespace Crafty.Engine.Core;

public static class GameAPIs
{
    public static IBlockRegistry BlockRegistry { get; private set; } = null!;
    public static IItemRegistry ItemRegistry { get; private set; } = null!;

    public static void Initialize()
    {
        AssetRegistry.Scan("Assets");

        var mod = new MainMod.MainMod();
        BlockRegistry = new BlockRegistry();
        ItemRegistry = new ItemRegistry();
        var modContext = new ModContext(new Logger(), BlockRegistry, ItemRegistry);
        mod.Initilaize(modContext);
    }
}
