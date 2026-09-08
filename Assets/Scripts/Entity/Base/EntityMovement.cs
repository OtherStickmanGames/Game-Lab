using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.Pathfinding;
using DwarfClone.Entity.Character;

namespace DwarfClone.Entity.Base
{
    public class EntityMovement : MonoBehaviour
    {
        public event Action<Vector3Int> OnStepTaken;
        public event Action OnDestinationReached;

        [Header("Movement Settings")]
        [SerializeField] private float baseSpeed = 4.0f;

        private EntityBase entity;
        private SpriteRenderer spriteRenderer;
        private CharacterNeeds needs;
        private List<Vector3Int> currentPath = new List<Vector3Int>();
        private int currentPathIndex = 0;
        private Vector3 currentTargetWorld;
        private bool isMoving = false;

        public bool IsMoving => isMoving;
        public float BaseSpeed => baseSpeed;

        private void Awake()
        {
            entity = GetComponent<EntityBase>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            needs = GetComponent<CharacterNeeds>();
        }

        private void Update()
        {
            if (!isMoving || currentPath == null || currentPathIndex >= currentPath.Count) return;

            float speedMult = needs != null ? needs.GetSpeedMultiplier() : 1.0f;
            float step = baseSpeed * speedMult * Time.deltaTime;

            Vector3 nextPos = Vector3.MoveTowards(transform.position, currentTargetWorld, step);

            // Sprite facing direction
            if (currentTargetWorld.x < transform.position.x - 0.05f)
            {
                if (spriteRenderer != null) spriteRenderer.flipX = true;
            }
            else if (currentTargetWorld.x > transform.position.x + 0.05f)
            {
                if (spriteRenderer != null) spriteRenderer.flipX = false;
            }

            transform.position = nextPos;

            // Check arrival at current waypoint
            if (Vector3.Distance(transform.position, currentTargetWorld) < 0.05f)
            {
                Vector3Int stepPos = currentPath[currentPathIndex];
                if (entity != null)
                {
                    entity.SetGridPosition(stepPos);
                }
                OnStepTaken?.Invoke(stepPos);

                currentPathIndex++;
                if (currentPathIndex < currentPath.Count)
                {
                    Vector3Int nextWaypoint = currentPath[currentPathIndex];
                    currentTargetWorld = new Vector3(nextWaypoint.x + 0.5f, nextWaypoint.y + 0.5f, 0f);
                }
                else
                {
                    // Path finished
                    isMoving = false;
                    currentPath.Clear();
                    OnDestinationReached?.Invoke();
                }
            }
        }

        public void MoveTo(Vector3Int targetPos)
        {
            if (entity == null) entity = GetComponent<EntityBase>();
            if (entity == null) return;

            var pathfinder = AStarPathfinder.Instance;
            if (pathfinder == null) return;

            var path = pathfinder.FindPathSync(entity.GridPosition, targetPos, out bool success);
            if (success && path.Count > 0)
            {
                SetPath(path);
            }
        }

        public void SetPath(List<Vector3Int> path)
        {
            if (path == null || path.Count == 0)
            {
                Stop();
                return;
            }

            currentPath = new List<Vector3Int>(path);
            currentPathIndex = 0;
            currentTargetWorld = new Vector3(currentPath[0].x + 0.5f, currentPath[0].y + 0.5f, 0f);
            isMoving = true;
        }

        public void Stop()
        {
            isMoving = false;
            currentPath.Clear();
            currentPathIndex = 0;
        }
    }
}
