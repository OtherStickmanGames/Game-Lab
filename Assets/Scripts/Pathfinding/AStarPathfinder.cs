using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;

namespace DwarfClone.Pathfinding
{
    public class AStarPathfinder : MonoBehaviour
    {
        public static AStarPathfinder Instance { get; private set; }

        private readonly Queue<PathRequest> requestQueue = new Queue<PathRequest>();
        private bool isProcessing = false;
        private PathGrid grid;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            grid = GetComponent<PathGrid>();
            if (grid == null)
            {
                grid = gameObject.AddComponent<PathGrid>();
            }
        }

        public void RequestPath(Vector3Int start, Vector3Int target, Action<List<Vector3Int>, bool> callback)
        {
            if (callback == null) return;
            requestQueue.Enqueue(new PathRequest(start, target, callback));
            if (!isProcessing)
            {
                StartCoroutine(ProcessQueueCoroutine());
            }
        }

        private IEnumerator ProcessQueueCoroutine()
        {
            isProcessing = true;

            while (requestQueue.Count > 0)
            {
                PathRequest currentRequest = requestQueue.Dequeue();
                List<Vector3Int> path = FindPathSync(currentRequest.start, currentRequest.target, out bool success);
                currentRequest.callback?.Invoke(path, success);

                yield return null;
            }

            isProcessing = false;
        }

        public List<Vector3Int> FindPathSync(Vector3Int start, Vector3Int target, out bool success)
        {
            success = false;
            if (grid == null) grid = PathGrid.Instance;
            if (grid == null) return new List<Vector3Int>();

            // If target is solid (e.g. mining wall, chopping tree), find best adjacent walkable neighbor
            Vector3Int actualTarget = target;
            if (!grid.IsWalkable(target))
            {
                var targetNeighbors = grid.GetNeighbors(target);
                if (targetNeighbors.Count > 0)
                {
                    actualTarget = targetNeighbors[0];
                    float minDist = Vector3.Distance(start, actualTarget);
                    for (int i = 1; i < targetNeighbors.Count; i++)
                    {
                        float d = Vector3.Distance(start, targetNeighbors[i]);
                        if (d < minDist)
                        {
                            minDist = d;
                            actualTarget = targetNeighbors[i];
                        }
                    }
                }
                else
                {
                    return new List<Vector3Int>();
                }
            }

            if (start == actualTarget)
            {
                success = true;
                return new List<Vector3Int> { actualTarget };
            }

            var openSet = new List<PathNode>();
            var openLookup = new Dictionary<Vector3Int, PathNode>();
            var closedLookup = new HashSet<Vector3Int>();

            PathNode startNode = new PathNode(start, true)
            {
                gCost = 0,
                hCost = GetDistance(start, actualTarget)
            };
            openSet.Add(startNode);
            openLookup[start] = startNode;

            int iterations = 0;

            while (openSet.Count > 0 && iterations < Constants.MAX_PATH_ITERATIONS)
            {
                iterations++;

                PathNode current = openSet[0];
                int bestIndex = 0;
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < current.fCost || (openSet[i].fCost == current.fCost && openSet[i].hCost < current.hCost))
                    {
                        current = openSet[i];
                        bestIndex = i;
                    }
                }

                openSet.RemoveAt(bestIndex);
                openLookup.Remove(current.position);
                closedLookup.Add(current.position);

                if (current.position == actualTarget)
                {
                    success = true;
                    return RetracePath(startNode, current);
                }

                List<Vector3Int> neighbors = grid.GetNeighbors(current.position);
                for (int n = 0; n < neighbors.Count; n++)
                {
                    Vector3Int nPos = neighbors[n];
                    if (closedLookup.Contains(nPos)) continue;

                    int moveCost = GetStepCost(current.position, nPos);
                    int tentativeGCost = current.gCost + moveCost;

                    if (!openLookup.TryGetValue(nPos, out var neighborNode))
                    {
                        neighborNode = new PathNode(nPos, true)
                        {
                            gCost = tentativeGCost,
                            hCost = GetDistance(nPos, actualTarget),
                            parent = current
                        };

                        openSet.Add(neighborNode);
                        openLookup[nPos] = neighborNode;
                    }
                    else if (tentativeGCost < neighborNode.gCost)
                    {
                        neighborNode.gCost = tentativeGCost;
                        neighborNode.parent = current;
                    }
                }
            }

            return new List<Vector3Int>();
        }

        private List<Vector3Int> RetracePath(PathNode startNode, PathNode endNode)
        {
            var path = new List<Vector3Int>();
            PathNode current = endNode;

            while (current != null && current.position != startNode.position)
            {
                path.Add(current.position);
                current = current.parent;
            }

            path.Reverse();
            return path;
        }

        private int GetStepCost(Vector3Int from, Vector3Int to)
        {
            int dz = Mathf.Abs(from.z - to.z);
            if (dz > 0) return 20 * dz;

            int dx = Mathf.Abs(from.x - to.x);
            int dy = Mathf.Abs(from.y - to.y);

            if (dx > 0 && dy > 0) return 14;
            return 10;
        }

        private int GetDistance(Vector3Int a, Vector3Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            int dz = Mathf.Abs(a.z - b.z);

            int diag = Mathf.Min(dx, dy);
            int straight = Mathf.Max(dx, dy) - diag;

            return (diag * 14) + (straight * 10) + (dz * 25);
        }
    }
}
