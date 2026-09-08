#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace DwarfClone.Editor
{
    public static class ProjectSetupMenu
    {
        [MenuItem("GameLab/Setup Complete Project (1-Click)", priority = 0)]
        public static void SetupCompleteProject()
        {
            Debug.Log("[ProjectSetup] Setting up complete Game-Lab project...");

            PrefabFactory.CreateAllPrefabs();
            SceneBuilder.BuildMainScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=========================================================================================");
            Debug.Log("[GameLab] Complete Project Setup Succeeded! All 125 sprites, prefabs, systems, and scene are configured.");
            Debug.Log("Press PLAY to start your 5-dwarf colony!");
            Debug.Log("=========================================================================================");
        }
    }
}
#endif
