using System;
using System.Collections.Generic;
using DwarfClone.Inventory;
using DwarfClone.Entity.Character;

namespace DwarfClone.Crafting
{
    [Serializable]
    public class Recipe
    {
        public string recipeId;
        public string recipeName;
        public CraftingStationType stationType;
        public List<ItemStack> inputs;
        public ItemStack output;
        public float craftTime;
        public SkillType requiredSkill;
        public int minSkillLevel;

        public Recipe(
            string recipeId,
            string recipeName,
            CraftingStationType stationType,
            List<ItemStack> inputs,
            ItemStack output,
            float craftTime = 3f,
            SkillType requiredSkill = SkillType.Construction,
            int minSkillLevel = 1)
        {
            this.recipeId = recipeId;
            this.recipeName = recipeName;
            this.stationType = stationType;
            this.inputs = inputs ?? new List<ItemStack>();
            this.output = output;
            this.craftTime = craftTime;
            this.requiredSkill = requiredSkill;
            this.minSkillLevel = minSkillLevel;
        }

        public bool CanCraft(InventorySystem inv)
        {
            if (inv == null) return false;
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].item == null) continue;
                if (!inv.HasItem(inputs[i].item.id, inputs[i].count))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ConsumeIngredients(InventorySystem inv)
        {
            if (!CanCraft(inv)) return false;
            for (int i = 0; i < inputs.Count; i++)
            {
                if (inputs[i].item == null) continue;
                inv.RemoveItem(inputs[i].item, inputs[i].count);
            }
            return true;
        }
    }
}
