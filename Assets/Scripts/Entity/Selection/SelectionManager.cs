using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DwarfClone.Core;
using DwarfClone.Entity.Character;
using DwarfClone.Entity.Animal;
using DwarfClone.Crafting;
using DwarfClone.Jobs;
using DwarfClone.Inventory;
using DwarfClone.World.ZLevel;

namespace DwarfClone.Entity.Selection
{
    public class SelectionManager : MonoBehaviour
    {
        public static SelectionManager Instance { get; private set; }

        public event Action OnSelectionChanged;
        public event Action<CraftingStation> OnStationSelected;

        private readonly List<DwarfCharacterController> selectedDwarfs = new List<DwarfCharacterController>();
        private Camera cam;

        private Vector3 dragStart;
        private bool isBoxSelecting = false;

        public IReadOnlyList<DwarfCharacterController> SelectedDwarfs => selectedDwarfs;
        public DwarfCharacterController PrimarySelected => selectedDwarfs.Count > 0 ? selectedDwarfs[0] : null;
        public CraftingStation SelectedStation { get; private set; }
        public bool IsBoxSelecting => isBoxSelecting;
        public Vector3 DragStart => dragStart;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null) cam = Camera.main;

            HandleHotkeys();
            HandleMouseSelection();
            HandleContextualOrders();
        }

        private void HandleHotkeys()
        {
            var squad = DwarfCharacterController.Squad;

            // 1..5 for individual selection
            if (Input.GetKeyDown(KeyCode.Alpha1) && squad.Count > 0) SelectSingle(squad[0]);
            else if (Input.GetKeyDown(KeyCode.Alpha2) && squad.Count > 1) SelectSingle(squad[1]);
            else if (Input.GetKeyDown(KeyCode.Alpha3) && squad.Count > 2) SelectSingle(squad[2]);
            else if (Input.GetKeyDown(KeyCode.Alpha4) && squad.Count > 3) SelectSingle(squad[3]);
            else if (Input.GetKeyDown(KeyCode.Alpha5) && squad.Count > 4) SelectSingle(squad[4]);

            // ~ (BackQuote) or 0 to select whole squad
            if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Alpha0))
            {
                SelectAll();
            }
        }

        private void HandleMouseSelection()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            // Left Mouse Button Down
            if (Input.GetMouseButtonDown(0))
            {
                dragStart = Input.mousePosition;
                isBoxSelecting = true;
            }

            // Left Mouse Button Up
            if (Input.GetMouseButtonUp(0) && isBoxSelecting)
            {
                isBoxSelecting = false;
                Vector3 dragEnd = Input.mousePosition;

                if (Vector3.Distance(dragStart, dragEnd) < 10f)
                {
                    // Single click selection
                    Vector3 worldPos = cam.ScreenToWorldPoint(dragEnd);
                    var clicked = FindDwarfAtWorldPos(worldPos);

                    if (clicked != null)
                    {
                        if (Input.GetKey(KeyCode.LeftShift))
                        {
                            ToggleSelect(clicked);
                        }
                        else
                        {
                            SelectSingle(clicked);
                        }
                    }
                    else
                    {
                        if (!Input.GetKey(KeyCode.LeftShift))
                        {
                            var station = FindStationAtWorldPos(worldPos);
                            if (station != null)
                            {
                                SelectStation(station);
                            }
                            else
                            {
                                ClearSelection();
                            }
                        }
                    }
                }
                else
                {
                    // Marquee Box Selection
                    SelectInScreenRect(dragStart, dragEnd);
                }
            }
        }

        private void HandleContextualOrders()
        {
            // Right Mouse Button Click (Kenshi style orders!)
            if (Input.GetMouseButtonDown(1) && selectedDwarfs.Count > 0)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

                Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int targetGrid = new Vector3Int(
                    Mathf.FloorToInt(mouseWorld.x),
                    Mathf.FloorToInt(mouseWorld.y),
                    ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL
                );

                bool isShift = Input.GetKey(KeyCode.LeftShift);

                // 1. Check if clicked on a Workbench
                var station = FindStationAt(targetGrid);
                if (station != null)
                {
                    if (isShift)
                    {
                        // Shift+RMB: Assign Permanent Job to Primary Selected Dwarf!
                        PrimarySelected?.AI.AssignPermanentStation(station);
                        Debug.Log($"[Order] Assigned permanent operation of {station.StationType} to {PrimarySelected.CharacterName}");
                    }
                    else
                    {
                        // Regular RMB: Execute single task
                        var job = new Job(JobType.CraftAtStation, station.GridPosition, ToolType.None, SkillType.Carpentry, 3f);
                        PrimarySelected?.AI.SetDirectJob(job);
                        Debug.Log($"[Order] Sent {PrimarySelected.CharacterName} to work at {station.StationType}");
                    }
                    return;
                }

                // 2. Check if clicked on a Hostile Animal -> Attack order!
                var animal = FindAnimalAt(targetGrid);
                if (animal != null)
                {
                    for (int i = 0; i < selectedDwarfs.Count; i++)
                    {
                        var d = selectedDwarfs[i];
                        var attackJob = new Job(JobType.Hunt, animal.GridPosition, ToolType.None, SkillType.MeleeCombat, 2f, () => animal.TakeDamage(15f));
                        d.AI.SetDirectJob(attackJob);
                    }
                    Debug.Log($"[Order] Squad ordered to attack {animal.AnimalType}!");
                    return;
                }

                // 3. Check if clicked on an incapacitated/bleeding squad ally -> First Aid order!
                var ally = FindDwarfAtGrid(targetGrid);
                if (ally != null && (ally.Health.IsBleeding || ally.Health.IsIncapacitated))
                {
                    var aidJob = new Job(JobType.FirstAid, ally.GridPosition, ToolType.None, SkillType.Medicine, 2f, () => ally.Health.ApplyFirstAid(null));
                    PrimarySelected?.AI.SetDirectJob(aidJob);
                    Debug.Log($"[Order] Ordered {PrimarySelected.CharacterName} to provide first aid to {ally.CharacterName}!");
                    return;
                }

                // 4. Default: Movement Order (Move squad in formation)
                OrderMoveSquad(targetGrid);
            }
        }

        private void OrderMoveSquad(Vector3Int centerTarget)
        {
            int count = selectedDwarfs.Count;
            for (int i = 0; i < count; i++)
            {
                var dwarf = selectedDwarfs[i];
                if (dwarf == null || dwarf.IsDead || dwarf.Health.IsIncapacitated) continue;

                // Slight offset per squad member so they don't stack on exact same pixel
                int offsetX = (i % 3) - 1;
                int offsetY = (i / 3);
                Vector3Int target = new Vector3Int(centerTarget.x + offsetX, centerTarget.y + offsetY, centerTarget.z);

                dwarf.Movement.MoveTo(target);
                dwarf.AI.SetDirectJob(null); // Clear direct task when manually moving
                dwarf.SetActionText("Moving to position");
            }
        }

        public void SelectSingle(DwarfCharacterController dwarf)
        {
            ClearSelection();
            if (dwarf != null)
            {
                selectedDwarfs.Add(dwarf);
                dwarf.SetSelected(true);
            }
            OnSelectionChanged?.Invoke();
        }

        public void SelectAll()
        {
            ClearSelection();
            var squad = DwarfCharacterController.Squad;
            for (int i = 0; i < squad.Count; i++)
            {
                selectedDwarfs.Add(squad[i]);
                squad[i].SetSelected(true);
            }
            OnSelectionChanged?.Invoke();
        }

        public void ToggleSelect(DwarfCharacterController dwarf)
        {
            if (dwarf == null) return;
            if (selectedDwarfs.Contains(dwarf))
            {
                selectedDwarfs.Remove(dwarf);
                dwarf.SetSelected(false);
            }
            else
            {
                selectedDwarfs.Add(dwarf);
                dwarf.SetSelected(true);
            }
            OnSelectionChanged?.Invoke();
        }

        public void SelectStation(CraftingStation station)
        {
            for (int i = 0; i < selectedDwarfs.Count; i++)
            {
                if (selectedDwarfs[i] != null) selectedDwarfs[i].SetSelected(false);
            }
            selectedDwarfs.Clear();
            OnSelectionChanged?.Invoke();

            SelectedStation = station;
            OnStationSelected?.Invoke(SelectedStation);
        }

        public void ClearSelection()
        {
            for (int i = 0; i < selectedDwarfs.Count; i++)
            {
                if (selectedDwarfs[i] != null) selectedDwarfs[i].SetSelected(false);
            }
            selectedDwarfs.Clear();
            OnSelectionChanged?.Invoke();

            if (SelectedStation != null)
            {
                SelectedStation = null;
                OnStationSelected?.Invoke(null);
            }
        }

        private void SelectInScreenRect(Vector3 start, Vector3 end)
        {
            ClearSelection();

            float xMin = Mathf.Min(start.x, end.x);
            float xMax = Mathf.Max(start.x, end.x);
            float yMin = Mathf.Min(start.y, end.y);
            float yMax = Mathf.Max(start.y, end.y);
            Rect screenRect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

            int curZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;
            var squad = DwarfCharacterController.Squad;

            for (int i = 0; i < squad.Count; i++)
            {
                var d = squad[i];
                if (d == null || d.CurrentZ != curZ) continue;

                Vector3 screenPos = cam.WorldToScreenPoint(d.transform.position);
                if (screenRect.Contains(screenPos))
                {
                    selectedDwarfs.Add(d);
                    d.SetSelected(true);
                }
            }

            OnSelectionChanged?.Invoke();
        }

        private DwarfCharacterController FindDwarfAtWorldPos(Vector3 worldPos)
        {
            int curZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;
            var squad = DwarfCharacterController.Squad;
            for (int i = 0; i < squad.Count; i++)
            {
                var d = squad[i];
                if (d == null || d.CurrentZ != curZ) continue;
                if (Vector3.Distance(worldPos, d.transform.position) < 0.8f) return d;
            }
            return null;
        }

        private CraftingStation FindStationAtWorldPos(Vector3 worldPos)
        {
            int curZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;
            var stations = CraftingStation.AllStations;
            for (int i = 0; i < stations.Count; i++)
            {
                var s = stations[i];
                if (s == null || s.GridPosition.z != curZ) continue;
                if (Vector3.Distance(worldPos, s.transform.position) < 0.8f) return s;
            }
            return null;
        }

        private DwarfCharacterController FindDwarfAtGrid(Vector3Int gridPos)
        {
            var squad = DwarfCharacterController.Squad;
            for (int i = 0; i < squad.Count; i++)
            {
                if (squad[i] != null && squad[i].GridPosition == gridPos) return squad[i];
            }
            return null;
        }

        private CraftingStation FindStationAt(Vector3Int gridPos)
        {
            var stations = CraftingStation.AllStations;
            for (int i = 0; i < stations.Count; i++)
            {
                if (stations[i] != null && stations[i].GridPosition == gridPos) return stations[i];
            }
            return null;
        }

        private AnimalAI FindAnimalAt(Vector3Int gridPos)
        {
            var animals = AnimalAI.AllAnimals;
            for (int i = 0; i < animals.Count; i++)
            {
                if (animals[i] != null && animals[i].GridPosition == gridPos) return animals[i];
            }
            return null;
        }
    }
}
