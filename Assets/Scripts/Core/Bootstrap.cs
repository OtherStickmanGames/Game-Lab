using System;
using UnityEngine;
using DwarfClone.World.Tile;
using DwarfClone.World.ZLevel;
using DwarfClone.World.Chunk;
using DwarfClone.World.Flora;
using DwarfClone.Pathfinding;
using DwarfClone.Inventory;
using DwarfClone.Crafting;
using DwarfClone.Jobs;
using DwarfClone.Building;
using DwarfClone.Building.Zones;
using DwarfClone.Entity.Animal;
using DwarfClone.Entity.Selection;
using DwarfClone.UI;

namespace DwarfClone.Core
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeGame()
        {
            if (GameManager.Instance != null)
            {
                Debug.Log("[Bootstrap] GameManager already exists in scene. Skipping bootstrap instantiation.");
                return;
            }

            Debug.Log("[Bootstrap] Initializing Game-Lab Core Systems...");

            // 1. Ensure Camera
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                mainCam = camObj.AddComponent<Camera>();
                mainCam.orthographic = true;
                mainCam.orthographicSize = 10f;
                mainCam.clearFlags = CameraClearFlags.SolidColor;
                mainCam.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
                camObj.AddComponent<AudioListener>();
                camObj.AddComponent<CameraController>();
            }
            else if (mainCam.GetComponent<CameraController>() == null)
            {
                mainCam.gameObject.AddComponent<CameraController>();
            }

            // 2. Systems Container
            GameObject systemsRoot = new GameObject("[Systems_Root]");
            UnityEngine.Object.DontDestroyOnLoad(systemsRoot);

            // Initialize pure C# databases
            var tReg = TileRegistry.Instance;
            var iDb = ItemDatabase.Instance;
            var rDb = RecipeDatabase.Instance;

            // Core MonoBehaviour Managers
            systemsRoot.AddComponent<TimeManager>();
            systemsRoot.AddComponent<ZLevelManager>();
            systemsRoot.AddComponent<WorldGrid>();
            systemsRoot.AddComponent<WorldGenerator>();
            systemsRoot.AddComponent<FloraManager>();
            systemsRoot.AddComponent<ChunkRenderer>();
            systemsRoot.AddComponent<PathGrid>();
            systemsRoot.AddComponent<AStarPathfinder>();
            systemsRoot.AddComponent<JobSystem>();
            systemsRoot.AddComponent<BuildingSystem>();
            systemsRoot.AddComponent<ZoneManager>();
            systemsRoot.AddComponent<AnimalSpawner>();
            systemsRoot.AddComponent<SelectionManager>();

            // UI System
            systemsRoot.AddComponent<UIManager>();

            // Game Manager
            systemsRoot.AddComponent<GameManager>();

            Debug.Log("[Bootstrap] All Game-Lab Systems successfully booted!");
        }
    }
}
