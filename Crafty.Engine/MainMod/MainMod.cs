using Crafty.SDK;
using Crafty.SDK.Client;
using Silk.NET.Core.Win32Extras;

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

        BuildingBlocks(context);
    }

    private void Wood(IModContext context)
    {
        var oakPlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.oak_planks", "Oak Planks");
        context.Blocks.Register(BlockCategory.Wood, oakPlanksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Oak Planks",
            BlockId = "crafty.oak_planks"
        });
        context.Logger.Log("crafty.oak_planks created successfully");

        var oakLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.oak_log", "Oak Log");
        context.Blocks.Register(BlockCategory.Wood, oakLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Oak Log",
            BlockId = "crafty.oak_log"
        });
        context.Logger.Log("crafty.oak_log created successfully");

        var strippedOakLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.oak_stripped_log", "Oak Stripped Log");
        context.Blocks.Register(BlockCategory.Wood, strippedOakLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Oak Stripped Log",
            BlockId = "crafty.oak_stripped_log"
        });
        context.Logger.Log("crafty.oak_stripped_log created successfully");

        var sprucePlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.spruce_planks", "Spruce Planks");
        context.Blocks.Register(BlockCategory.Wood, sprucePlanksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Spruce Planks",
            BlockId = "crafty.spruce_planks"
        });
        context.Logger.Log("crafty.spruce_planks created successfully");

        var spruceLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.spruce_log", "Spruce Log");
        context.Blocks.Register(BlockCategory.Wood, spruceLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Spruce Log",
            BlockId = "crafty.spruce_log"
        });
        context.Logger.Log("crafty.spruce_log created successfully");

        var strippedSpruceLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.spruce_stripped_log", "Spruce Stripped Log");
        context.Blocks.Register(BlockCategory.Wood, strippedSpruceLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Spruce Stripped Log",
            BlockId = "crafty.spruce_stripped_log"
        });
        context.Logger.Log("crafty.spruce_stripped_log created successfully");

        var darkOakPlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.dark_oak_planks", "Dark Oak Planks");
        context.Blocks.Register(BlockCategory.Wood, darkOakPlanksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Dark Oak Planks",
            BlockId = "crafty.dark_oak_planks"
        });
        context.Logger.Log("crafty.dark_oak_planks created successfully");

        var darkOakLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.dark_oak_log", "Dark Oak Log");
        context.Blocks.Register(BlockCategory.Wood, darkOakLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Dark Oak Log",
            BlockId = "crafty.dark_oak_log"
        });
        context.Logger.Log("crafty.dark_oak_log created successfully");

        var strippedDarkOakLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.dark_oak_stripped_log", "Dark Oak Stripped Log");
        context.Blocks.Register(BlockCategory.Wood, strippedDarkOakLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Dark Oak Stripped Log",
            BlockId = "crafty.dark_oak_stripped_log"
        });
        context.Logger.Log("crafty.dark_oak_stripped_log created successfully");

        var birchPlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.birch_planks", "Birch Planks");
        context.Blocks.Register(BlockCategory.Wood, birchPlanksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Birch Planks",
            BlockId = "crafty.birch_planks"
        });
        context.Logger.Log("crafty.birch_planks created successfully");

        var birchLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.birch_log", "Birch Log");
        context.Blocks.Register(BlockCategory.Wood, birchLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Birch Log",
            BlockId = "crafty.birch_log"
        });
        context.Logger.Log("crafty.birch_log created successfully");

        var strippedBirchLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.birch_stripped_log", "Birch Stripped Log");
        context.Blocks.Register(BlockCategory.Wood, strippedBirchLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Birch Stripped Log",
            BlockId = "crafty.birch_stripped_log"
        });
        context.Logger.Log("crafty.birch_stripped_log created successfully");

        var acaciaPlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.acacia_planks", "Acacia Planks");
        context.Blocks.Register(BlockCategory.Wood, acaciaPlanksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Acacia Planks",
            BlockId = "crafty.acacia_planks"
        });
        context.Logger.Log("crafty.acacia_planks created successfully");

        var acaciaLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.acacia_log", "Acacia Log");
        context.Blocks.Register(BlockCategory.Wood, acaciaLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Acacia Log",
            BlockId = "crafty.acacia_log"
        });
        context.Logger.Log("crafty.acacia_log created successfully");

        var strippedAcaciaLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.acacia_stripped_log", "Acacia Stripped Log");
        context.Blocks.Register(BlockCategory.Wood, strippedAcaciaLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Acacia Stripped Log",
            BlockId = "crafty.acacia_stripped_log"
        });
        context.Logger.Log("crafty.acacia_stripped_log created successfully");

        var cherryPlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cherry_planks", "Cherry Planks");
        context.Blocks.Register(BlockCategory.Wood, cherryPlanksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cherry Planks",
            BlockId = "crafty.cherry_planks"
        });
        context.Logger.Log("crafty.cherry_planks created successfully");

        var cherryLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.cherry_log", "Cherry Log");
        context.Blocks.Register(BlockCategory.Wood, cherryLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cherry Log",
            BlockId = "crafty.cherry_log"
        });
        context.Logger.Log("crafty.cherry_log created successfully");

        var strippedCherryLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.cherry_stripped_log", "Cherry Stripped Log");
        context.Blocks.Register(BlockCategory.Wood, strippedCherryLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cherry Stripped Log",
            BlockId = "crafty.cherry_stripped_log"
        });
        context.Logger.Log("crafty.cherry_stripped_log created successfully");

        var mangrovePlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.mangrove_planks", "Mangrove Planks");
        context.Blocks.Register(BlockCategory.Wood, mangrovePlanksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Mangrove Planks",
            BlockId = "crafty.mangrove_planks"
        });
        context.Logger.Log("crafty.mangrove_planks created successfully");

        var mangroveLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.mangrove_log", "Mangrove Log");
        context.Blocks.Register(BlockCategory.Wood, mangroveLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Mangrove Log",
            BlockId = "crafty.mangrove_log"
        });
        context.Logger.Log("crafty.mangrove_log created successfully");

        var strippedMangroveLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.mangrove_stripped_log", "Mangrove Stripped Log");
        context.Blocks.Register(BlockCategory.Wood, strippedMangroveLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Mangrove Stripped Log",
            BlockId = "crafty.mangrove_stripped_log"
        });
        context.Logger.Log("crafty.mangrove_stripped_log created successfully");

        var junglePlanksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.jungle_planks", "Jungle Planks");
        context.Blocks.Register(BlockCategory.Wood, junglePlanksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Jungle Planks",
            BlockId = "crafty.jungle_planks"
        });
        context.Logger.Log("crafty.jungle_planks created successfully");

        var jungleLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.jungle_log", "Jungle Log");
        context.Blocks.Register(BlockCategory.Wood, jungleLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Jungle Log",
            BlockId = "crafty.jungle_log"
        });
        context.Logger.Log("crafty.jungle_log created successfully");

        var strippedJungleLogBlock = BlocksCreator.CreateTwoSidedTextureBlock("crafty.jungle_stripped_log", "Jungle Stripped Log");
        context.Blocks.Register(BlockCategory.Wood, strippedJungleLogBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Jungle Stripped Log",
            BlockId = "crafty.jungle_stripped_log"
        });
        context.Logger.Log("crafty.jungle_stripped_log created successfully");

        // leaves are here:

        var oakLeavesBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.oak_leaves", "Oak Leaves");
        context.Blocks.Register(BlockCategory.Wood, oakLeavesBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Oak Leaves",
            BlockId = "crafty.oak_leaves"
        });
        context.Logger.Log("crafty.oak_leaves created successfully");

        var birchLeavesBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.birch_leaves", "Birch Leaves");
        birchLeavesBlock.Properties = new() { Opaque = false, Transparent = false };
        context.Blocks.Register(BlockCategory.Wood, birchLeavesBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Birch Leaves",
            BlockId = "crafty.birch_leaves"
        });
        context.Logger.Log("crafty.birch_leaves created successfully");

        var acaciaLeavesBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.acacia_leaves", "Acacia Leaves");
        acaciaLeavesBlock.Properties = new() { Opaque = false, Transparent = false };
        context.Blocks.Register(BlockCategory.Wood, acaciaLeavesBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Acacia Leaves",
            BlockId = "crafty.acacia_leaves"
        });
        context.Logger.Log("crafty.acacia_leaves created successfully");

        var cherryLeavesBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cherry_leaves", "Cherry Leaves");
        cherryLeavesBlock.Properties = new() { Opaque = false, Transparent = false };
        context.Blocks.Register(BlockCategory.Wood, cherryLeavesBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cherry Leaves",
            BlockId = "crafty.cherry_leaves"
        });
        context.Logger.Log("crafty.cherry_leaves created successfully");

        var darkOakLeavesBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.dark_oak_leaves", "Dark Oak Leaves");
        darkOakLeavesBlock.Properties = new() { Opaque = false, Transparent = false };
        context.Blocks.Register(BlockCategory.Wood, darkOakLeavesBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Dark Oak Leaves",
            BlockId = "crafty.dark_oak_leaves"
        });
        context.Logger.Log("crafty.dark_oak_leaves created successfully");

        var jungleLeavesBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.jungle_leaves", "Jungle Leaves");
        jungleLeavesBlock.Properties = new() { Opaque = false, Transparent = false };
        context.Blocks.Register(BlockCategory.Wood, jungleLeavesBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Jungle Leaves",
            BlockId = "crafty.jungle_leaves"
        });
        context.Logger.Log("crafty.jungle_leaves created successfully");

        var mangroveLeavesBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.mangrove_leaves", "Mangrove Leaves");
        mangroveLeavesBlock.Properties = new() { Opaque = false, Transparent = false };
        context.Blocks.Register(BlockCategory.Wood, mangroveLeavesBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Mangrove Leaves",
            BlockId = "crafty.mangrove_leaves"
        });
        context.Logger.Log("crafty.mangrove_leaves created successfully");

        var spruceLeavesBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.spruce_leaves", "Spruce Leaves");
        spruceLeavesBlock.Properties = new() { Opaque = false, Transparent = false };
        context.Blocks.Register(BlockCategory.Wood, spruceLeavesBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Spruce Leaves",
            BlockId = "crafty.spruce_leaves"
        });
        context.Logger.Log("crafty.spruce_leaves created successfully");
    }

    private void Nature(IModContext context)
    {
        var air = BlocksCreator.CreateAirBlock();
        context.Blocks.Register(BlockCategory.Nature, air);
        context.Items.Register(ItemCategory.Block, new Item() { HiddenItem = true });
        context.Logger.Log("crafty.air created successfully");

        var grassBlock = BlocksCreator.CreateThreeSidedTextureBlock("crafty.grass", "Grass Block");
        context.Blocks.Register(BlockCategory.Nature, grassBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Grass block",
            BlockId = "crafty.grass"
        });
        context.Logger.Log("crafty.grass created successfully");

        var dirtBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.dirt", "Dirt");
        context.Blocks.Register(BlockCategory.Nature, dirtBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Dirt",
            BlockId = "crafty.dirt"
        });
        context.Logger.Log("crafty.dirt created successfully");

        var courseDirtBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.coarse_dirt", "Coarse Dirt");
        context.Blocks.Register(BlockCategory.Nature, courseDirtBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Course Dirt",
            BlockId = "crafty.coarse_dirt"
        });
        context.Logger.Log("crafty.coarse_dirt created successfully");

        var podzolBlock = BlocksCreator.CreateThreeSidedTextureBlock("crafty.podzol", "Podzol");
        context.Blocks.Register(BlockCategory.Nature, podzolBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Podzol",
            BlockId = "crafty.podzol"
        });
        context.Logger.Log("crafty.podzol created successfully");

        var clayBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.clay_block", "Clay Block");
        context.Blocks.Register(BlockCategory.Nature, clayBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Clay Block",
            BlockId = "crafty.clay_block"
        });
        context.Logger.Log("crafty.clay_block created successfully");

        var iceBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.ice", "Ice");
        context.Blocks.Register(BlockCategory.Nature, iceBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Ice",
            BlockId = "crafty.ice"
        });
        context.Logger.Log("crafty.ice created successfully");

        var packedIceBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.packed_ice", "Packed Ice");
        context.Blocks.Register(BlockCategory.Nature, packedIceBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Packed Ice",
            BlockId = "crafty.packed_ice"
        });
        context.Logger.Log("crafty.packed_ice created successfully");

        var snowBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.snow_block", "Snow Block");
        context.Blocks.Register(BlockCategory.Nature, snowBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Snow Block",
            BlockId = "crafty.snow_block"
        });
        context.Logger.Log("crafty.snow_block created successfully");

        var myceliumBlock = BlocksCreator.CreateThreeSidedTextureBlock("crafty.mycelium", "Mycelium");
        context.Blocks.Register(BlockCategory.Nature, myceliumBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Mycelium",
            BlockId = "crafty.mycelium"
        });
        context.Logger.Log("crafty.mycelium created successfully");

        var mossBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.moss_block", "Moss Block");
        context.Blocks.Register(BlockCategory.Nature, mossBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Moss Block",
            BlockId = "crafty.moss_block"
        });
        context.Logger.Log("crafty.moss_block created successfully");
    }

    private void StoneAndUndergroundBlocks(IModContext context)
    {
        var stoneBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.stone", "Stone");
        context.Blocks.Register(BlockCategory.Stone, stoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Stone",
            BlockId = "crafty.stone"
        });
        context.Logger.Log("crafty.stone created successfully");

        var sandBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.sand", "Sand");
        context.Blocks.Register(BlockCategory.Stone, sandBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Sand",
            BlockId = "crafty.sand"
        });
        context.Logger.Log("crafty.sand created successfully");

        var redSandBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.red_sand", "Red Sand");
        context.Blocks.Register(BlockCategory.Stone, redSandBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Red Sand",
            BlockId = "crafty.red_sand"
        });
        context.Logger.Log("crafty.red_sand created successfully");

        var cobblestoneBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cobblestone", "Cobblestone");
        context.Blocks.Register(BlockCategory.Stone, cobblestoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cobblestone",
            BlockId = "crafty.cobblestone"
        });
        context.Logger.Log("crafty.cobblestone created successfully");

        var sandstoneBlock = BlocksCreator.CreateOneTwoSidedTextureBlock("crafty.sandstone", "Sandstone");
        context.Blocks.Register(BlockCategory.Stone, sandstoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Sandstone",
            BlockId = "crafty.sandstone"
        });
        context.Logger.Log("crafty.sandstone created successfully");

        var redSandstoneBlock = BlocksCreator.CreateOneTwoSidedTextureBlock("crafty.red_sandstone", "Red Sandstone");
        context.Blocks.Register(BlockCategory.Stone, redSandstoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Red Sandstone",
            BlockId = "crafty.red_sandstone"
        });
        context.Logger.Log("crafty.red_sandstone created successfully");

        var gravelBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.gravel", "Gravel");
        context.Blocks.Register(BlockCategory.Stone, gravelBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Gravel",
            BlockId = "crafty.gravel"
        });
        context.Logger.Log("crafty.gravel created successfully");

        var deepslateBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.deepslate", "Deepslate");
        context.Blocks.Register(BlockCategory.Stone, deepslateBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Deepslate",
            BlockId = "crafty.deepslate"
        });
        context.Logger.Log("crafty.deepslate created successfully");

        var cobbledDeepslateBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cobbled_deepslate", "Cobbled Deepslate");
        context.Blocks.Register(BlockCategory.Stone, cobbledDeepslateBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cobbled Deepslate",
            BlockId = "crafty.cobbled_deepslate"
        });
        context.Logger.Log("crafty.cobbled_deepslate created successfully");

        var graniteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.granite", "Granite");
        context.Blocks.Register(BlockCategory.Stone, graniteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Granite",
            BlockId = "crafty.granite"
        });
        context.Logger.Log("crafty.granite created successfully");

        var dioriteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.diorite", "Diorite");
        context.Blocks.Register(BlockCategory.Stone, dioriteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Diorite",
            BlockId = "crafty.diorite"
        });
        context.Logger.Log("crafty.diorite created successfully");

        var andesiteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.andesite", "Andesite");
        context.Blocks.Register(BlockCategory.Stone, andesiteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Andesite",
            BlockId = "crafty.andesite"
        });
        context.Logger.Log("crafty.andesite created successfully");

        var basaltBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.basalt", "Basalt");
        context.Blocks.Register(BlockCategory.Stone, basaltBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Basalt",
            BlockId = "crafty.basalt"
        });
        context.Logger.Log("crafty.basalt created successfully");

        var calciteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.calcite", "Calcite");
        context.Blocks.Register(BlockCategory.Stone, calciteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Calcite",
            BlockId = "crafty.calcite"
        });
        context.Logger.Log("crafty.calcite created successfully");

        var tuffBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.tuff", "Tuff");
        context.Blocks.Register(BlockCategory.Stone, tuffBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Tuff",
            BlockId = "crafty.tuff"
        });
        context.Logger.Log("crafty.tuff created successfully");

        var coreShellBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.core_shell", "Core Shell");
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
        var coalOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.coal_ore", "Coal Ore");
        context.Blocks.Register(BlockCategory.Ore, coalOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Coal Ore",
            BlockId = "crafty.coal_ore"
        });
        context.Logger.Log("crafty.coal_ore created successfully");

        var ironOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.iron_ore", "Iron Ore");
        context.Blocks.Register(BlockCategory.Ore, ironOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Iron Ore",
            BlockId = "crafty.iron_ore"
        });
        context.Logger.Log("crafty.iron_ore created successfully");

        var goldOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.gold_ore", "Gold Ore");
        context.Blocks.Register(BlockCategory.Ore, goldOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Gold Ore",
            BlockId = "crafty.gold_ore"
        });
        context.Logger.Log("crafty.gold_ore created successfully");

        var copperOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.copper_ore", "Copper Ore");
        context.Blocks.Register(BlockCategory.Ore, copperOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Copper Ore",
            BlockId = "crafty.copper_ore"
        });
        context.Logger.Log("crafty.copper_ore created successfully");

        var diamondOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.diamond_ore", "Diamond Ore");
        context.Blocks.Register(BlockCategory.Ore, diamondOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Diamond Ore",
            BlockId = "crafty.diamond_ore"
        });
        context.Logger.Log("crafty.diamond_ore created successfully");

        var emeraldOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.emerald_ore", "Emerald Ore");
        context.Blocks.Register(BlockCategory.Ore, emeraldOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Emerald Ore",
            BlockId = "crafty.emerald_ore"
        });
        context.Logger.Log("crafty.emerald_ore created successfully");

        var redDustOreBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.red_dust_ore", "Red Dust Ore");
        context.Blocks.Register(BlockCategory.Ore, redDustOreBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Red Dust Ore",
            BlockId = "crafty.red_dust_ore"
        });
        context.Logger.Log("crafty.red_dust_ore created successfully");
    }

    private void BuildingBlocks(IModContext context)
    {
        var glassBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.glass", "Glass");
        glassBlock.Properties = new() { Opaque = false, Transparent = true };
        context.Blocks.Register(BlockCategory.Building, glassBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Glass",
            BlockId = "crafty.glass"
        });
        context.Logger.Log("crafty.glass created successfully");

        var brickBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.bricks", "Bricks");
        context.Blocks.Register(BlockCategory.Building, brickBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Bricks",
            BlockId = "crafty.bricks"
        });
        context.Logger.Log("crafty.bricks created successfully");

        var smoothStoneBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.smooth_stone", "Smooth Stone");
        context.Blocks.Register(BlockCategory.Building, smoothStoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Smooth Stone",
            BlockId = "crafty.smooth_stone"
        });
        context.Logger.Log("crafty.smooth_stone created successfully");

        var smoothSandstoneBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.smooth_sandstone", "Smooth Sandstone");
        context.Blocks.Register(BlockCategory.Building, smoothSandstoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Smooth Sandstone",
            BlockId = "crafty.smooth_sandstone"
        });
        context.Logger.Log("crafty.smooth_sandstone created successfully");

        var smoothRedSandstoneBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.smooth_red_sandstone", "Smooth Red Sandstone");
        context.Blocks.Register(BlockCategory.Building, smoothRedSandstoneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Smooth Red Sandstone",
            BlockId = "crafty.smooth_red_sandstone"
        });
        context.Logger.Log("crafty.smooth_red_sandstone created successfully");

        var stoneBricksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.stone_bricks", "Stone Bricks");
        context.Blocks.Register(BlockCategory.Building, stoneBricksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Stone Bricks",
            BlockId = "crafty.stone_bricks"
        });
        context.Logger.Log("crafty.stone_bricks created successfully");

        var quartzBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.quartz_block", "Quartz Bricks");
        context.Blocks.Register(BlockCategory.Building, quartzBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Quartz Bricks",
            BlockId = "crafty.quartz_block"
        });
        context.Logger.Log("crafty.quartz_block created successfully");

        var mossyStoneBricksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.mossy_stone_bricks", "Mossy Stone Bricks");
        context.Blocks.Register(BlockCategory.Building, mossyStoneBricksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Mossy Stone Bricks",
            BlockId = "crafty.mossy_stone_bricks"
        });
        context.Logger.Log("crafty.mossy_stone_bricks created successfully");

        var crackedStoneBricksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cracked_stone_bricks", "Cracked Stone Bricks");
        context.Blocks.Register(BlockCategory.Building, crackedStoneBricksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cracked Stone Bricks",
            BlockId = "crafty.cracked_stone_bricks"
        });
        context.Logger.Log("crafty.cracked_stone_bricks created successfully");

        var chiseledStoneBricksBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.chiseled_stone_bricks", "Chiseled Stone Bricks");
        context.Blocks.Register(BlockCategory.Building, chiseledStoneBricksBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Chiseled Stone Bricks",
            BlockId = "crafty.chiseled_stone_bricks"
        });
        context.Logger.Log("crafty.chiseled_stone_bricks created successfully");

        var glassPaneBlock = BlocksCreator.CreateGlassPaneBlock("crafty.glass_pane", "Glass Pane");
        context.Blocks.Register(BlockCategory.Building, glassPaneBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Glass Pane",
            BlockId = "crafty.glass_pane"
        });
        context.Logger.Log("crafty.glass_pane created successfully");

        Concrete(context);

        Terracotta(context);
    }

    private void Concrete(IModContext context)
    {
        var whiteConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.white_concrete", "White Concrete");
        context.Blocks.Register(BlockCategory.Building, whiteConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "White Concrete",
            BlockId = "crafty.white_concrete"
        });
        context.Logger.Log("crafty.white_concrete created successfully");

        var blackConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.black_concrete", "Black Concrete");
        context.Blocks.Register(BlockCategory.Building, blackConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Black Concrete",
            BlockId = "crafty.black_concrete"
        });
        context.Logger.Log("crafty.black_concrete created successfully");

        var blueConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.blue_concrete", "Blue Concrete");
        context.Blocks.Register(BlockCategory.Building, blueConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Blue Concrete",
            BlockId = "crafty.blue_concrete"
        });
        context.Logger.Log("crafty.blue_concrete created successfully");

        var grayConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.gray_concrete", "Gray Concrete");
        context.Blocks.Register(BlockCategory.Building, grayConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Gray Concrete",
            BlockId = "crafty.gray_concrete"
        });
        context.Logger.Log("crafty.gray_concrete created successfully");

        var greenConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.green_concrete", "Green Concrete");
        context.Blocks.Register(BlockCategory.Building, greenConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Green Concrete",
            BlockId = "crafty.green_concrete"
        });
        context.Logger.Log("crafty.green_concrete created successfully");

        var redConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.red_concrete", "Red Concrete");
        context.Blocks.Register(BlockCategory.Building, redConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Red Concrete",
            BlockId = "crafty.red_concrete"
        });
        context.Logger.Log("crafty.red_concrete created successfully");

        var yellowConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.yellow_concrete", "Yellow Concrete");
        context.Blocks.Register(BlockCategory.Building, yellowConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Yellow Concrete",
            BlockId = "crafty.yellow_concrete"
        });
        context.Logger.Log("crafty.yellow_concrete created successfully");

        var orangeConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.orange_concrete", "Orange Concrete");
        context.Blocks.Register(BlockCategory.Building, orangeConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Orange Concrete",
            BlockId = "crafty.orange_concrete"
        });
        context.Logger.Log("crafty.orange_concrete created successfully");

        var pinkConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.pink_concrete", "Pink Concrete");
        context.Blocks.Register(BlockCategory.Building, pinkConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Pink Concrete",
            BlockId = "crafty.pink_concrete"
        });
        context.Logger.Log("crafty.pink_concrete created successfully");

        var purpleConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.purple_concrete", "Purple Concrete");
        context.Blocks.Register(BlockCategory.Building, purpleConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Purple Concrete",
            BlockId = "crafty.purple_concrete"
        });
        context.Logger.Log("crafty.purple_concrete created successfully");

        var magentaConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.magenta_concrete", "Magenta Concrete");
        context.Blocks.Register(BlockCategory.Building, magentaConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Magenta Concrete",
            BlockId = "crafty.magenta_concrete"
        });
        context.Logger.Log("crafty.magenta_concrete created successfully");

        var limeConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.lime_concrete", "Lime Concrete");
        context.Blocks.Register(BlockCategory.Building, limeConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Lime Concrete",
            BlockId = "crafty.lime_concrete"
        });
        context.Logger.Log("crafty.lime_concrete created successfully");

        var lightGrayConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.light_gray_concrete", "Light Gray Concrete");
        context.Blocks.Register(BlockCategory.Building, lightGrayConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Light Gray Concrete",
            BlockId = "crafty.light_gray_concrete"
        });
        context.Logger.Log("crafty.light_gray_concrete created successfully");

        var lightBlueConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.light_blue_concrete", "Light Blue Concrete");
        context.Blocks.Register(BlockCategory.Building, lightBlueConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Light Blue Concrete",
            BlockId = "crafty.light_blue_concrete"
        });
        context.Logger.Log("crafty.light_blue_concrete created successfully");

        var cyanConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cyan_concrete", "Cyan Concrete");
        context.Blocks.Register(BlockCategory.Building, cyanConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cyan Concrete",
            BlockId = "crafty.cyan_concrete"
        });
        context.Logger.Log("crafty.cyan_concrete created successfully");

        var brownConcreteBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.brown_concrete", "Brown Concrete");
        context.Blocks.Register(BlockCategory.Building, brownConcreteBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Brown Concrete",
            BlockId = "crafty.brown_concrete"
        });
        context.Logger.Log("crafty.brown_concrete created successfully");
    }

    private void Terracotta(IModContext context)
    {
        var whiteTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.white_terracotta", "White Terracotta");
        context.Blocks.Register(BlockCategory.Building, whiteTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "White Terracotta",
            BlockId = "crafty.white_terracotta"
        });
        context.Logger.Log("crafty.white_terracotta created successfully");

        var yellowTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.yellow_terracotta", "Yellow Terracotta");
        context.Blocks.Register(BlockCategory.Building, yellowTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Yellow Terracotta",
            BlockId = "crafty.yellow_terracotta"
        });
        context.Logger.Log("crafty.yellow_terracotta created successfully");

        var redTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.red_terracotta", "Red Terracotta");
        context.Blocks.Register(BlockCategory.Building, redTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Red Terracotta",
            BlockId = "crafty.red_terracotta"
        });
        context.Logger.Log("crafty.red_terracotta created successfully");

        var purpleTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.purple_terracotta", "Purple Terracotta");
        context.Blocks.Register(BlockCategory.Building, purpleTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Purple Terracotta",
            BlockId = "crafty.purple_terracotta"
        });
        context.Logger.Log("crafty.purple_terracotta created successfully");

        var pinkTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.pink_terracotta", "Pink Terracotta");
        context.Blocks.Register(BlockCategory.Building, pinkTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Pink Terracotta",
            BlockId = "crafty.pink_terracotta"
        });
        context.Logger.Log("crafty.pink_terracotta created successfully");

        var orangeTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.orange_terracotta", "Orange Terracotta");
        context.Blocks.Register(BlockCategory.Building, orangeTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Orange Terracotta",
            BlockId = "crafty.orange_terracotta"
        });
        context.Logger.Log("crafty.orange_terracotta created successfully");

        var magentaTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.magenta_terracotta", "Magenta Terracotta");
        context.Blocks.Register(BlockCategory.Building, magentaTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Magenta Terracotta",
            BlockId = "crafty.magenta_terracotta"
        });
        context.Logger.Log("crafty.magenta_terracotta created successfully");

        var limeTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.lime_terracotta", "Lime Terracotta");
        context.Blocks.Register(BlockCategory.Building, limeTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Lime Terracotta",
            BlockId = "crafty.lime_terracotta"
        });
        context.Logger.Log("crafty.lime_terracotta created successfully");

        var lightGrayTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.light_gray_terracotta", "Light Gray Terracotta");
        context.Blocks.Register(BlockCategory.Building, lightGrayTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Light Gray Terracotta",
            BlockId = "crafty.light_gray_terracotta"
        });
        context.Logger.Log("crafty.light_gray_terracotta created successfully");

        var lightBlueTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.light_blue_terracotta", "Light Blue Terracotta");
        context.Blocks.Register(BlockCategory.Building, lightBlueTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Light Blue Terracotta",
            BlockId = "crafty.light_blue_terracotta"
        });
        context.Logger.Log("crafty.light_blue_terracotta created successfully");

        var greenTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.green_terracotta", "Green Terracotta");
        context.Blocks.Register(BlockCategory.Building, greenTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Green Terracotta",
            BlockId = "crafty.green_terracotta"
        });
        context.Logger.Log("crafty.green_terracotta created successfully");

        var grayTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.gray_terracotta", "Gray Terracotta");
        context.Blocks.Register(BlockCategory.Building, grayTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Gray Terracotta",
            BlockId = "crafty.gray_terracotta"
        });
        context.Logger.Log("crafty.gray_terracotta created successfully");

        var cyanTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.cyan_terracotta", "Cyan Terracotta");
        context.Blocks.Register(BlockCategory.Building, cyanTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Cyan Terracotta",
            BlockId = "crafty.cyan_terracotta"
        });
        context.Logger.Log("crafty.cyan_terracotta created successfully");

        var brownTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.brown_terracotta", "Brown Terracotta");
        context.Blocks.Register(BlockCategory.Building, brownTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Brown Terracotta",
            BlockId = "crafty.brown_terracotta"
        });
        context.Logger.Log("crafty.brown_terracotta created successfully");

        var blueTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.blue_terracotta", "Blue Terracotta");
        context.Blocks.Register(BlockCategory.Building, blueTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Blue Terracotta",
            BlockId = "crafty.blue_terracotta"
        });
        context.Logger.Log("crafty.blue_terracotta created successfully");

        var blackTerracottaBlock = BlocksCreator.CreateOneSidedTextureBlock("crafty.black_terracotta", "Black Terracotta");
        context.Blocks.Register(BlockCategory.Building, blackTerracottaBlock);
        context.Items.Register(ItemCategory.Block, new Item()
        {
            Name = "Black Terracotta",
            BlockId = "crafty.black_terracotta"
        });
        context.Logger.Log("crafty.black_terracotta created successfully");
    }
}
