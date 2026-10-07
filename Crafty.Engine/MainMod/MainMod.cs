using Crafty.SDK;
using Crafty.SDK.Client;

namespace Crafty.Engine.MainMod;

public sealed class MainMod : IMod
{
    public string Id => "crafty";

    public string Name => "Crafty";

    public string Version => "26.10.8";

    public string AssetsDirectory => "..\\Assets";

    public void Initilaize(IModContext context)
    {
        Nature(context);
        Wood(context);

        StoneAndUndergroundBlocks(context);
        Ores(context);
    }

    private void Wood(IModContext context)
    {
        var oakPlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.oak_planks");
        oakPlanksBlock.Name = "Oak Planks";
        context.Blocks.Register(BlockCategory.Wood, oakPlanksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Oak Planks",
            BlockId = "crafty.oak_planks"
        });
        context.Logger.Log("crafty.oak_planks created successfully");

        var oakLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.oak_log");
        oakLogBlock.Name = "Oak Log";
        context.Blocks.Register(BlockCategory.Wood, oakLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
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
        context.Blocks.Register(BlockCategory.Nature, air);
        context.Logger.Log("crafty.air created successfully");

        var grassBlock = BlocksCreator.CreateThreeSidedTextureBlock("crafty.grass");
        grassBlock.Name = "Grass block";
        context.Blocks.Register(BlockCategory.Nature, grassBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Grass block",
            BlockId = "crafty.grass"
        });
        context.Logger.Log("crafty.grass created successfully");

        var dirtBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.dirt");
        dirtBlock.Name = "Dirt";
        context.Blocks.Register(BlockCategory.Nature, dirtBlock);
        context.Items.Register(ItemCategory.Block, new Item()
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
        context.Blocks.Register(BlockCategory.Stone, stoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Stone",
            BlockId = "crafty.stone"
        });
        context.Logger.Log("crafty.stone created successfully");

        var sandBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.sand");
        sandBlock.Name = "Sand";
        context.Blocks.Register(BlockCategory.Stone, sandBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Sand",
            BlockId = "crafty.sand"
        });
        context.Logger.Log("crafty.sand created successfully");

        var redSandBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.red_sand");
        redSandBlock.Name = "Red Sand";
        context.Blocks.Register(BlockCategory.Stone, redSandBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Red Sand",
            BlockId = "crafty.red_sand"
        });
        context.Logger.Log("crafty.red_sand created successfully");

        var cobblestoneBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cobblestone");
        cobblestoneBlock.Name = "Cobblestone";
        context.Blocks.Register(BlockCategory.Stone, cobblestoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cobblestone",
            BlockId = "crafty.cobblestone"
        });
        context.Logger.Log("crafty.cobblestone created successfully");

        var sandstoneBlock = BlocksCreator.CreateOneTwoSidedTextureBlock("crafty.sandstone");
        sandstoneBlock.Name = "Sandstone";
        context.Blocks.Register(BlockCategory.Stone, sandstoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Sandstone",
            BlockId = "crafty.sandstone"
        });
        context.Logger.Log("crafty.sandstone created successfully");

        var redSandstoneBlock = BlocksCreator.CreateOneTwoSidedTextureBlock("crafty.red_sandstone");
        redSandstoneBlock.Name = "Red Sandstone";
        context.Blocks.Register(BlockCategory.Stone, redSandstoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Red Sandstone",
            BlockId = "crafty.red_sandstone"
        });
        context.Logger.Log("crafty.red_sandstone created successfully");

        var gravelBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.gravel");
        gravelBlock.Name = "Gravel";
        context.Blocks.Register(BlockCategory.Stone, gravelBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Gravel",
            BlockId = "crafty.gravel"
        });
        context.Logger.Log("crafty.gravel created successfully");

        var deepslateBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.deepslate");
        deepslateBlock.Name = "Deepslate";
        context.Blocks.Register(BlockCategory.Stone, deepslateBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Deepslate",
            BlockId = "crafty.deepslate"
        });
        context.Logger.Log("crafty.deepslate created successfully");

        var cobbledDeepslateBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cobbled_deepslate");
        cobbledDeepslateBlock.Name = "Cobbled Deepslate";
        context.Blocks.Register(BlockCategory.Stone, cobbledDeepslateBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cobbled Deepslate",
            BlockId = "crafty.cobbled_deepslate"
        });
        context.Logger.Log("crafty.cobbled_deepslate created successfully");

        var graniteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.granite");
        graniteBlock.Name = "Granite";
        context.Blocks.Register(BlockCategory.Stone, graniteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Granite",
            BlockId = "crafty.granite"
        });
        context.Logger.Log("crafty.granite created successfully");

        var dioriteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.diorite");
        dioriteBlock.Name = "Diorite";
        context.Blocks.Register(BlockCategory.Stone, dioriteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Diorite",
            BlockId = "crafty.diorite"
        });
        context.Logger.Log("crafty.diorite created successfully");

        var andesiteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.andesite");
        andesiteBlock.Name = "Andesite";
        context.Blocks.Register(BlockCategory.Stone, andesiteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Andesite",
            BlockId = "crafty.andesite"
        });
        context.Logger.Log("crafty.andesite created successfully");

        var basaltBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.basalt");
        basaltBlock.Name = "Basalt";
        context.Blocks.Register(BlockCategory.Stone, basaltBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Basalt",
            BlockId = "crafty.basalt"
        });
        context.Logger.Log("crafty.basalt created successfully");

        var calciteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.calcite");
        calciteBlock.Name = "Calcite";
        context.Blocks.Register(BlockCategory.Stone, calciteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Calcite",
            BlockId = "crafty.calcite"
        });
        context.Logger.Log("crafty.calcite created successfully");

        var tuffBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.tuff");
        tuffBlock.Name = "Tuff";
        context.Blocks.Register(BlockCategory.Stone, tuffBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Tuff",
            BlockId = "crafty.tuff"
        });
        context.Logger.Log("crafty.tuff created successfully");

        var coreShellBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.core_shell");
        coreShellBlock.Name = "Core Shell";
        context.Blocks.Register(BlockCategory.Stone, coreShellBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Core Shell",
            BlockId = "crafty.core_shell"
        });
        context.Logger.Log("crafty.core_shell created successfully");
    }

    private void Ores(IModContext context)
    {
        var coalOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.coal_ore");
        coalOreBlock.Name = "Coal Ore";
        context.Blocks.Register(BlockCategory.Ore, coalOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Coal Ore",
            BlockId = "crafty.coal_ore"
        });
        context.Logger.Log("crafty.coal_ore created successfully");

        var ironOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.iron_ore");
        ironOreBlock.Name = "Iron Ore";
        context.Blocks.Register(BlockCategory.Ore, ironOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Iron Ore",
            BlockId = "crafty.iron_ore"
        });
        context.Logger.Log("crafty.iron_ore created successfully");

        var goldOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.gold_ore");
        goldOreBlock.Name = "Gold Ore";
        context.Blocks.Register(BlockCategory.Ore, goldOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Gold Ore",
            BlockId = "crafty.gold_ore"
        });
        context.Logger.Log("crafty.gold_ore created successfully");

        var copperOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.copper_ore");
        copperOreBlock.Name = "Copper Ore";
        context.Blocks.Register(BlockCategory.Ore, copperOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Copper Ore",
            BlockId = "crafty.copper_ore"
        });
        context.Logger.Log("crafty.copper_ore created successfully");

        var diamondOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.diamond_ore");
        diamondOreBlock.Name = "Diamond Ore";
        context.Blocks.Register(BlockCategory.Ore, diamondOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Diamond Ore",
            BlockId = "crafty.diamond_ore"
        });
        context.Logger.Log("crafty.diamond_ore created successfully");

        var emeraldOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.emerald_ore");
        emeraldOreBlock.Name = "Emerald Ore";
        context.Blocks.Register(BlockCategory.Ore, emeraldOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Emerald Ore",
            BlockId = "crafty.emerald_ore"
        });
        context.Logger.Log("crafty.emerald_ore created successfully");

        var redDustOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.red_dust_ore");
        redDustOreBlock.Name = "Red Dust Ore";
        context.Blocks.Register(BlockCategory.Ore, redDustOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Red Dust Ore",
            BlockId = "crafty.red_dust_ore"
        });
        context.Logger.Log("crafty.red_dust_ore created successfully");
    }
}
