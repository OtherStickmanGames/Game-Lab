using System;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.Inventory;
using DwarfClone.Medicine;

namespace DwarfClone.Entity.Character
{
    public class CharacterNeeds : MonoBehaviour
    {
        public event Action OnNeedsChanged;

        [Header("Needs")]
        [SerializeField] private float hunger = 100f; // 0 = starving, 100 = fully satiated
        [SerializeField] private float energy = 100f;

        private HealthSystem health;

        public float Hunger => hunger;
        public float Energy => energy;
        public bool IsHungry => hunger < 40f;
        public bool IsCriticallyHungry => hunger < 15f;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
        }

        public void TickNeeds(float dt)
        {
            if (health != null && health.IsDead) return;

            // Hunger depletion
            hunger = Mathf.Max(0f, hunger - Constants.HUNGER_DECREASE_PER_SEC * dt);

            // Starvation damage
            if (hunger <= 0f && health != null)
            {
                health.TakeDamage(Constants.STARVATION_DAMAGE_PER_SEC * dt, Combat.DamageType.Blunt, 0f, BodyPartType.Torso);
            }

            OnNeedsChanged?.Invoke();
        }

        public bool TryEat(ItemData food)
        {
            if (food == null || !food.IsEdible) return false;

            hunger = Mathf.Min(Constants.MAX_NEED_VALUE, hunger + food.nutrition);
            OnNeedsChanged?.Invoke();
            return true;
        }

        public float GetSpeedMultiplier()
        {
            float mult = 1.0f;
            if (health != null && health.IsCrippled) mult *= 0.38f;
            if (hunger < 15f) mult *= 0.65f;
            else if (hunger < 30f) mult *= 0.85f;
            return mult;
        }
    }
}
