using Crafty.SDK;
using Crafty.SDK.Client;

namespace Crafty.Engine.MainMod;

public sealed class MainMod : IMod
{
    public string Id => "crafty";

    public string Name => "Crafty";

    public string Version => "27.1";

    public string AssetsDirectory => throw new NotImplementedException();

    public void Initilaize(IModContext context)
    {
        var air = BlocksCreator.CreateAirBlock();
        context.Blocks.Register(air);
        context.Logger.Log("crafty.air created successfully");

        var grassBlock = BlocksCreator.CreateThreeSidedTextureBlock("crafty.grass");
        context.Blocks.Register(grassBlock);
        context.Items.Register(new Item()
        { 
            Name = "Grass block",
            BlockId = "crafty.grass"
        });
        context.Logger.Log("crafty.grass created successfully");

        var dirtBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.dirt");
        context.Blocks.Register(dirtBlock);
        context.Items.Register(new Item()
        {
            Name = "Dirt",
            BlockId = "crafty.dirt"
        });
        context.Logger.Log("crafty.dirt created successfully");
        
        var stoneBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.stone");
        context.Blocks.Register(stoneBlock);
        context.Items.Register(new Item()
        { 
            Name = "Stone",
            BlockId = "crafty.stone"
        });
        context.Logger.Log("crafty.stone created successfully");
    }
}
