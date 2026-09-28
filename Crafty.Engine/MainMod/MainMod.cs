using Crafty.SDK;

namespace Crafty.Engine.MainMod;

public sealed class MainMod : IMod
{
    public string Id => "crafty";

    public string Name => "Crafty";

    public string Version => "27.1";

    public string AssetsDirectory => throw new NotImplementedException();

    public void Initilaize(IModContext context)
    {
        var blocksCreator = new BlocksCreator();

        var air = blocksCreator.CreateOneSidedTextureBlock("crafty.air");
        context.Blocks.Register(air);
        context.Logger.Log("crafty.air created successfully");

        var grassBlock = blocksCreator.CreateThreeSidedTextureBlock("crafty.grass");
        context.Blocks.Register(grassBlock);
        context.Logger.Log("crafty.grass created successfully");

        var dirtBlock = blocksCreator.CreateOneSidedTextureBlock("crafty.dirt");
        context.Blocks.Register(dirtBlock);
        context.Logger.Log("crafty.dirt created successfully");
        
        var stoneBlock = blocksCreator.CreateOneSidedTextureBlock("crafty.stone");
        context.Blocks.Register(stoneBlock);
        context.Logger.Log("crafty.stone created successfully");
    }
}
