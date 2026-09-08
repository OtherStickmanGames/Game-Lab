using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.Inventory;
using DwarfClone.World.ZLevel;
using DwarfClone.Entity.Character;

namespace DwarfClone.Crafting
{
    [Serializable]
    public class CraftOrder
    {
        public Recipe recipe;
        public int orderedCount;
        public CraftingMode mode;

        public CraftOrder(Recipe recipe, int count, CraftingMode mode)
        {
            this.recipe = recipe;
            this.orderedCount = count;
            this.mode = mode;
        }
    }

    public class CraftingStation : MonoBehaviour
    {
        public static readonly List<CraftingStation> AllStations = new List<CraftingStation>();

        public event Action OnQueueChanged;

        [Header("Station Setup")]
        [SerializeField] private CraftingStationType stationType;
        [SerializeField] private Vector3Int gridPosition;

        private readonly List<CraftOrder> productionQueue = new List<CraftOrder>();
        private float currentCraftProgress = 0f;
        private DwarfCharacterController assignedWorker;
        private bool isPermanentWorker = false;
        private SpriteRenderer spriteRenderer;

        public CraftingStationType StationType => stationType;
        public Vector3Int GridPosition => gridPosition;
        public IReadOnlyList<CraftOrder> ProductionQueue => productionQueue;
        public float CurrentProgress => currentCraftProgress;
        public DwarfCharacterController AssignedWorker => assignedWorker;
        public bool IsPermanentWorker => isPermanentWorker;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = Constants.SORTING_ORDER_BUILDINGS;
        }

        private void OnEnable()
        {
            AllStations.Add(this);
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged += HandleZLevelChanged;
            }
            UpdateVisibility();
        }

        private void OnDisable()
        {
            AllStations.Remove(this);
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged -= HandleZLevelChanged;
            }
        }

        public void Initialize(CraftingStationType type, Vector3Int pos)
        {
            this.stationType = type;
            this.gridPosition = pos;
            transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);

            var def = RecipeDatabase.Instance.GetStationDef(type);
            if (def != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = def.GetSprite();
            }

            UpdateVisibility();
        }

        public void AddOrder(Recipe recipe, int count = 1, CraftingMode mode = CraftingMode.SingleOrder)
        {
            if (recipe == null || count <= 0) return;
            productionQueue.Add(new CraftOrder(recipe, count, mode));
            OnQueueChanged?.Invoke();
        }

        public void RemoveOrder(int index)
        {
            if (index >= 0 && index < productionQueue.Count)
            {
                productionQueue.RemoveAt(index);
                currentCraftProgress = 0f;
                OnQueueChanged?.Invoke();
            }
        }

        public void ToggleContinuous(int index)
        {
            if (index >= 0 && index < productionQueue.Count)
            {
                var o = productionQueue[index];
                o.mode = (o.mode == CraftingMode.SingleOrder) ? CraftingMode.ContinuousOnFlow : CraftingMode.SingleOrder;
                OnQueueChanged?.Invoke();
            }
        }

        public void AssignWorker(DwarfCharacterController worker, bool permanent)
        {
            assignedWorker = worker;
            isPermanentWorker = permanent;
        }

        public void ClearWorker()
        {
            assignedWorker = null;
            isPermanentWorker = false;
        }

        public bool HasWork()
        {
            return productionQueue.Count > 0;
        }

        public CraftOrder GetCurrentOrder()
        {
            return productionQueue.Count > 0 ? productionQueue[0] : null;
        }

        public bool AdvanceCraft(float deltaWork, InventorySystem workerInventory)
        {
            if (productionQueue.Count == 0) return false;

            var order = productionQueue[0];
            currentCraftProgress += deltaWork;

            if (currentCraftProgress >= order.recipe.craftTime)
            {
                // Finished crafting an item
                currentCraftProgress = 0f;

                // Produce output
                if (order.recipe.output != null && order.recipe.output.item != null)
                {
                    if (workerInventory != null && workerInventory.AddItem(order.recipe.output.item, order.recipe.output.count))
                    {
                        // Stored in worker backpack
                    }
                    else
                    {
                        // Drop on ground at station
                        WorldItem.Spawn(order.recipe.output.item, order.recipe.output.count, gridPosition);
                    }
                }

                // Decrement order count if single order
                if (order.mode == CraftingMode.SingleOrder)
                {
                    order.orderedCount--;
                    if (order.orderedCount <= 0)
                    {
                        productionQueue.RemoveAt(0);
                    }
                }

                OnQueueChanged?.Invoke();
                return true;
            }

            return false;
        }

        private void HandleZLevelChanged(int curZ)
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            int curZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;
            gameObject.SetActive(gridPosition.z == curZ);
        }
    }
}
