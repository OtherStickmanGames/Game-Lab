using System;
using UnityEngine;
using DwarfClone.Core;

namespace DwarfClone.World.ZLevel
{
    public class ZLevelManager : MonoBehaviour
    {
        public static ZLevelManager Instance { get; private set; }

        public event Action<int> OnZLevelChanged;

        [Header("Z-Level Tracking")]
        [SerializeField] private int currentZ = Constants.SURFACE_Z_LEVEL;

        public int CurrentZ => currentZ;
        public int MinZ => 0;
        public int MaxZ => Constants.Z_LEVELS - 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // Keyboard shortcuts for Z-Level: < / > or PageUp / PageDown
            if (Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.PageDown))
            {
                DownOneLevel();
            }
            else if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.PageUp))
            {
                UpOneLevel();
            }
        }

        public void UpOneLevel()
        {
            SetZLevel(currentZ + 1);
        }

        public void DownOneLevel()
        {
            SetZLevel(currentZ - 1);
        }

        public void SetZLevel(int newZ)
        {
            int clamped = Mathf.Clamp(newZ, MinZ, MaxZ);
            if (clamped != currentZ)
            {
                currentZ = clamped;
                OnZLevelChanged?.Invoke(currentZ);
            }
        }

        public bool IsVisibleOnCurrentZ(int entityZ)
        {
            return entityZ == currentZ;
        }
    }
}
