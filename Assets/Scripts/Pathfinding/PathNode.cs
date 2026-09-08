using UnityEngine;

namespace DwarfClone.Pathfinding
{
    public class PathNode
    {
        public Vector3Int position;
        public bool isWalkable;
        public int gCost;
        public int hCost;
        public PathNode parent;

        public int fCost => gCost + hCost;

        public PathNode(Vector3Int pos, bool walkable)
        {
            this.position = pos;
            this.isWalkable = walkable;
        }
    }
}
