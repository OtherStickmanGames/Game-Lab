using System;
using System.Collections.Generic;
using UnityEngine;

namespace DwarfClone.Pathfinding
{
    public struct PathRequest
    {
        public Vector3Int start;
        public Vector3Int target;
        public Action<List<Vector3Int>, bool> callback;

        public PathRequest(Vector3Int start, Vector3Int target, Action<List<Vector3Int>, bool> callback)
        {
            this.start = start;
            this.target = target;
            this.callback = callback;
        }
    }
}
