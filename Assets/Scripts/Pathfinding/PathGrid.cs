using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.World.Chunk;
using DwarfClone.World.Tile;

namespace DwarfClone.Pathfinding
{
    public class PathGrid : MonoBehaviour
    {
        public static PathGrid Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool IsWalkable(int x, int y, int z)
        {
            var grid = WorldGrid.Instance;
            if (grid == null) return false;
            return grid.IsWalkable(x, y, z);
        }

        public bool IsWalkable(Vector3Int pos)
        {
            return IsWalkable(pos.x, pos.y, pos.z);
        }

        public List<Vector3Int> GetNeighbors(Vector3Int current)
        {
            var neighbors = new List<Vector3Int>();
            var grid = WorldGrid.Instance;
            var reg = TileRegistry.Instance;
            if (grid == null || reg == null) return neighbors;

            // 1. Horizontal movement (8 directions on same Z-level)
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = current.x + dx;
                    int ny = current.y + dy;
                    int nz = current.z;

                    if (!grid.InBounds(nx, ny, nz)) continue;

                    // Prevent diagonal cutting through solid corners
                    if (dx != 0 && dy != 0)
                    {
                        if (!grid.IsWalkable(current.x + dx, current.y, nz) ||
                            !grid.IsWalkable(current.x, current.y + dy, nz))
                        {
                            continue;
                        }
                    }

                    if (grid.IsWalkable(nx, ny, nz))
                    {
                        neighbors.Add(new Vector3Int(nx, ny, nz));
                    }
                }
            }

            // 2. Vertical Z-level stairs transitions
            TileType curTile = grid.GetTile(current.x, current.y, current.z);

            // Going UP via stairs
            if (reg.ConnectsZUp(curTile) && current.z < Constants.Z_LEVELS - 1)
            {
                Vector3Int upPos = new Vector3Int(current.x, current.y, current.z + 1);
                TileType upTile = grid.GetTile(upPos);
                if (grid.IsWalkable(upPos) || reg.ConnectsZDown(upTile) || upTile == TileType.Air)
                {
                    neighbors.Add(upPos);
                }
            }

            // Going DOWN via stairs
            if (reg.ConnectsZDown(curTile) && current.z > 0)
            {
                Vector3Int downPos = new Vector3Int(current.x, current.y, current.z - 1);
                TileType downTile = grid.GetTile(downPos);
                if (grid.IsWalkable(downPos) || reg.ConnectsZUp(downTile) || downTile == TileType.Floor_Stone || downTile == TileType.Dirt)
                {
                    neighbors.Add(downPos);
                }
            }

            return neighbors;
        }
    }
}
