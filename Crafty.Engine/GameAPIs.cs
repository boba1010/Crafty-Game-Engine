using Crafty.SDK.Client;
using CraftyNative.ThreeD;

namespace Crafty.Engine;

public static class GameAPIs
{
    public static IBlockRegistry BlockRegistry { get; private set; } = null!;

    public static void Initialize()
    {
        AssetRegistry.Scan("Assets");

        var mod = new MainMod.MainMod();
        mod.Initilaize(new ModContext());
        BlockRegistry = new BlockRegistry();
    }
}
