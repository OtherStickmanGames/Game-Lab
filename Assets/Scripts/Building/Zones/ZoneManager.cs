using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DwarfClone.Core;
using DwarfClone.World.ZLevel;

namespace DwarfClone.Building.Zones
{
    public class ZoneManager : MonoBehaviour
    {
        public static ZoneManager Instance { get; private set; }

        public event Action OnZonesChanged;

        private readonly List<Zone> zones = new List<Zone>();
        private Tilemap zoneTilemap;
        private UnityEngine.Tilemaps.Tile zoneTileAsset;

        public IReadOnlyList<Zone> AllZones => zones;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureTilemap();
        }

        private void Start()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged += HandleZLevelChanged;
            }
        }

        private void OnDestroy()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged -= HandleZLevelChanged;
            }
        }

        private void EnsureTilemap()
        {
            if (zoneTilemap == null)
            {
                GameObject obj = new GameObject("Zone_Overlay_Tilemap");
                obj.transform.SetParent(transform);
                zoneTilemap = obj.AddComponent<Tilemap>();
                var r = obj.AddComponent<TilemapRenderer>();
                r.sortingOrder = Constants.SORTING_ORDER_ZONES;

                // Simple white tile that gets tinted per zone color
                var tex = new Texture2D(32, 32);
                Color[] cols = new Color[32 * 32];
                for (int i = 0; i < cols.Length; i++) cols[i] = Color.white;
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                var spr = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);

                zoneTileAsset = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                zoneTileAsset.sprite = spr;
            }
        }

        public Zone CreateZone(ZoneType type, Vector3Int cornerA, Vector3Int cornerB)
        {
            int minX = Mathf.Min(cornerA.x, cornerB.x);
            int maxX = Mathf.Max(cornerA.x, cornerB.x);
            int minY = Mathf.Min(cornerA.y, cornerB.y);
            int maxY = Mathf.Max(cornerA.y, cornerB.y);
            int z = cornerA.z;

            Color col = GetZoneColor(type);
            string id = $"Zone_{type}_{zones.Count + 1}";
            string name = $"{type} Zone #{zones.Count + 1}";

            Zone zone = new Zone(id, name, type, col);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    zone.AddTile(new Vector3Int(x, y, z));
                }
            }

            zones.Add(zone);
            RefreshOverlay();
            OnZonesChanged?.Invoke();
            return zone;
        }

        public Zone GetZoneAt(Vector3Int pos)
        {
            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].Contains(pos)) return zones[i];
            }
            return null;
        }

        public Vector3Int? GetNearestStockpileTile(Vector3Int fromPos)
        {
            Vector3Int? nearest = null;
            float minDist = float.MaxValue;

            for (int i = 0; i < zones.Count; i++)
            {
                if (zones[i].zoneType == ZoneType.Stockpile)
                {
                    foreach (var tile in zones[i].tiles)
                    {
                        if (tile.z == fromPos.z)
                        {
                            float d = Vector3.Distance(fromPos, tile);
                            if (d < minDist)
                            {
                                minDist = d;
                                nearest = tile;
                            }
                        }
                    }
                }
            }
            return nearest;
        }

        public void DeleteZone(Zone zone)
        {
            if (zone != null && zones.Remove(zone))
            {
                RefreshOverlay();
                OnZonesChanged?.Invoke();
            }
        }

        private void HandleZLevelChanged(int curZ)
        {
            RefreshOverlay();
        }

        public void RefreshOverlay()
        {
            EnsureTilemap();
            zoneTilemap.ClearAllTiles();

            int curZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;

            for (int i = 0; i < zones.Count; i++)
            {
                var z = zones[i];
                var tAsset = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                tAsset.sprite = zoneTileAsset.sprite;
                tAsset.color = z.color;

                foreach (var tile in z.tiles)
                {
                    if (tile.z == curZ)
                    {
                        zoneTilemap.SetTile(new Vector3Int(tile.x, tile.y, 0), tAsset);
                    }
                }
            }
        }

        private Color GetZoneColor(ZoneType type)
        {
            switch (type)
            {
                case ZoneType.Stockpile:
                    return new Color(0.95f, 0.75f, 0.2f, 0.35f); // Gold amber
                case ZoneType.Farm:
                    return new Color(0.2f, 0.85f, 0.3f, 0.35f);  // Green
                case ZoneType.Workshop:
                    return new Color(0.85f, 0.45f, 0.15f, 0.35f);// Warm orange
                case ZoneType.Bedroom:
                    return new Color(0.35f, 0.55f, 0.95f, 0.35f);// Soft blue
                case ZoneType.Fishing:
                    return new Color(0.15f, 0.85f, 0.95f, 0.35f);// Aqua cyan
                case ZoneType.Hunting:
                    return new Color(0.95f, 0.25f, 0.25f, 0.35f);// Crimson red
                default:
                    return new Color(0.8f, 0.8f, 0.8f, 0.35f);
            }
        }
    }
}
