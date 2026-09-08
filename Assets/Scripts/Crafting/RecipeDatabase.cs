using System.Collections.Generic;
using DwarfClone.Inventory;
using DwarfClone.Entity.Character;

namespace DwarfClone.Crafting
{
    public class RecipeDatabase
    {
        private static RecipeDatabase instance;
        public static RecipeDatabase Instance => instance ?? (instance = new RecipeDatabase());

        private readonly Dictionary<CraftingStationType, List<Recipe>> stationRecipes = new Dictionary<CraftingStationType, List<Recipe>>();
        private readonly Dictionary<CraftingStationType, CraftingStationDefinition> stationDefs = new Dictionary<CraftingStationType, CraftingStationDefinition>();
        private readonly List<Recipe> allRecipes = new List<Recipe>();

        public RecipeDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (stationDefs.Count > 0) return;

            var items = ItemDatabase.Instance;

            // 1. Carpenter's Bench
            RegisterStation(new CraftingStationDefinition(CraftingStationType.CarpenterBench, "carpenter_bench", "Carpenter's Bench", "Basic carpentry workshop for woodworking.", SkillType.Carpentry));
            AddRecipe("rec_planks", "Cut Planks", CraftingStationType.CarpenterBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_wood_log"), 1) },
                new ItemStack(items.GetItem("item_wood_plank"), 2), 2f, SkillType.Carpentry);

            // 2. Sawmill
            RegisterStation(new CraftingStationDefinition(CraftingStationType.Sawmill, "sawmill", "Sawmill", "High-efficiency mechanized circular saw for lumber.", SkillType.Woodcutting));
            AddRecipe("rec_sawmill_planks", "Industrial Plank Milling", CraftingStationType.Sawmill,
                new List<ItemStack> { new ItemStack(items.GetItem("item_wood_log"), 1) },
                new ItemStack(items.GetItem("item_wood_plank"), 4), 1.5f, SkillType.Woodcutting);

            // 3. Stonecutter's Table
            RegisterStation(new CraftingStationDefinition(CraftingStationType.StonecutterTable, "stonecutter_table", "Stonecutter's Table", "Chisels rough quarried stone into square masonry blocks.", SkillType.Stonecutting));
            AddRecipe("rec_stone_blocks", "Chisel Stone Blocks", CraftingStationType.StonecutterTable,
                new List<ItemStack> { new ItemStack(items.GetItem("item_stone_rough"), 2) },
                new ItemStack(items.GetItem("item_stone_block"), 2), 2.5f, SkillType.Stonecutting);

            // 4. Smelter
            RegisterStation(new CraftingStationDefinition(CraftingStationType.Smelter, "smelter", "Ore Smelter", "Furnace bloomery for smelting raw minerals into metal ingots.", SkillType.Smelting));
            AddRecipe("rec_smelt_iron", "Smelt Iron Ingot", CraftingStationType.Smelter,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ore_iron"), 2), new ItemStack(items.GetItem("item_coal"), 1) },
                new ItemStack(items.GetItem("item_ingot_iron"), 1), 4f, SkillType.Smelting);
            AddRecipe("rec_smelt_copper", "Smelt Copper Ingot", CraftingStationType.Smelter,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ore_copper"), 2), new ItemStack(items.GetItem("item_coal"), 1) },
                new ItemStack(items.GetItem("item_ingot_copper"), 1), 3f, SkillType.Smelting);
            AddRecipe("rec_smelt_gold", "Smelt Gold Bullion", CraftingStationType.Smelter,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ore_gold"), 2), new ItemStack(items.GetItem("item_coal"), 1) },
                new ItemStack(items.GetItem("item_ingot_gold"), 1), 5f, SkillType.Smelting);
            AddRecipe("rec_smelt_silver", "Smelt Silver Ingot", CraftingStationType.Smelter,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ore_silver"), 2), new ItemStack(items.GetItem("item_coal"), 1) },
                new ItemStack(items.GetItem("item_ingot_silver"), 1), 4.5f, SkillType.Smelting);

            // 5. Blacksmith's Anvil
            RegisterStation(new CraftingStationDefinition(CraftingStationType.BlacksmithAnvil, "blacksmith_anvil", "Blacksmith's Anvil", "Heavy iron anvil for forging tools and hardware.", SkillType.Smithing));
            AddRecipe("rec_forge_pickaxe", "Forge Miner's Pickaxe", CraftingStationType.BlacksmithAnvil,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 2), new ItemStack(items.GetItem("item_wood_plank"), 1) },
                new ItemStack(items.GetItem("item_pickaxe_miner"), 1), 4f, SkillType.Smithing);
            AddRecipe("rec_forge_axe", "Forge Woodcutter's Axe", CraftingStationType.BlacksmithAnvil,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 2), new ItemStack(items.GetItem("item_wood_plank"), 1) },
                new ItemStack(items.GetItem("item_axe_woodcutter"), 1), 4f, SkillType.Smithing);
            AddRecipe("rec_forge_shovel", "Forge Iron Spade", CraftingStationType.BlacksmithAnvil,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 1), new ItemStack(items.GetItem("item_wood_plank"), 1) },
                new ItemStack(items.GetItem("item_shovel"), 1), 3f, SkillType.Smithing);
            AddRecipe("rec_forge_hammer", "Forge Smithing Hammer", CraftingStationType.BlacksmithAnvil,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 2), new ItemStack(items.GetItem("item_wood_plank"), 1) },
                new ItemStack(items.GetItem("item_hammer"), 1), 3.5f, SkillType.Smithing);

            // 6. Weaponsmith's Bench
            RegisterStation(new CraftingStationDefinition(CraftingStationType.WeaponsmithBench, "weaponsmith_bench", "Weaponsmith's Bench", "Specialized bench for crafting lethal martial weaponry.", SkillType.Smithing));
            AddRecipe("rec_forge_sword", "Forge Iron Broadsword", CraftingStationType.WeaponsmithBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 3), new ItemStack(items.GetItem("item_leather"), 1) },
                new ItemStack(items.GetItem("item_sword_iron"), 1), 5f, SkillType.Smithing);
            AddRecipe("rec_forge_mace", "Forge War Mace", CraftingStationType.WeaponsmithBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 3), new ItemStack(items.GetItem("item_wood_plank"), 1) },
                new ItemStack(items.GetItem("item_mace_heavy"), 1), 5f, SkillType.Smithing);
            AddRecipe("rec_forge_spear", "Forge Hunting Spear", CraftingStationType.WeaponsmithBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 1), new ItemStack(items.GetItem("item_wood_plank"), 2) },
                new ItemStack(items.GetItem("item_spear"), 1), 4f, SkillType.Smithing);

            // 7. Armorer's Forge
            RegisterStation(new CraftingStationDefinition(CraftingStationType.ArmorsmithForge, "armorsmith_forge", "Armorer's Forge", "Forge for shaping steel plates into protective suits.", SkillType.Smithing));
            AddRecipe("rec_forge_plate", "Forge Plate Armor", CraftingStationType.ArmorsmithForge,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 6), new ItemStack(items.GetItem("item_leather"), 2) },
                new ItemStack(items.GetItem("item_armor_plate"), 1), 8f, SkillType.Smithing);
            AddRecipe("rec_forge_helmet", "Forge Iron Helmet", CraftingStationType.ArmorsmithForge,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 3) },
                new ItemStack(items.GetItem("item_helmet_iron"), 1), 4.5f, SkillType.Smithing);

            // 8. Tanning Rack
            RegisterStation(new CraftingStationDefinition(CraftingStationType.TanningRack, "tanning_rack", "Tanning Rack", "Scrapes and cures raw animal hides into supple leather.", SkillType.Tanning));
            AddRecipe("rec_cure_leather", "Cure Leather", CraftingStationType.TanningRack,
                new List<ItemStack> { new ItemStack(items.GetItem("item_hide_raw"), 1) },
                new ItemStack(items.GetItem("item_leather"), 1), 3f, SkillType.Tanning);

            // 9. Leatherworker's Bench
            RegisterStation(new CraftingStationDefinition(CraftingStationType.LeatherworkerBench, "leatherworker_bench", "Leatherworker's Bench", "Stitches leather into armor, bags, and boots.", SkillType.Tailoring));
            AddRecipe("rec_leather_armor", "Craft Leather Cuirass", CraftingStationType.LeatherworkerBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_leather"), 4) },
                new ItemStack(items.GetItem("item_armor_leather"), 1), 4.5f, SkillType.Tailoring);
            AddRecipe("rec_craft_backpack", "Stitch Leather Backpack", CraftingStationType.LeatherworkerBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_leather"), 3), new ItemStack(items.GetItem("item_cloth"), 1) },
                new ItemStack(items.GetItem("item_backpack"), 1), 4f, SkillType.Tailoring);

            // 10. Loom
            RegisterStation(new CraftingStationDefinition(CraftingStationType.Loom, "loom", "Weaving Loom", "Spins and weaves raw plant fibers into bolts of cloth.", SkillType.Tailoring));
            AddRecipe("rec_weave_cloth", "Weave Cloth Bolt", CraftingStationType.Loom,
                new List<ItemStack> { new ItemStack(items.GetItem("item_fiber_plant"), 3) },
                new ItemStack(items.GetItem("item_cloth"), 1), 2.5f, SkillType.Tailoring);

            // 11. Tailor's Bench
            RegisterStation(new CraftingStationDefinition(CraftingStationType.TailorBench, "tailor_bench", "Tailor's Bench", "Sews garments, apparel, and sterile linen bandages.", SkillType.Tailoring));
            AddRecipe("rec_sew_bandage", "Sew Herbal Bandage", CraftingStationType.TailorBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_cloth"), 1) },
                new ItemStack(items.GetItem("item_bandage"), 2), 2f, SkillType.Tailoring);

            // 12. Cooking Stove
            RegisterStation(new CraftingStationDefinition(CraftingStationType.CookingStove, "cooking_stove", "Cooking Stove & Hearth", "Prepares hot cooked meals, roasted meat, and savory stews.", SkillType.Cooking));
            AddRecipe("rec_cook_stew", "Cook Meat Stew", CraftingStationType.CookingStove,
                new List<ItemStack> { new ItemStack(items.GetItem("item_meat_raw"), 1), new ItemStack(items.GetItem("item_berries"), 1) },
                new ItemStack(items.GetItem("item_meat_stew"), 1), 3f, SkillType.Cooking);
            AddRecipe("rec_bake_bread", "Bake Wheat Bread", CraftingStationType.CookingStove,
                new List<ItemStack> { new ItemStack(items.GetItem("item_seeds"), 2) },
                new ItemStack(items.GetItem("item_bread"), 1), 2.5f, SkillType.Cooking);

            // 13. Brewery
            RegisterStation(new CraftingStationDefinition(CraftingStationType.Brewery, "brewery", "Dwarven Brewery", "Ferments grain and mountain water into dwarven ale.", SkillType.Brewing));
            AddRecipe("rec_brew_ale", "Brew Dwarven Ale", CraftingStationType.Brewery,
                new List<ItemStack> { new ItemStack(items.GetItem("item_seeds"), 2), new ItemStack(items.GetItem("item_berries"), 1) },
                new ItemStack(items.GetItem("item_ale_mug"), 2), 4f, SkillType.Brewing);

            // 14. Mill
            RegisterStation(new CraftingStationDefinition(CraftingStationType.Mill, "mill", "Quern Mill", "Rotary millstone for grinding wheat grain and minerals.", SkillType.Farming));
            AddRecipe("rec_mill_seeds", "Process Crop Seeds", CraftingStationType.Mill,
                new List<ItemStack> { new ItemStack(items.GetItem("item_bread"), 1) },
                new ItemStack(items.GetItem("item_seeds"), 3), 2f, SkillType.Farming);

            // 15. Butcher's Table
            RegisterStation(new CraftingStationDefinition(CraftingStationType.ButcherTable, "butcher_table", "Butcher's Table", "Dismantles animal carcasses into meat and hides.", SkillType.Cooking));
            AddRecipe("rec_butcher_meat", "Prepare Fresh Steaks", CraftingStationType.ButcherTable,
                new List<ItemStack> { new ItemStack(items.GetItem("item_hide_raw"), 1) },
                new ItemStack(items.GetItem("item_meat_raw"), 2), 2f, SkillType.Cooking);

            // 16. Jeweler's Bench
            RegisterStation(new CraftingStationDefinition(CraftingStationType.JewelerBench, "jeweler_bench", "Jeweler's Bench", "Cuts precious gems and solders ornate rings.", SkillType.Jewelry));
            AddRecipe("rec_craft_ring", "Craft Ornate Gold Ring", CraftingStationType.JewelerBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_gold"), 1), new ItemStack(items.GetItem("item_gem_rough"), 1) },
                new ItemStack(items.GetItem("item_ring_gold"), 1), 5f, SkillType.Jewelry);

            // 17. Apothecary Table
            RegisterStation(new CraftingStationDefinition(CraftingStationType.ApothecaryTable, "apothecary_table", "Apothecary Table", "Prepares medicinal salves, antiseptics, and remedies.", SkillType.Medicine));
            AddRecipe("rec_craft_salve", "Brew Antiseptic Salve", CraftingStationType.ApothecaryTable,
                new List<ItemStack> { new ItemStack(items.GetItem("item_berries"), 2) },
                new ItemStack(items.GetItem("item_healing_salve"), 1), 3f, SkillType.Medicine);

            // 18. Masonry Bench
            RegisterStation(new CraftingStationDefinition(CraftingStationType.MasonryBench, "masonry_bench", "Masonry Bench", "Carves architectural stonework and statues.", SkillType.Stonecutting));
            AddRecipe("rec_mason_block", "Reinforced Stone Blocks", CraftingStationType.MasonryBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_stone_rough"), 3) },
                new ItemStack(items.GetItem("item_stone_block"), 3), 3f, SkillType.Stonecutting);

            // 19. Kiln
            RegisterStation(new CraftingStationDefinition(CraftingStationType.Kiln, "kiln", "Brick Kiln", "Bakes soft clay into durable red masonry bricks.", SkillType.Stonecutting));
            AddRecipe("rec_bake_brick", "Fire Clay Bricks", CraftingStationType.Kiln,
                new List<ItemStack> { new ItemStack(items.GetItem("item_clay"), 2), new ItemStack(items.GetItem("item_coal"), 1) },
                new ItemStack(items.GetItem("item_brick"), 2), 3.5f, SkillType.Stonecutting);

            // 20. Glassmaker's Furnace
            RegisterStation(new CraftingStationDefinition(CraftingStationType.GlassmakerFurnace, "glassmaker_furnace", "Glassmaker's Furnace", "Melts fine quartz sand into clear glass panes.", SkillType.Smelting));
            AddRecipe("rec_melt_glass", "Blow Glass Pane", CraftingStationType.GlassmakerFurnace,
                new List<ItemStack> { new ItemStack(items.GetItem("item_sand"), 2), new ItemStack(items.GetItem("item_coal"), 1) },
                new ItemStack(items.GetItem("item_glass_pane"), 1), 3f, SkillType.Smelting);

            // 21. Mechanic's Bench
            RegisterStation(new CraftingStationDefinition(CraftingStationType.MechanicBench, "mechanic_bench", "Mechanic's Bench", "Assembles bronze gears, springs, and clockwork levers.", SkillType.Smithing));
            AddRecipe("rec_craft_mechanism", "Assemble Mechanism Gear", CraftingStationType.MechanicBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_ingot_iron"), 1), new ItemStack(items.GetItem("item_ingot_copper"), 1) },
                new ItemStack(items.GetItem("item_mechanism"), 1), 4f, SkillType.Smithing);

            // 22. Siege Workshop
            RegisterStation(new CraftingStationDefinition(CraftingStationType.SiegeWorkshop, "siege_workshop", "Siege & Defense Workshop", "Constructs heavy barricades and projectile emplacements.", SkillType.Carpentry));
            AddRecipe("rec_craft_bow", "Craft Yew Shortbow", CraftingStationType.SiegeWorkshop,
                new List<ItemStack> { new ItemStack(items.GetItem("item_wood_plank"), 2), new ItemStack(items.GetItem("item_fiber_plant"), 1) },
                new ItemStack(items.GetItem("item_bow_wood"), 1), 3.5f, SkillType.Carpentry);

            // 23. Research Bench
            RegisterStation(new CraftingStationDefinition(CraftingStationType.ResearchBench, "research_bench", "Research Tech Desk", "Drafts engineering blueprints and colony research.", SkillType.Construction));
            AddRecipe("rec_tech_draft", "Draft Engineering Scroll", CraftingStationType.ResearchBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_cloth"), 1) },
                new ItemStack(items.GetItem("item_mechanism"), 1), 6f, SkillType.Construction);

            // 24. Toolmaker's Bench
            RegisterStation(new CraftingStationDefinition(CraftingStationType.ToolmakerBench, "toolmaker_bench", "Toolmaker's Bench", "Precision crafting of specialized agrarian and angling gear.", SkillType.Carpentry));
            AddRecipe("rec_craft_rod", "Assemble Fishing Rod", CraftingStationType.ToolmakerBench,
                new List<ItemStack> { new ItemStack(items.GetItem("item_wood_plank"), 1), new ItemStack(items.GetItem("item_fiber_plant"), 1) },
                new ItemStack(items.GetItem("item_fishing_rod"), 1), 3f, SkillType.Carpentry);

            // 25. Fishery Station
            RegisterStation(new CraftingStationDefinition(CraftingStationType.FisheryStation, "fishery_station", "Fishery Cleaning Station", "Cleans, fillets, and dries river catches.", SkillType.Cooking));
            AddRecipe("rec_clean_fish", "Prepare Fish Stew", CraftingStationType.FisheryStation,
                new List<ItemStack> { new ItemStack(items.GetItem("item_berries"), 1) },
                new ItemStack(items.GetItem("item_meat_stew"), 1), 2.5f, SkillType.Cooking);
        }

        private void RegisterStation(CraftingStationDefinition def)
        {
            stationDefs[def.stationType] = def;
            if (!stationRecipes.ContainsKey(def.stationType))
            {
                stationRecipes[def.stationType] = new List<Recipe>();
            }
        }

        private void AddRecipe(string id, string name, CraftingStationType station, List<ItemStack> inputs, ItemStack output, float time, SkillType skill)
        {
            var r = new Recipe(id, name, station, inputs, output, time, skill);
            allRecipes.Add(r);
            if (!stationRecipes.ContainsKey(station))
            {
                stationRecipes[station] = new List<Recipe>();
            }
            stationRecipes[station].Add(r);
        }

        public CraftingStationDefinition GetStationDef(CraftingStationType type)
        {
            stationDefs.TryGetValue(type, out var def);
            return def;
        }

        public IReadOnlyList<Recipe> GetRecipesForStation(CraftingStationType type)
        {
            if (stationRecipes.TryGetValue(type, out var list))
            {
                return list;
            }
            return new List<Recipe>();
        }

        public IReadOnlyDictionary<CraftingStationType, CraftingStationDefinition> AllStations => stationDefs;
    }
}
