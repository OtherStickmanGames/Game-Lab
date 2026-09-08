using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.World.Tile;
using DwarfClone.World.Chunk;
using DwarfClone.World.ZLevel;
using DwarfClone.Inventory;

namespace DwarfClone.World.Flora
{
    public enum FloraType
    {
        Tree_Oak,
        Tree_Pine,
        Tree_Palm,
        Tree_Dead,
        Berry_Bush,
        Crop_Wheat,
        Crop_Cotton,
        Plant_Herb,
        Mushroom,
        Stone_Boulder,
        Fallen_Log
    }

    public class FloraInstance
    {
        public FloraType type;
        public Vector3Int position;
        public int growthStage; // 0, 1, 2
        public GameObject gameObject;
        public SpriteRenderer renderer;

        public bool IsTree => type == FloraType.Tree_Oak || type == FloraType.Tree_Pine || type == FloraType.Tree_Palm || type == FloraType.Tree_Dead;
        public bool IsCrop => type == FloraType.Crop_Wheat || type == FloraType.Crop_Cotton;
        public bool IsGatherable => type == FloraType.Berry_Bush || type == FloraType.Plant_Herb || type == FloraType.Mushroom || (IsCrop && growthStage >= 2);
    }

    public class FloraManager : MonoBehaviour
    {
        public static FloraManager Instance { get; private set; }

        private readonly Dictionary<Vector3Int, FloraInstance> floraMap = new Dictionary<Vector3Int, FloraInstance>();
        private Transform floraContainer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            GameObject containerObj = new GameObject("Flora_Entities");
            containerObj.transform.SetParent(transform);
            floraContainer = containerObj.transform;
        }

        private void Start()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged += HandleZLevelChanged;
            }
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick += HandleTick;
            }
        }

        private void OnDestroy()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged -= HandleZLevelChanged;
            }
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick -= HandleTick;
            }
        }

        public void PopulateFlora(int seed)
        {
            ClearAll();
            var grid = WorldGrid.Instance;
            if (grid == null) return;

            UnityEngine.Random.InitState(seed);
            int width = Constants.WORLD_WIDTH;
            int height = Constants.WORLD_HEIGHT;
            int z = Constants.SURFACE_Z_LEVEL;

            for (int x = 2; x < width - 2; x++)
            {
                for (int y = 2; y < height - 2; y++)
                {
                    TileType ground = grid.GetTile(x, y, z);
                    if (ground == TileType.Water || ground == TileType.Air) continue;

                    float roll = UnityEngine.Random.value;

                    if (ground == TileType.Grass)
                    {
                        if (roll < 0.04f)
                        {
                            SpawnFlora(FloraType.Tree_Oak, new Vector3Int(x, y, z));
                        }
                        else if (roll < 0.07f)
                        {
                            SpawnFlora(FloraType.Tree_Pine, new Vector3Int(x, y, z));
                        }
                        else if (roll < 0.09f)
                        {
                            SpawnFlora(FloraType.Berry_Bush, new Vector3Int(x, y, z));
                        }
                        else if (roll < 0.11f)
                        {
                            SpawnFlora(FloraType.Plant_Herb, new Vector3Int(x, y, z));
                        }
                        else if (roll < 0.12f)
                        {
                            SpawnFlora(FloraType.Mushroom, new Vector3Int(x, y, z));
                        }
                        else if (roll < 0.13f)
                        {
                            SpawnFlora(FloraType.Fallen_Log, new Vector3Int(x, y, z));
                        }
                    }
                    else if (ground == TileType.Sand)
                    {
                        if (roll < 0.02f)
                        {
                            SpawnFlora(FloraType.Tree_Palm, new Vector3Int(x, y, z));
                        }
                        else if (roll < 0.035f)
                        {
                            SpawnFlora(FloraType.Stone_Boulder, new Vector3Int(x, y, z));
                        }
                    }
                    else if (ground == TileType.Dirt)
                    {
                        if (roll < 0.03f)
                        {
                            SpawnFlora(FloraType.Tree_Dead, new Vector3Int(x, y, z));
                        }
                        else if (roll < 0.05f)
                        {
                            SpawnFlora(FloraType.Stone_Boulder, new Vector3Int(x, y, z));
                        }
                    }
                }
            }
        }

        public FloraInstance SpawnFlora(FloraType type, Vector3Int pos, int initialGrowth = 2)
        {
            if (floraMap.ContainsKey(pos)) return null;

            GameObject obj = new GameObject($"Flora_{type}_{pos.x}_{pos.y}");
            obj.transform.SetParent(floraContainer);
            obj.transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = Constants.SORTING_ORDER_BUILDINGS;

            FloraInstance instance = new FloraInstance
            {
                type = type,
                position = pos,
                growthStage = initialGrowth,
                gameObject = obj,
                renderer = sr
            };

            UpdateSprite(instance);
            floraMap[pos] = instance;

            int curZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;
            obj.SetActive(pos.z == curZ);

            return instance;
        }

        public bool HarvestFlora(Vector3Int pos)
        {
            if (!floraMap.TryGetValue(pos, out var instance)) return false;

            var db = ItemDatabase.Instance;

            if (instance.IsTree)
            {
                // Drops wood logs
                ItemData log = db?.GetItem("item_wood_log");
                if (log != null) WorldItem.Spawn(log, 4, pos);
                DestroyFlora(pos);
                return true;
            }
            else if (instance.type == FloraType.Berry_Bush)
            {
                // Harvest berries, bush stays
                ItemData berry = db?.GetItem("item_berries");
                if (berry != null) WorldItem.Spawn(berry, 3, pos);
                return true;
            }
            else if (instance.type == FloraType.Plant_Herb)
            {
                ItemData herb = db?.GetItem("item_healing_salve");
                if (herb != null) WorldItem.Spawn(herb, 1, pos);
                DestroyFlora(pos);
                return true;
            }
            else if (instance.type == FloraType.Mushroom)
            {
                ItemData food = db?.GetItem("item_berries");
                if (food != null) WorldItem.Spawn(food, 2, pos);
                DestroyFlora(pos);
                return true;
            }
            else if (instance.type == FloraType.Crop_Wheat && instance.growthStage >= 2)
            {
                ItemData wheat = db?.GetItem("item_bread");
                if (wheat != null) WorldItem.Spawn(wheat, 2, pos);
                instance.growthStage = 0;
                UpdateSprite(instance);
                return true;
            }
            else if (instance.type == FloraType.Stone_Boulder)
            {
                ItemData stone = db?.GetItem("item_stone_rough");
                if (stone != null) WorldItem.Spawn(stone, 3, pos);
                DestroyFlora(pos);
                return true;
            }
            else if (instance.type == FloraType.Fallen_Log)
            {
                ItemData log = db?.GetItem("item_wood_log");
                if (log != null) WorldItem.Spawn(log, 2, pos);
                DestroyFlora(pos);
                return true;
            }

            return false;
        }

        public void DestroyFlora(Vector3Int pos)
        {
            if (floraMap.TryGetValue(pos, out var instance))
            {
                if (instance.gameObject != null) Destroy(instance.gameObject);
                floraMap.Remove(pos);
            }
        }

        public FloraInstance GetFloraAt(Vector3Int pos)
        {
            floraMap.TryGetValue(pos, out var instance);
            return instance;
        }

        public bool HasFlora(Vector3Int pos)
        {
            return floraMap.ContainsKey(pos);
        }

        private void UpdateSprite(FloraInstance instance)
        {
            string spriteName = instance.type.ToString();
            if (instance.type == FloraType.Crop_Wheat)
            {
                spriteName = $"Crop_Wheat_{instance.growthStage}";
            }

            Sprite spr = Resources.Load<Sprite>($"{Constants.FLORA_SPRITES_PATH}{spriteName}");
            if (spr != null && instance.renderer != null)
            {
                instance.renderer.sprite = spr;
            }
        }

        private void HandleTick()
        {
            // Advance crop growth occasionally
            foreach (var kvp in floraMap)
            {
                var f = kvp.Value;
                if (f.IsCrop && f.growthStage < 2)
                {
                    if (UnityEngine.Random.value < 0.05f)
                    {
                        f.growthStage++;
                        UpdateSprite(f);
                    }
                }
            }
        }

        private void HandleZLevelChanged(int curZ)
        {
            foreach (var kvp in floraMap)
            {
                var f = kvp.Value;
                if (f.gameObject != null)
                {
                    f.gameObject.SetActive(f.position.z == curZ);
                }
            }
        }

        public void ClearAll()
        {
            foreach (var kvp in floraMap)
            {
                if (kvp.Value.gameObject != null) Destroy(kvp.Value.gameObject);
            }
            floraMap.Clear();
        }
    }
}
