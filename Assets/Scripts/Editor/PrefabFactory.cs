#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using DwarfClone.Entity.Character;
using DwarfClone.Entity.Animal;
using DwarfClone.Entity.Base;
using DwarfClone.Medicine;
using DwarfClone.Inventory;
using DwarfClone.Crafting;

namespace DwarfClone.Editor
{
    public static class PrefabFactory
    {
        private const string PREFABS_DIR = "Assets/Prefabs";

        [MenuItem("GameLab/Create Prefabs")]
        public static void CreateAllPrefabs()
        {
            if (!Directory.Exists(PREFABS_DIR))
            {
                Directory.CreateDirectory(PREFABS_DIR);
            }

            CreateDwarfPrefab();
            CreateAnimalPrefab();
            CreateCraftingStationPrefab();
            CreateWorldItemPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PrefabFactory] All game prefabs generated successfully in Assets/Prefabs/!");
        }

        private static void CreateDwarfPrefab()
        {
            GameObject obj = new GameObject("DwarfCharacter");
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<HealthSystem>();
            obj.AddComponent<CharacterNeeds>();
            obj.AddComponent<CharacterSkills>();
            obj.AddComponent<InventorySystem>();
            obj.AddComponent<EntityMovement>();
            obj.AddComponent<EntityAnimator>();
            obj.AddComponent<CharacterAI>();
            obj.AddComponent<DwarfCharacterController>();

            string path = $"{PREFABS_DIR}/DwarfCharacter.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        private static void CreateAnimalPrefab()
        {
            GameObject obj = new GameObject("Animal");
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<EntityMovement>();
            obj.AddComponent<EntityAnimator>();
            obj.AddComponent<AnimalAI>();

            string path = $"{PREFABS_DIR}/Animal.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        private static void CreateCraftingStationPrefab()
        {
            GameObject obj = new GameObject("CraftingStation");
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<CraftingStation>();

            string path = $"{PREFABS_DIR}/CraftingStation.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        private static void CreateWorldItemPrefab()
        {
            GameObject obj = new GameObject("WorldItem");
            obj.AddComponent<SpriteRenderer>();
            obj.AddComponent<WorldItem>();

            string path = $"{PREFABS_DIR}/WorldItem.prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
#endif
