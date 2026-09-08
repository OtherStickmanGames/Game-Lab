using UnityEngine;
using DwarfClone.Core;
using DwarfClone.World.ZLevel;

namespace DwarfClone.Entity.Base
{
    public abstract class EntityBase : MonoBehaviour
    {
        [Header("Grid Coordinates")]
        [SerializeField] protected Vector3Int gridPosition;

        protected SpriteRenderer spriteRenderer;

        public Vector3Int GridPosition => gridPosition;
        public int CurrentZ => gridPosition.z;
        public abstract bool IsDead { get; }

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = Constants.SORTING_ORDER_ENTITIES;
        }

        protected virtual void Start()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged += HandleZLevelChanged;
            }
            UpdateVisibility();
        }

        protected virtual void OnDestroy()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged -= HandleZLevelChanged;
            }
        }

        public virtual void SetGridPosition(Vector3Int newPos)
        {
            gridPosition = newPos;
            transform.position = new Vector3(newPos.x + 0.5f, newPos.y + 0.5f, 0f);
            UpdateVisibility();
        }

        public virtual void SetGridPositionDirect(int x, int y, int z)
        {
            SetGridPosition(new Vector3Int(x, y, z));
        }

        protected virtual void HandleZLevelChanged(int activeZ)
        {
            UpdateVisibility();
        }

        public virtual void UpdateVisibility()
        {
            int activeZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;
            gameObject.SetActive(gridPosition.z == activeZ);
        }
    }
}
