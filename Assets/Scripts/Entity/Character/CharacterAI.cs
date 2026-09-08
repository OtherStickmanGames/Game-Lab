using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.Entity.Base;
using DwarfClone.Jobs;
using DwarfClone.Crafting;
using DwarfClone.Inventory;
using DwarfClone.Medicine;

namespace DwarfClone.Entity.Character
{
    public enum AIState
    {
        Idle,
        MovingToTarget,
        PerformingJob,
        OperatingWorkbench,
        Combat,
        FirstAid,
        Eating,
        Incapacitated
    }

    public class CharacterAI : MonoBehaviour
    {
        public event Action<AIState> OnStateChanged;

        [Header("AI State")]
        [SerializeField] private AIState currentState = AIState.Idle;

        private DwarfCharacterController dwarf;
        private EntityMovement movement;
        private EntityAnimator animator;

        // Kenshi-style Job Priorities
        private readonly List<CraftingStation> assignedStations = new List<CraftingStation>();
        private Job currentDirectJob = null;
        private Job currentActiveJob = null;
        private float wanderTimer = 0f;

        public AIState CurrentState => currentState;
        public IReadOnlyList<CraftingStation> AssignedStations => assignedStations;
        public Job ActiveJob => currentActiveJob ?? currentDirectJob;

        private void Awake()
        {
            dwarf = GetComponent<DwarfCharacterController>();
            movement = GetComponent<EntityMovement>();
            animator = GetComponent<EntityAnimator>();
        }

        private void Start()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick += HandleTick;
            }
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick -= HandleTick;
            }
        }

        public void SetDirectJob(Job job)
        {
            currentDirectJob = job;
            currentActiveJob = null;
            if (job != null)
            {
                movement.MoveTo(job.targetPosition);
                SetState(AIState.MovingToTarget);
            }
        }

        public void AssignPermanentStation(CraftingStation station)
        {
            if (station == null) return;
            if (!assignedStations.Contains(station))
            {
                assignedStations.Add(station);
                station.AssignWorker(dwarf, true);
            }
        }

        public void RemovePermanentStation(CraftingStation station)
        {
            if (station != null)
            {
                assignedStations.Remove(station);
                station.ClearWorker();
            }
        }

        private void HandleTick()
        {
            if (dwarf == null || dwarf.IsDead) return;

            // Tick vitals
            dwarf.Needs.TickNeeds(Constants.BASE_TICK_RATE);
            dwarf.Health.TickMedicine(Constants.BASE_TICK_RATE);

            if (dwarf.Health.IsIncapacitated)
            {
                SetState(AIState.Incapacitated);
                dwarf.SetActionText("Unconscious");
                animator?.SetState(AnimState.Incapacitated);
                return;
            }

            // Priority 1: Self-Preservation / First Aid
            if (dwarf.Health.IsBleeding)
            {
                if (TrySelfBandage())
                {
                    SetState(AIState.FirstAid);
                    dwarf.SetActionText("Bandaging Wounds");
                    animator?.SetState(AnimState.Action);
                    return;
                }
            }

            // Priority 2: Direct Player Order
            if (currentDirectJob != null && !currentDirectJob.isComplete)
            {
                ExecuteJob(currentDirectJob);
                return;
            }

            // Priority 3: Hunger (Auto-eat if hungry)
            if (dwarf.Needs.IsHungry)
            {
                if (TryEatFromInventory())
                {
                    SetState(AIState.Eating);
                    dwarf.SetActionText("Eating Food");
                    return;
                }
            }

            // Priority 4: Shift-Assigned Permanent Workbenches (Kenshi style)
            if (assignedStations.Count > 0)
            {
                for (int i = 0; i < assignedStations.Count; i++)
                {
                    var station = assignedStations[i];
                    if (station != null && station.HasWork())
                    {
                        ExecuteWorkbenchJob(station);
                        return;
                    }
                }
            }

            // Priority 5: Global Designated Jobs (Mining, Chopping, Gathering, Building)
            if (currentActiveJob == null || currentActiveJob.isComplete)
            {
                Job job = JobSystem.Instance?.GetBestJobFor(dwarf);
                if (job != null)
                {
                    currentActiveJob = job;
                    movement.MoveTo(job.targetPosition);
                    SetState(AIState.MovingToTarget);
                    dwarf.SetActionText($"Going to {job.jobType}");
                    return;
                }
            }
            else
            {
                ExecuteJob(currentActiveJob);
                return;
            }

            // Priority 6: Idle Wander
            PerformIdle();
        }

        private void ExecuteJob(Job job)
        {
            float dist = Vector3.Distance(dwarf.GridPosition, job.targetPosition);

            if (dist > 1.5f)
            {
                if (!movement.IsMoving)
                {
                    movement.MoveTo(job.targetPosition);
                }
                SetState(AIState.MovingToTarget);
                dwarf.SetActionText($"Moving to {job.jobType}");
            }
            else
            {
                // In range: perform work
                SetState(AIState.PerformingJob);
                dwarf.SetActionText($"{job.jobType} in progress");
                animator?.SetState(AnimState.Action);

                float skillMult = dwarf.Skills.GetSkillMultiplier(job.requiredSkill);
                job.TickWork(Constants.BASE_TICK_RATE, skillMult);
                dwarf.Skills.AddXP(job.requiredSkill, 2f);

                if (job.isComplete)
                {
                    animator?.SetState(AnimState.Idle);
                    if (job == currentDirectJob) currentDirectJob = null;
                    if (job == currentActiveJob)
                    {
                        JobSystem.Instance?.RemoveJob(job);
                        currentActiveJob = null;
                    }
                }
            }
        }

        private void ExecuteWorkbenchJob(CraftingStation station)
        {
            float dist = Vector3.Distance(dwarf.GridPosition, station.GridPosition);

            if (dist > 1.5f)
            {
                if (!movement.IsMoving)
                {
                    movement.MoveTo(station.GridPosition);
                }
                SetState(AIState.MovingToTarget);
                dwarf.SetActionText($"Heading to {station.StationType}");
            }
            else
            {
                SetState(AIState.OperatingWorkbench);
                dwarf.SetActionText($"Operating {station.StationType}");
                animator?.SetState(AnimState.Action);

                var order = station.GetCurrentOrder();
                if (order != null)
                {
                    float skillMult = dwarf.Skills.GetSkillMultiplier(order.recipe.requiredSkill);
                    station.AdvanceCraft(Constants.BASE_TICK_RATE * skillMult, dwarf.Inventory);
                    dwarf.Skills.AddXP(order.recipe.requiredSkill, 3f);
                }
            }
        }

        private bool TrySelfBandage()
        {
            if (dwarf.Inventory.HasItem("item_bandage"))
            {
                dwarf.Inventory.RemoveItem(ItemDatabase.Instance.GetItem("item_bandage"), 1);
                dwarf.Health.ApplyFirstAid(ItemDatabase.Instance.GetItem("item_bandage"));
                return true;
            }
            return false;
        }

        private bool TryEatFromInventory()
        {
            var slots = dwarf.Inventory.Backpack;
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty && slots[i].item.IsEdible)
                {
                    ItemData food = slots[i].item;
                    dwarf.Inventory.RemoveItem(food, 1);
                    dwarf.Needs.TryEat(food);
                    return true;
                }
            }
            return false;
        }

        private void PerformIdle()
        {
            SetState(AIState.Idle);
            dwarf.SetActionText("Idle");
            animator?.SetState(AnimState.Idle);

            wanderTimer += Constants.BASE_TICK_RATE;
            if (wanderTimer >= UnityEngine.Random.Range(10f, 20f))
            {
                wanderTimer = 0f;
                int rx = dwarf.GridPosition.x + UnityEngine.Random.Range(-3, 4);
                int ry = dwarf.GridPosition.y + UnityEngine.Random.Range(-3, 4);
                Vector3Int wanderPos = new Vector3Int(rx, ry, dwarf.GridPosition.z);
                movement.MoveTo(wanderPos);
            }
        }

        private void SetState(AIState state)
        {
            if (currentState != state)
            {
                currentState = state;
                OnStateChanged?.Invoke(currentState);
            }
        }
    }
}
