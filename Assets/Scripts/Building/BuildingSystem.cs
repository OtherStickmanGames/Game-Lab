using System;
using DwarfClone.Entity.Character;
using UnityEngine;
using UnityEngine.EventSystems;
using DwarfClone.Core;
using DwarfClone.World.Chunk;
using DwarfClone.World.Tile;
using DwarfClone.World.Flora;
using DwarfClone.World.ZLevel;
using DwarfClone.Jobs;
using DwarfClone.Crafting;
using DwarfClone.Building.Zones;
using DwarfClone.Inventory;

namespace DwarfClone.Building
{
    public class BuildingSystem : MonoBehaviour
    {
        public static BuildingSystem Instance { get; private set; }

        public event Action<BuildingMode> OnModeChanged;

        [Header("State")]
        [SerializeField] private BuildingMode currentMode = BuildingMode.None;
        [SerializeField] private CraftingStationType selectedWorkbench = CraftingStationType.CarpenterBench;
        [SerializeField] private ZoneType selectedZone = ZoneType.Stockpile;

        private Camera cam;
        private Vector3Int dragStartGrid;
        private bool isDragging = false;
        private GameObject ghostObject;
        private SpriteRenderer ghostRenderer;

        public BuildingMode CurrentMode => currentMode;
        public CraftingStationType SelectedWorkbench => selectedWorkbench;
        public ZoneType SelectedZone => selectedZone;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            cam = Camera.main;
            CreateGhost();
        }

        private void CreateGhost()
        {
            ghostObject = new GameObject("Building_Ghost");
            ghostObject.transform.SetParent(transform);
            ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
            ghostRenderer.sortingOrder = Constants.SORTING_ORDER_OVERLAY;
            ghostRenderer.color = new Color(0.4f, 0.9f, 0.4f, 0.55f);
            ghostObject.SetActive(false);
        }

        private void Update()
        {
            if (cam == null) cam = Camera.main;

            if (currentMode == BuildingMode.None)
            {
                if (ghostObject.activeSelf) ghostObject.SetActive(false);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetMode(BuildingMode.None);
                return;
            }

            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            int curZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;
            Vector3Int mouseGrid = new Vector3Int(Mathf.FloorToInt(mouseWorld.x), Mathf.FloorToInt(mouseWorld.y), curZ);

            UpdateGhostPreview(mouseGrid);

            // Handle Clicks
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonDown(0))
            {
                dragStartGrid = mouseGrid;
                isDragging = true;
            }

            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                ExecuteAction(dragStartGrid, mouseGrid);
            }
        }

        public void SetMode(BuildingMode mode)
        {
            currentMode = mode;
            if (ghostObject != null) ghostObject.SetActive(mode != BuildingMode.None);
            OnModeChanged?.Invoke(currentMode);
        }

        public void SelectWorkbenchToBuild(CraftingStationType type)
        {
            selectedWorkbench = type;
            SetMode(BuildingMode.BuildWorkbench);
        }

        public void SelectZoneToDesignate(ZoneType zone)
        {
            selectedZone = zone;
            SetMode(BuildingMode.DesignateZone);
        }

        private void UpdateGhostPreview(Vector3Int gridPos)
        {
            if (ghostObject == null || currentMode == BuildingMode.None) return;

            ghostObject.SetActive(true);
            ghostObject.transform.position = new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);

            var reg = TileRegistry.Instance;
            Sprite spr = null;

            if (currentMode == BuildingMode.BuildWall) spr = reg.GetSprite(TileType.Wall_Wood);
            else if (currentMode == BuildingMode.BuildFloor) spr = reg.GetSprite(TileType.Floor_Wood);
            else if (currentMode == BuildingMode.BuildStairsUp) spr = reg.GetSprite(TileType.Stairs_Up);
            else if (currentMode == BuildingMode.BuildStairsDown) spr = reg.GetSprite(TileType.Stairs_Down);
            else if (currentMode == BuildingMode.BuildDoor) spr = reg.GetSprite(TileType.Door_Wood);
            else if (currentMode == BuildingMode.BuildWorkbench)
            {
                var def = RecipeDatabase.Instance.GetStationDef(selectedWorkbench);
                if (def != null) spr = def.GetSprite();
            }
            else if (currentMode == BuildingMode.DigArea)
            {
                spr = Resources.Load<Sprite>("Sprites/UI/ui_cursor_dig");
            }
            else if (currentMode == BuildingMode.ChopTrees)
            {
                spr = Resources.Load<Sprite>("Sprites/UI/ui_cursor_chop");
            }
            else if (currentMode == BuildingMode.GatherPlants)
            {
                spr = Resources.Load<Sprite>("Sprites/UI/ui_cursor_gather");
            }

            if (spr != null) ghostRenderer.sprite = spr;
        }

        private void ExecuteAction(Vector3Int a, Vector3Int b)
        {
            int minX = Mathf.Min(a.x, b.x);
            int maxX = Mathf.Max(a.x, b.x);
            int minY = Mathf.Min(a.y, b.y);
            int maxY = Mathf.Max(a.y, b.y);
            int z = a.z;

            var grid = WorldGrid.Instance;
            var jobs = JobSystem.Instance;
            var reg = TileRegistry.Instance;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);

                    if (currentMode == BuildingMode.DigArea)
                    {
                        TileType cur = grid.GetTile(pos);
                        if (cur != TileType.Air && cur != TileType.Bedrock)
                        {
                            ToolType tool = (cur == TileType.Dirt || cur == TileType.Sand || cur == TileType.Mud) ? ToolType.Shovel : ToolType.Pickaxe;
                            jobs?.AddJob(new Job(JobType.Mine, pos, tool, SkillType.Mining, 2.5f, () => grid.DigTile(pos.x, pos.y, pos.z)));
                        }
                    }
                    else if (currentMode == BuildingMode.ChopTrees)
                    {
                        var flora = FloraManager.Instance?.GetFloraAt(pos);
                        if (flora != null && flora.IsTree)
                        {
                            jobs?.AddJob(new Job(JobType.ChopWood, pos, ToolType.Axe, SkillType.Woodcutting, 3f, () => FloraManager.Instance.HarvestFlora(pos)));
                        }
                    }
                    else if (currentMode == BuildingMode.GatherPlants)
                    {
                        var flora = FloraManager.Instance?.GetFloraAt(pos);
                        if (flora != null && flora.IsGatherable)
                        {
                            jobs?.AddJob(new Job(JobType.GatherPlant, pos, ToolType.None, SkillType.Farming, 2f, () => FloraManager.Instance.HarvestFlora(pos)));
                        }
                    }
                    else if (currentMode == BuildingMode.BuildWall)
                    {
                        grid.SetTile(pos, TileType.Wall_Wood);
                    }
                    else if (currentMode == BuildingMode.BuildFloor)
                    {
                        grid.SetTile(pos, TileType.Floor_Wood);
                    }
                    else if (currentMode == BuildingMode.BuildStairsUp)
                    {
                        grid.SetTile(pos, TileType.Stairs_Up);
                        // Connect downward stairs above if within bounds
                        if (z < Constants.Z_LEVELS - 1)
                        {
                            grid.SetTile(pos.x, pos.y, z + 1, TileType.Stairs_Down);
                        }
                    }
                    else if (currentMode == BuildingMode.BuildStairsDown)
                    {
                        grid.SetTile(pos, TileType.Stairs_Down);
                        if (z > 0)
                        {
                            grid.SetTile(pos.x, pos.y, z - 1, TileType.Stairs_Up);
                        }
                    }
                    else if (currentMode == BuildingMode.BuildDoor)
                    {
                        grid.SetTile(pos, TileType.Door_Wood);
                    }
                    else if (currentMode == BuildingMode.BuildWorkbench)
                    {
                        // Spawn workbench at single clicked cell
                        GameObject wbObj = new GameObject($"Station_{selectedWorkbench}_{pos.x}_{pos.y}");
                        var station = wbObj.AddComponent<CraftingStation>();
                        station.Initialize(selectedWorkbench, pos);
                        return; // Only place one station per click
                    }
                    else if (currentMode == BuildingMode.CancelOrders)
                    {
                        jobs?.CancelJobsAt(pos);
                    }
                }
            }

            if (currentMode == BuildingMode.DesignateZone)
            {
                ZoneManager.Instance?.CreateZone(selectedZone, a, b);
            }
        }
    }
}
