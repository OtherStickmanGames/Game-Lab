using System;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.World.Tile;
using DwarfClone.Inventory;

namespace DwarfClone.World.Chunk
{
    public class WorldGrid : MonoBehaviour
    {
        public static WorldGrid Instance { get; private set; }

        public event Action<int, int, int, TileType> OnTileChanged;
        public event Action OnWorldGenerated;

        private TileType[,,] grid;
        private bool isReady = false;

        public int Width => Constants.WORLD_WIDTH;
        public int Height => Constants.WORLD_HEIGHT;
        public int Depth => Constants.Z_LEVELS;
        public bool IsReady => isReady;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            grid = new TileType[Constants.WORLD_WIDTH, Constants.WORLD_HEIGHT, Constants.Z_LEVELS];
        }

        public void AllocateGrid()
        {
            if (grid == null)
            {
                grid = new TileType[Constants.WORLD_WIDTH, Constants.WORLD_HEIGHT, Constants.Z_LEVELS];
            }
        }

        public void MarkReady()
        {
            isReady = true;
            OnWorldGenerated?.Invoke();
        }

        public bool InBounds(int x, int y, int z)
        {
            return x >= 0 && x < Constants.WORLD_WIDTH &&
                   y >= 0 && y < Constants.WORLD_HEIGHT &&
                   z >= 0 && z < Constants.Z_LEVELS;
        }

        public TileType GetTile(int x, int y, int z)
        {
            if (!InBounds(x, y, z)) return TileType.Air;
            return grid[x, y, z];
        }

        public TileType GetTile(Vector3Int pos)
        {
            return GetTile(pos.x, pos.y, pos.z);
        }

        public void SetTile(int x, int y, int z, TileType type)
        {
            if (!InBounds(x, y, z)) return;
            if (grid[x, y, z] == type) return;

            grid[x, y, z] = type;
            OnTileChanged?.Invoke(x, y, z, type);
        }

        public void SetTile(Vector3Int pos, TileType type)
        {
            SetTile(pos.x, pos.y, pos.z, type);
        }

        public bool DigTile(int x, int y, int z)
        {
            if (!InBounds(x, y, z)) return false;

            TileType current = grid[x, y, z];
            var registry = TileRegistry.Instance;
            TileData data = registry.GetData(current);

            if (data == null || !data.isDiggable) return false;

            // Spawn dropped resource
            if (!string.IsNullOrEmpty(data.dropItemId) && data.dropItemCount > 0)
            {
                ItemData item = ItemDatabase.Instance?.GetItem(data.dropItemId);
                if (item != null)
                {
                    WorldItem.Spawn(item, data.dropItemCount, new Vector3Int(x, y, z));
                }
            }

            // Determine replacement tile
            TileType replacement = TileType.Air;
            if (current == TileType.Grass) replacement = TileType.Dirt;
            else if (data.isSolid && z > 0)
            {
                // In Dwarf Fortress, digging a stone wall leaves stone floor
                replacement = TileType.Floor_Stone;
            }

            SetTile(x, y, z, replacement);
            return true;
        }

        public bool IsWalkable(int x, int y, int z)
        {
            if (!InBounds(x, y, z)) return false;

            TileType t = grid[x, y, z];
            var registry = TileRegistry.Instance;

            // Solid obstacle
            if (registry.IsSolid(t)) return false;

            // Water is impassable without bridge/swimming
            if (t == TileType.Water) return false;

            // Stairs are directly walkable and connect vertically
            if (registry.ConnectsZUp(t) || registry.ConnectsZDown(t)) return true;

            // Built floor or ground tile
            if (registry.IsWalkable(t)) return true;

            // Air is only walkable if standing on solid floor below (Z-1)
            if (t == TileType.Air && z > 0)
            {
                TileType below = grid[x, y, z - 1];
                if (registry.IsSolid(below) || registry.IsWalkable(below))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsWalkable(Vector3Int pos)
        {
            return IsWalkable(pos.x, pos.y, pos.z);
        }
    }
}
