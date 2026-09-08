using UnityEngine;
using DwarfClone.Core;
using DwarfClone.World.ZLevel;

namespace DwarfClone.Inventory
{
    public class WorldItem : MonoBehaviour
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int count = 1;
        [SerializeField] private Vector3Int gridPosition;

        private SpriteRenderer spriteRenderer;
        private Vector3 basePosition;

        public ItemData Item => itemData;
        public int Count => count;
        public Vector3Int GridPosition => gridPosition;

        private void Awake()
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = Constants.SORTING_ORDER_ITEMS;
        }

        private void Start()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged += HandleZLevelChanged;
            }
            UpdateVisibility();
        }

        private void OnDestroy()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged -= HandleZLevelChanged;
            }
        }

        private void Update()
        {
            // Subtle floating bob animation
            float bob = Mathf.Sin(Time.time * 3f + (gridPosition.x + gridPosition.y)) * 0.05f;
            transform.position = basePosition + new Vector3(0, bob, 0);
        }

        public void Initialize(ItemData item, int count, Vector3Int pos)
        {
            this.itemData = item;
            this.count = count;
            this.gridPosition = pos;

            basePosition = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
            transform.position = basePosition;

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && item != null)
            {
                spriteRenderer.sprite = item.GetSprite();
            }

            UpdateVisibility();
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

        public static WorldItem Spawn(ItemData item, int count, Vector3Int pos)
        {
            if (item == null || count <= 0) return null;

            GameObject obj = new GameObject($"Item_{item.id}_{pos.x}_{pos.y}");
            WorldItem wi = obj.AddComponent<WorldItem>();
            wi.Initialize(item, count, pos);
            return wi;
        }
    }
}
