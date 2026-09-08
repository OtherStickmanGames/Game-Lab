using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.Inventory;
using DwarfClone.Entity.Character;

namespace DwarfClone.Crafting
{
    [Serializable]
    public class CraftingStationDefinition
    {
        public CraftingStationType stationType;
        public string stationId;
        public string displayName;
        public string description;
        public SkillType primarySkill;
        public string spriteName;
        public List<ItemStack> buildCosts;

        public CraftingStationDefinition(
            CraftingStationType stationType,
            string stationId,
            string displayName,
            string description,
            SkillType primarySkill,
            string spriteName = null,
            List<ItemStack> buildCosts = null)
        {
            this.stationType = stationType;
            this.stationId = stationId;
            this.displayName = displayName;
            this.description = description;
            this.primarySkill = primarySkill;
            this.spriteName = spriteName ?? stationId;
            this.buildCosts = buildCosts ?? new List<ItemStack>();
        }

        public Sprite GetSprite()
        {
            return Resources.Load<Sprite>($"{Constants.WORKBENCHES_SPRITES_PATH}{spriteName}");
        }
    }
}
