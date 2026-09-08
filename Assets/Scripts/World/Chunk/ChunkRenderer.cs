using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DwarfClone.Core;
using DwarfClone.World.Tile;
using DwarfClone.World.ZLevel;

namespace DwarfClone.World.Chunk
{
    public class ChunkRenderer : MonoBehaviour
    {
        public static ChunkRenderer Instance { get; private set; }

        [Header("Tilemaps")]
        [SerializeField] private Tilemap currentZTilemap;
        [SerializeField] private Tilemap belowZTilemap;

        private static readonly Dictionary<TileType, UnityEngine.Tilemaps.Tile> tileCache = new Dictionary<TileType, UnityEngine.Tilemaps.Tile>();
        private static readonly Color belowTint = new Color(0.42f, 0.44f, 0.52f, 1.0f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureTilemaps();
        }

        private void Start()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged += HandleZLevelChanged;
            }
            if (WorldGrid.Instance != null)
            {
                WorldGrid.Instance.OnTileChanged += HandleTileChanged;
                WorldGrid.Instance.OnWorldGenerated += RefreshEntireView;
            }
        }

        private void OnDestroy()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged -= HandleZLevelChanged;
            }
            if (WorldGrid.Instance != null)
            {
                WorldGrid.Instance.OnTileChanged -= HandleTileChanged;
                WorldGrid.Instance.OnWorldGenerated -= RefreshEntireView;
            }
        }

        public void EnsureTilemaps()
        {
            if (currentZTilemap == null)
            {
                GameObject curObj = new GameObject("CurrentZ_Tilemap");
                curObj.transform.SetParent(transform);
                currentZTilemap = curObj.AddComponent<Tilemap>();
                var r = curObj.AddComponent<TilemapRenderer>();
                r.sortingOrder = Constants.SORTING_ORDER_TERRAIN;
            }

            if (belowZTilemap == null)
            {
                GameObject belowObj = new GameObject("BelowZ_Tilemap");
                belowObj.transform.SetParent(transform);
                belowZTilemap = belowObj.AddComponent<Tilemap>();
                var r = belowObj.AddComponent<TilemapRenderer>();
                r.sortingOrder = Constants.SORTING_ORDER_TERRAIN_BELOW;
            }
        }

        private void HandleZLevelChanged(int newZ)
        {
            RefreshEntireView();
        }

        private void HandleTileChanged(int x, int y, int z, TileType type)
        {
            int curZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;

            if (z == curZ)
            {
                UpdateCell(x, y, curZ);
            }
            else if (z == curZ - 1)
            {
                UpdateCell(x, y, curZ);
            }
        }

        public void RefreshEntireView()
        {
            EnsureTilemaps();
            currentZTilemap.ClearAllTiles();
            belowZTilemap.ClearAllTiles();

            var grid = WorldGrid.Instance;
            if (grid == null) return;

            int curZ = ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL;
            var reg = TileRegistry.Instance;

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    TileType curType = grid.GetTile(x, y, curZ);
                    Vector3Int pos = new Vector3Int(x, y, 0);

                    if (curType != TileType.Air)
                    {
                        var t = GetOrCreateTile(curType, reg, Color.white);
                        currentZTilemap.SetTile(pos, t);
                    }
                    else if (curZ > 0)
                    {
                        // Show floor below
                        TileType belowType = grid.GetTile(x, y, curZ - 1);
                        if (belowType != TileType.Air)
                        {
                            var t = GetOrCreateTile(belowType, reg, belowTint);
                            belowZTilemap.SetTile(pos, t);
                        }
                    }
                }
            }
        }

        private void UpdateCell(int x, int y, int curZ)
        {
            var grid = WorldGrid.Instance;
            if (grid == null) return;

            Vector3Int pos = new Vector3Int(x, y, 0);
            var reg = TileRegistry.Instance;

            TileType curType = grid.GetTile(x, y, curZ);
            if (curType != TileType.Air)
            {
                currentZTilemap.SetTile(pos, GetOrCreateTile(curType, reg, Color.white));
                belowZTilemap.SetTile(pos, null);
            }
            else
            {
                currentZTilemap.SetTile(pos, null);
                if (curZ > 0)
                {
                    TileType belowType = grid.GetTile(x, y, curZ - 1);
                    if (belowType != TileType.Air)
                    {
                        belowZTilemap.SetTile(pos, GetOrCreateTile(belowType, reg, belowTint));
                    }
                    else
                    {
                        belowZTilemap.SetTile(pos, null);
                    }
                }
            }
        }

        private static UnityEngine.Tilemaps.Tile GetOrCreateTile(TileType type, TileRegistry reg, Color tint)
        {
            Sprite spr = reg.GetSprite(type);
            var tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
            tile.sprite = spr;
            tile.color = tint;
            return tile;
        }
    }
}
