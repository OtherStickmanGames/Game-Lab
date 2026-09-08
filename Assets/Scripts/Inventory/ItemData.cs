using System;
using UnityEngine;
using DwarfClone.Core;

namespace DwarfClone.Inventory
{
    [Serializable]
    public class ItemData
    {
        public string id;
        public string itemName;
        public string description;
        public ItemCategory category;
        public ToolType toolType = ToolType.None;
        public float damage = 0f;
        public float armorRating = 0f;
        public float nutrition = 0f;
        public float healingValue = 0f;
        public int stackLimit = 99;
        public string spriteName;

        private Sprite cachedSprite;

        public bool IsEdible => nutrition > 0f;
        public bool IsTool => toolType != ToolType.None;
        public bool IsWeapon => damage > 0f;
        public bool IsArmor => armorRating > 0f;
        public bool IsMedical => healingValue > 0f;

        public ItemData(
            string id,
            string itemName,
            string description,
            ItemCategory category,
            ToolType toolType = ToolType.None,
            float damage = 0f,
            float armorRating = 0f,
            float nutrition = 0f,
            float healingValue = 0f,
            int stackLimit = 99,
            string spriteName = null)
        {
            this.id = id;
            this.itemName = itemName;
            this.description = description;
            this.category = category;
            this.toolType = toolType;
            this.damage = damage;
            this.armorRating = armorRating;
            this.nutrition = nutrition;
            this.healingValue = healingValue;
            this.stackLimit = stackLimit;
            this.spriteName = spriteName ?? id;
        }

        public Sprite GetSprite()
        {
            if (cachedSprite != null) return cachedSprite;
            cachedSprite = Resources.Load<Sprite>($"{Constants.ITEMS_SPRITES_PATH}{spriteName}");
            return cachedSprite;
        }
    }
}
