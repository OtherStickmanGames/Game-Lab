using System.Collections.Generic;
using UnityEngine;

namespace DwarfClone.Inventory
{
    public class ItemDatabase
    {
        private static ItemDatabase instance;
        public static ItemDatabase Instance => instance ?? (instance = new ItemDatabase());

        private readonly Dictionary<string, ItemData> itemMap = new Dictionary<string, ItemData>();
        private readonly List<ItemData> allItems = new List<ItemData>();

        public IReadOnlyList<ItemData> AllItems => allItems;

        public ItemDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (itemMap.Count > 0) return;

            // Tools
            Register(new ItemData("item_pickaxe_miner", "Miner's Pickaxe", "Essential tool for mining rock and ores.", ItemCategory.Tool, ToolType.Pickaxe, damage: 8f));
            Register(new ItemData("item_axe_woodcutter", "Woodcutter's Axe", "Sharp axe for felling timber and clearing trees.", ItemCategory.Tool, ToolType.Axe, damage: 10f));
            Register(new ItemData("item_shovel", "Iron Spade", "Durable shovel for excavating dirt, sand, and clay.", ItemCategory.Tool, ToolType.Shovel, damage: 5f));
            Register(new ItemData("item_fishing_rod", "Bamboo Fishing Rod", "Used for angling fish in rivers and lakes.", ItemCategory.Tool, ToolType.FishingRod));
            Register(new ItemData("item_hammer", "Smithing Hammer", "Heavy hammer used for forging and construction.", ItemCategory.Tool, ToolType.Hammer, damage: 7f));

            // Weapons
            Register(new ItemData("item_sword_iron", "Iron Broadsword", "Balanced forged steel blade for combat.", ItemCategory.Weapon, ToolType.None, damage: 22f));
            Register(new ItemData("item_mace_heavy", "Flanged War Mace", "Crushing blunt weapon effective against armored foes.", ItemCategory.Weapon, ToolType.None, damage: 26f));
            Register(new ItemData("item_spear", "Hunting Spear", "Long reach piercing spear for hunting and defense.", ItemCategory.Weapon, ToolType.None, damage: 18f));
            Register(new ItemData("item_bow_wood", "Yew Shortbow", "Ranged bow for shooting arrows from afar.", ItemCategory.Weapon, ToolType.None, damage: 16f));

            // Armor & Apparel
            Register(new ItemData("item_armor_leather", "Studded Leather Cuirass", "Flexible protective armor made of hardened leather.", ItemCategory.Armor, ToolType.None, armorRating: 15f));
            Register(new ItemData("item_armor_plate", "Steel Plate Armor", "Heavy protective plate armor offering superb defense.", ItemCategory.Armor, ToolType.None, armorRating: 40f));
            Register(new ItemData("item_helmet_iron", "Iron Spangenhelm", "Reinforced helmet protecting the head from lethal strikes.", ItemCategory.Armor, ToolType.None, armorRating: 25f));
            Register(new ItemData("item_backpack", "Leather Backpack", "Expands carry inventory capacity.", ItemCategory.Armor, ToolType.None, armorRating: 5f));

            // Raw Resources
            Register(new ItemData("item_wood_log", "Wood Log", "Natural felled tree trunk for carpentry and fuel.", ItemCategory.Material));
            Register(new ItemData("item_wood_plank", "Wood Plank", "Cut timber board for construction and crafting.", ItemCategory.Material));
            Register(new ItemData("item_stone_rough", "Rough Stone", "Natural stone rubble from mining rock.", ItemCategory.Material));
            Register(new ItemData("item_stone_block", "Stone Block", "Chiseled architectural block for sturdy structures.", ItemCategory.Material));
            Register(new ItemData("item_coal", "Coal Lump", "Combustible fossil fuel for smelting forges.", ItemCategory.Material));
            Register(new ItemData("item_ore_iron", "Iron Ore", "Raw iron mineral ready for smelting.", ItemCategory.Material));
            Register(new ItemData("item_ore_copper", "Copper Ore", "Green-veined copper mineral for smelting.", ItemCategory.Material));
            Register(new ItemData("item_ore_gold", "Gold Ore", "Precious golden ore vein chunk.", ItemCategory.Material));
            Register(new ItemData("item_ore_silver", "Silver Ore", "Glittering raw silver ore chunk.", ItemCategory.Material));
            Register(new ItemData("item_ingot_iron", "Iron Ingot", "Smelted bar of workable iron.", ItemCategory.Material));
            Register(new ItemData("item_ingot_copper", "Copper Ingot", "Smelted bar of copper.", ItemCategory.Material));
            Register(new ItemData("item_ingot_gold", "Gold Bullion Bar", "Refined 24k gold bullion.", ItemCategory.Valuable));
            Register(new ItemData("item_ingot_silver", "Silver Ingot", "Bright refined silver ingot.", ItemCategory.Valuable));
            Register(new ItemData("item_hide_raw", "Raw Animal Hide", "Uncured animal pelt from hunting.", ItemCategory.Material));
            Register(new ItemData("item_leather", "Tanned Leather", "Treated durable leather for armor and gear.", ItemCategory.Material));
            Register(new ItemData("item_fiber_plant", "Plant Fiber / Cotton", "Raw natural fibers from crops for spinning.", ItemCategory.Material));
            Register(new ItemData("item_cloth", "Woven Cloth", "Bolt of fabric for clothes and bandages.", ItemCategory.Material));
            Register(new ItemData("item_clay", "Malleable Clay", "Natural clay for brickmaking and pottery.", ItemCategory.Material));
            Register(new ItemData("item_brick", "Fired Masonry Brick", "Kiln-baked brick for walls and chimneys.", ItemCategory.Material));
            Register(new ItemData("item_sand", "Fine Quartz Sand", "Sand for glassmaking and mortar.", ItemCategory.Material));
            Register(new ItemData("item_glass_pane", "Glass Sheet", "Transparent glass pane for windows and vials.", ItemCategory.Material));
            Register(new ItemData("item_seeds", "Crop Seeds", "Agricultural seeds for farming plots.", ItemCategory.Material));

            // Food & Provisions
            Register(new ItemData("item_bread", "Crusty Wheat Bread", "Nutritious baked bread loaf.", ItemCategory.Food, nutrition: 35f));
            Register(new ItemData("item_meat_raw", "Raw Animal Meat", "Fresh uncooked steak.", ItemCategory.Food, nutrition: 15f));
            Register(new ItemData("item_meat_stew", "Hearty Meat Stew", "Warm savory pot of meat and vegetable stew.", ItemCategory.Food, nutrition: 60f));
            Register(new ItemData("item_ale_mug", "Dwarven Ale Mug", "Frothy invigorating ale. Restores morale and hunger.", ItemCategory.Food, nutrition: 20f));
            Register(new ItemData("item_berries", "Wild Berries", "Sweet gathered wild forest berries.", ItemCategory.Food, nutrition: 15f));

            // Medicine
            Register(new ItemData("item_bandage", "Herbal Linen Bandage", "Stops bleeding immediately and aids wound recovery.", ItemCategory.Medicine, healingValue: 30f));
            Register(new ItemData("item_healing_salve", "Antiseptic Salve", "Medicinal ointment that accelerates limb regeneration.", ItemCategory.Medicine, healingValue: 50f));

            // Valuables & Tech
            Register(new ItemData("item_gem_rough", "Rough Gemstone", "Uncut sparkling crystal from deep rocks.", ItemCategory.Valuable));
            Register(new ItemData("item_ring_gold", "Ornate Gold Ring", "Jeweler forged ring set with fine cut ruby.", ItemCategory.Valuable));
            Register(new ItemData("item_mechanism", "Bronze Mechanism Gear", "Precision clockwork gears for traps and levers.", ItemCategory.Material));
        }

        private void Register(ItemData item)
        {
            itemMap[item.id] = item;
            allItems.Add(item);
        }

        public ItemData GetItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            itemMap.TryGetValue(id, out var item);
            return item;
        }

        public bool HasItem(string id)
        {
            return itemMap.ContainsKey(id);
        }
    }
}
