using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.World.Chunk;
using DwarfClone.World.Tile;
using DwarfClone.Entity.Character;
using DwarfClone.Entity.Animal;
using DwarfClone.Entity.Selection;
using DwarfClone.Inventory;

namespace DwarfClone.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private bool isGameStarted = false;
        [SerializeField] private Vector3Int spawnLocation;

        public bool IsGameStarted => isGameStarted;
        public Vector3Int SpawnLocation => spawnLocation;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            StartNewColony();
        }

        public void StartNewColony()
        {
            if (isGameStarted) return;
            isGameStarted = true;

            Debug.Log("[GameManager] Starting New Colony Sim...");

            // 1. Generate Procedural World
            if (WorldGenerator.Instance != null)
            {
                WorldGenerator.Instance.GenerateWorld();
            }

            // 2. Find Walkable Spawn Center on Surface
            spawnLocation = FindSuitableSpawnLocation();
            Debug.Log($"[GameManager] Squad spawn position determined at: {spawnLocation}");

            // 3. Spawn the 5 Starting Dwarfs
            SpawnStartingSquad(spawnLocation);

            // 4. Drop Starting Colony Resource Crates on Ground
            SpawnStartingSupplies(spawnLocation);

            // 5. Spawn Wildlife
            if (AnimalSpawner.Instance != null)
            {
                AnimalSpawner.Instance.SpawnWildlife();
            }

            // 6. Camera focus on spawn location
            if (CameraController.Instance != null)
            {
                CameraController.Instance.FocusOn(new Vector3(spawnLocation.x + 0.5f, spawnLocation.y + 0.5f, 0f));
            }

            // 7. Select Squad
            if (SelectionManager.Instance != null && DwarfCharacterController.Squad.Count > 0)
            {
                SelectionManager.Instance.SelectAll();
            }

            Debug.Log("[GameManager] Colony successfully initialized! Welcome to Game-Lab.");
        }

        private Vector3Int FindSuitableSpawnLocation()
        {
            var grid = WorldGrid.Instance;
            int cx = Constants.WORLD_WIDTH / 2;
            int cy = Constants.WORLD_HEIGHT / 2;
            int z = Constants.SURFACE_Z_LEVEL;

            // Spiral search for a clean walkable grassland or dirt tile
            for (int r = 0; r < 40; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        if (Mathf.Abs(dx) != r && Mathf.Abs(dy) != r) continue;

                        int x = cx + dx;
                        int y = cy + dy;

                        if (grid != null && grid.IsWalkable(x, y, z))
                        {
                            TileType t = grid.GetTile(x, y, z);
                            if (t == TileType.Grass || t == TileType.Dirt)
                            {
                                return new Vector3Int(x, y, z);
                            }
                        }
                    }
                }
            }

            return new Vector3Int(cx, cy, z);
        }

        private void SpawnStartingSquad(Vector3Int center)
        {
            var squadData = new (string name, string prof, string archetype, int offX, int offY)[]
            {
                ("Урист Защитник", "Воин", "warrior", 0, 0),
                ("Торин Рудокоп", "Шахтёр", "miner", 1, 0),
                ("Балин Зодчий", "Строитель", "builder", -1, 0),
                ("Двалин Лекарь", "Доктор", "medic", 0, 1),
                ("Фили Кузнец", "Ремесленник", "smith", 0, -1)
            };

            for (int i = 0; i < squadData.Length; i++)
            {
                var info = squadData[i];
                Vector3Int pos = new Vector3Int(center.x + info.offX, center.y + info.offY, center.z);

                GameObject dwarfObj = new GameObject($"Dwarf_{info.archetype}_{i + 1}");
                var controller = dwarfObj.AddComponent<DwarfCharacterController>();
                controller.Initialize(info.name, info.prof, info.archetype, pos);
            }
        }

        private void SpawnStartingSupplies(Vector3Int center)
        {
            var db = ItemDatabase.Instance;
            if (db == null) return;

            // Drop crates of food, materials, ingots, and medicine near the squad
            WorldItem.Spawn(db.GetItem("item_bread"), 15, new Vector3Int(center.x + 2, center.y, center.z));
            WorldItem.Spawn(db.GetItem("item_wood_plank"), 20, new Vector3Int(center.x + 2, center.y + 1, center.z));
            WorldItem.Spawn(db.GetItem("item_stone_block"), 20, new Vector3Int(center.x + 2, center.y - 1, center.z));
            WorldItem.Spawn(db.GetItem("item_ingot_iron"), 10, new Vector3Int(center.x - 2, center.y, center.z));
            WorldItem.Spawn(db.GetItem("item_bandage"), 8, new Vector3Int(center.x - 2, center.y + 1, center.z));
        }
    }
}
