#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using DwarfClone.Core;
using DwarfClone.World.Tile;
using DwarfClone.World.ZLevel;
using DwarfClone.World.Chunk;
using DwarfClone.World.Flora;
using DwarfClone.Pathfinding;
using DwarfClone.Jobs;
using DwarfClone.Building;
using DwarfClone.Building.Zones;
using DwarfClone.Entity.Animal;
using DwarfClone.Entity.Selection;
using DwarfClone.UI;

namespace DwarfClone.Editor
{
    public static class SceneBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/SampleScene.unity";

        [MenuItem("GameLab/Rebuild Main Scene")]
        public static void BuildMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Main Camera
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            Camera cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 10f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.1f, 1f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<CameraController>();

            // 2. Event System
            GameObject esObj = new GameObject("EventSystem");
            esObj.AddComponent<EventSystem>();
            esObj.AddComponent<StandaloneInputModule>();

            // 3. Systems Root
            GameObject systemsRoot = new GameObject("[Systems_Root]");

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
            systemsRoot.AddComponent<UIManager>();
            systemsRoot.AddComponent<GameManager>();

            // Save Scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SceneBuilder] Main Scene successfully generated and saved to {SCENE_PATH}!");
        }
    }
}
#endif
