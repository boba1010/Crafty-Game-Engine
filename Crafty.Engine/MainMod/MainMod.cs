using Crafty.SDK;
using Crafty.SDK.Client;

namespace Crafty.Engine.MainMod;

public sealed class MainMod : IMod
{
    public string Id => "crafty";

    public string Name => "Crafty";

    public string Version => "27.1";

    public string AssetsDirectory => "..\\Assets";

    public void Initilaize(IModContext context)
    {
        Nature(context);

        StoneAndUndergroundBlocks(context);

        Wood(context);   
    }

    private void Wood(IModContext context)
    {
        var oakPlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.oak_planks");
        oakPlanksBlock.Name = "Oak Planks";
        context.Blocks.Register(oakPlanksBlock);
        context.Items.Register(new Item()
        {
            Name = "Oak Planks",
            BlockId = "crafty.oak_planks"
        });
        context.Logger.Log("crafty.oak_planks created successfully");

        var oakLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.oak_log");
        oakLogBlock.Name = "Oak Log";
        context.Blocks.Register(oakLogBlock);
        context.Items.Register(new Item()
        {
            Name = "Oak Log",
            BlockId = "crafty.oak_log"
        });
        context.Logger.Log("crafty.oak_log created successfully");
    }

    private void Nature(IModContext context)
    {
        var air = BlocksCreator.CreateAirBlock();
        air.Name = "Air";
        context.Blocks.Register(air);
        context.Logger.Log("crafty.air created successfully");

        var grassBlock = BlocksCreator.CreateThreeSidedTextureBlock("crafty.grass");
        grassBlock.Name = "Grass block";
        context.Blocks.Register(grassBlock);
        context.Items.Register(new Item()
        {
            Name = "Grass block",
            BlockId = "crafty.grass"
        });
        context.Logger.Log("crafty.grass created successfully");

        var dirtBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.dirt");
        dirtBlock.Name = "Dirt";
        context.Blocks.Register(dirtBlock);
        context.Items.Register(new Item()
        {
            Name = "Dirt",
            BlockId = "crafty.dirt"
        });
        context.Logger.Log("crafty.dirt created successfully");
    }

    private void StoneAndUndergroundBlocks(IModContext context)
    {
        var stoneBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.stone");
        stoneBlock.Name = "Stone";
        context.Blocks.Register(stoneBlock);
        context.Items.Register(new Item()
        {
            Name = "Stone",
            BlockId = "crafty.stone"
        });
        context.Logger.Log("crafty.stone created successfully");

        var sandBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.sand");
        sandBlock.Name = "Sand";
        context.Blocks.Register(sandBlock);
        context.Items.Register(new Item()
        {
            Name = "Sand",
            BlockId = "crafty.sand"
        });
        context.Logger.Log("crafty.sand created successfully");

        var redSandBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.red_sand");
        redSandBlock.Name = "Red Sand";
        context.Blocks.Register(redSandBlock);
        context.Items.Register(new Item()
        {
            Name = "Red Sand",
            BlockId = "crafty.red_sand"
        });
        context.Logger.Log("crafty.red_sand created successfully");

        var cobblestoneBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cobblestone");
        cobblestoneBlock.Name = "Cobblestone";
        context.Blocks.Register(cobblestoneBlock);
        context.Items.Register(new Item()
        {
            Name = "Cobblestone",
            BlockId = "crafty.cobblestone"
        });
        context.Logger.Log("crafty.cobblestone created successfully");

        var sandstoneBlock = BlocksCreator.CreateOneTwoSidedTextureBlock("crafty.sandstone");
        sandstoneBlock.Name = "Sandstone";
        context.Blocks.Register(sandstoneBlock);
        context.Items.Register(new Item()
        {
            Name = "Sandstone",
            BlockId = "crafty.sandstone"
        });
        context.Logger.Log("crafty.sandstone created successfully");

        var redSandstoneBlock = BlocksCreator.CreateOneTwoSidedTextureBlock("crafty.red_sandstone");
        redSandstoneBlock.Name = "Red Sandstone";
        context.Blocks.Register(redSandstoneBlock);
        context.Items.Register(new Item()
        {
            Name = "Red Sandstone",
            BlockId = "crafty.red_sandstone"
        });
        context.Logger.Log("crafty.red_sandstone created successfully");

        var gravelBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.gravel");
        gravelBlock.Name = "Gravel";
        context.Blocks.Register(gravelBlock);
        context.Items.Register(new Item()
        {
            Name = "Gravel",
            BlockId = "crafty.gravel"
        });
        context.Logger.Log("crafty.gravel created successfully");

        var deepslateBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.deepslate");
        deepslateBlock.Name = "Deepslate";
        context.Blocks.Register(deepslateBlock);
        context.Items.Register(new Item()
        {
            Name = "Deepslate",
            BlockId = "crafty.deepslate"
        });
        context.Logger.Log("crafty.deepslate created successfully");

        var cobbledDeepslateBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cobbled_deepslate");
        cobbledDeepslateBlock.Name = "Cobbled Deepslate";
        context.Blocks.Register(cobbledDeepslateBlock);
        context.Items.Register(new Item()
        {
            Name = "Cobbled Deepslate",
            BlockId = "crafty.cobbled_deepslate"
        });
        context.Logger.Log("crafty.cobbled_deepslate created successfully");

        var graniteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.granite");
        graniteBlock.Name = "Granite";
        context.Blocks.Register(graniteBlock);
        context.Items.Register(new Item()
        {
            Name = "Granite",
            BlockId = "crafty.granite"
        });
        context.Logger.Log("crafty.granite created successfully");

        var dioriteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.diorite");
        dioriteBlock.Name = "Diorite";
        context.Blocks.Register(dioriteBlock);
        context.Items.Register(new Item()
        {
            Name = "Diorite",
            BlockId = "crafty.diorite"
        });
        context.Logger.Log("crafty.diorite created successfully");

        var andesiteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.andesite");
        andesiteBlock.Name = "Andesite";
        context.Blocks.Register(andesiteBlock);
        context.Items.Register(new Item()
        {
            Name = "Andesite",
            BlockId = "crafty.andesite"
        });
        context.Logger.Log("crafty.andesite created successfully");
    }
}
