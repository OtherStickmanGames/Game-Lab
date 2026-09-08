using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;

namespace DwarfClone.World.Tile
{
    public class TileRegistry
    {
        private static TileRegistry instance;
        public static TileRegistry Instance => instance ?? (instance = new TileRegistry());

        private readonly Dictionary<TileType, TileData> tileDataMap = new Dictionary<TileType, TileData>();
        private readonly Dictionary<TileType, Sprite> spriteCache = new Dictionary<TileType, Sprite>();

        public TileRegistry()
        {
            RegisterAllTiles();
        }

        private void RegisterAllTiles()
        {
            tileDataMap.Clear();

            // Air
            Register(new TileData(TileType.Air, "Air", 0f, null, 0, false, false, false));

            // Ground & Soil
            Register(new TileData(TileType.Grass, "Grass", 1.5f, "item_seeds", 1, false, true, true));
            Register(new TileData(TileType.Dirt, "Dirt", 1.2f, "item_clay", 1, false, true, true));
            Register(new TileData(TileType.Sand, "Sand", 1.0f, "item_sand", 2, false, true, true));
            Register(new TileData(TileType.Mud, "Mud", 1.4f, "item_clay", 1, false, true, true));
            Register(new TileData(TileType.Water, "Water", 999f, null, 0, false, false, false));
            Register(new TileData(TileType.Bedrock, "Bedrock", 9999f, null, 0, true, false, false));

            // Stones
            Register(new TileData(TileType.Granite, "Granite Rock", 4.5f, "item_stone_rough", 2, true, false, true));
            Register(new TileData(TileType.Limestone, "Limestone Rock", 3.0f, "item_stone_rough", 2, true, false, true));
            Register(new TileData(TileType.Basalt, "Basalt Rock", 5.0f, "item_stone_rough", 2, true, false, true));
            Register(new TileData(TileType.Marble, "Marble Rock", 4.0f, "item_stone_rough", 2, true, false, true));

            // Ores & Minerals
            Register(new TileData(TileType.Coal_Ore, "Coal Ore Vein", 3.5f, "item_coal", 3, true, false, true));
            Register(new TileData(TileType.Iron_Ore, "Iron Ore Vein", 5.0f, "item_ore_iron", 2, true, false, true));
            Register(new TileData(TileType.Copper_Ore, "Copper Ore Vein", 4.0f, "item_ore_copper", 2, true, false, true));
            Register(new TileData(TileType.Gold_Ore, "Gold Ore Vein", 4.5f, "item_ore_gold", 2, true, false, true));
            Register(new TileData(TileType.Silver_Ore, "Silver Ore Vein", 4.5f, "item_ore_silver", 2, true, false, true));

            // Built Structures - Walls
            Register(new TileData(TileType.Wall_Wood, "Wood Wall", 3.0f, "item_wood_plank", 2, true, false, true));
            Register(new TileData(TileType.Wall_Stone, "Stone Wall", 6.0f, "item_stone_block", 2, true, false, true));
            Register(new TileData(TileType.Wall_Brick, "Brick Wall", 7.0f, "item_stone_block", 2, true, false, true));

            // Built Structures - Floors
            Register(new TileData(TileType.Floor_Wood, "Wood Floor", 2.0f, "item_wood_plank", 1, false, true, true));
            Register(new TileData(TileType.Floor_Stone, "Stone Floor", 3.5f, "item_stone_block", 1, false, true, true));

            // Built Structures - Doors
            Register(new TileData(TileType.Door_Wood, "Wood Door", 2.5f, "item_wood_plank", 2, false, true, true));
            Register(new TileData(TileType.Door_Iron, "Iron Door", 6.0f, "item_ingot_iron", 2, false, true, true));

            // Stairs
            Register(new TileData(TileType.Stairs_Up, "Stairs (Up)", 3.0f, "item_stone_rough", 1, false, true, true, connectsZUp: true, connectsZDown: false));
            Register(new TileData(TileType.Stairs_Down, "Stairs (Down)", 3.0f, "item_stone_rough", 1, false, true, true, connectsZUp: false, connectsZDown: true));
            Register(new TileData(TileType.Stairs_Both, "Stairs (Up & Down)", 4.0f, "item_stone_rough", 2, false, true, true, connectsZUp: true, connectsZDown: true));
        }

        private void Register(TileData data)
        {
            tileDataMap[data.type] = data;
        }

        public TileData GetData(TileType type)
        {
            if (tileDataMap.TryGetValue(type, out var data))
            {
                return data;
            }
            return null;
        }

        public Sprite GetSprite(TileType type)
        {
            if (type == TileType.Air) return null;

            if (spriteCache.TryGetValue(type, out var cachedSprite) && cachedSprite != null)
            {
                return cachedSprite;
            }

            TileData data = GetData(type);
            string spriteName = data != null ? data.spriteName : type.ToString();
            Sprite spr = Resources.Load<Sprite>($"{Constants.TILES_SPRITES_PATH}{spriteName}");

            if (spr == null)
            {
                spr = CreateFallbackSprite(type);
            }

            if (spr != null)
            {
                spriteCache[type] = spr;
            }
            return spr;
        }

        private Sprite CreateFallbackSprite(TileType type)
        {
            Color c = Color.gray;
            switch (type)
            {
                case TileType.Grass: c = new Color(0.25f, 0.65f, 0.2f); break;
                case TileType.Dirt: c = new Color(0.5f, 0.35f, 0.2f); break;
                case TileType.Sand: c = new Color(0.85f, 0.75f, 0.45f); break;
                case TileType.Mud: c = new Color(0.35f, 0.28f, 0.2f); break;
                case TileType.Water: c = new Color(0.2f, 0.45f, 0.85f); break;
                case TileType.Bedrock: c = new Color(0.12f, 0.12f, 0.14f); break;
                case TileType.Granite: c = new Color(0.6f, 0.55f, 0.55f); break;
                case TileType.Limestone: c = new Color(0.8f, 0.78f, 0.7f); break;
                case TileType.Basalt: c = new Color(0.25f, 0.25f, 0.3f); break;
                case TileType.Marble: c = new Color(0.9f, 0.9f, 0.92f); break;
                case TileType.Coal_Ore: c = new Color(0.2f, 0.2f, 0.22f); break;
                case TileType.Iron_Ore: c = new Color(0.7f, 0.45f, 0.3f); break;
                case TileType.Copper_Ore: c = new Color(0.75f, 0.45f, 0.2f); break;
                case TileType.Gold_Ore: c = new Color(0.95f, 0.8f, 0.15f); break;
                case TileType.Silver_Ore: c = new Color(0.85f, 0.9f, 0.95f); break;
                case TileType.Wall_Wood: c = new Color(0.48f, 0.3f, 0.15f); break;
                case TileType.Floor_Wood: c = new Color(0.65f, 0.45f, 0.25f); break;
                case TileType.Wall_Stone: c = new Color(0.45f, 0.45f, 0.45f); break;
                case TileType.Floor_Stone: c = new Color(0.55f, 0.55f, 0.55f); break;
                case TileType.Door_Wood: c = new Color(0.55f, 0.32f, 0.15f); break;
                case TileType.Door_Iron: c = new Color(0.6f, 0.65f, 0.7f); break;
                case TileType.Stairs_Up: case TileType.Stairs_Down: case TileType.Stairs_Both: c = new Color(0.7f, 0.65f, 0.55f); break;
            }

            Texture2D tex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        }

        public bool IsSolid(TileType type)
        {
            TileData d = GetData(type);
            return d != null && d.isSolid;
        }

        public bool IsWalkable(TileType type)
        {
            TileData d = GetData(type);
            return d != null && d.isWalkable;
        }

        public bool ConnectsZUp(TileType type)
        {
            TileData d = GetData(type);
            return d != null && d.connectsZUp;
        }

        public bool ConnectsZDown(TileType type)
        {
            TileData d = GetData(type);
            return d != null && d.connectsZDown;
        }
    }
}
