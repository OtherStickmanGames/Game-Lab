using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.Combat;
using DwarfClone.Inventory;

namespace DwarfClone.Medicine
{
    public class HealthSystem : MonoBehaviour
    {
        public event Action OnHealthChanged;
        public event Action OnBleedingStatusChanged;
        public event Action OnIncapacitated;
        public event Action OnDeath;

        private readonly Dictionary<BodyPartType, BodyPart> bodyParts = new Dictionary<BodyPartType, BodyPart>();

        [Header("Vitals")]
        [SerializeField] private float maxBlood = 100f;
        [SerializeField] private float currentBlood = 100f;
        [SerializeField] private bool isDead = false;
        [SerializeField] private bool isIncapacitated = false;

        public float Blood => currentBlood;
        public float MaxBlood => maxBlood;
        public bool IsDead => isDead;
        public bool IsIncapacitated => isIncapacitated;

        public float TotalBleedingRate
        {
            get
            {
                float total = 0f;
                foreach (var part in bodyParts.Values)
                {
                    total += part.bleedingRate;
                }
                return total;
            }
        }

        public bool IsBleeding => TotalBleedingRate > 0.01f;

        public bool IsCrippled
        {
            get
            {
                if (bodyParts.TryGetValue(BodyPartType.LeftLeg, out var ll) && ll.IsCrippled) return true;
                if (bodyParts.TryGetValue(BodyPartType.RightLeg, out var rl) && rl.IsCrippled) return true;
                return false;
            }
        }

        private void Awake()
        {
            InitializeBodyParts();
        }

        public void InitializeBodyParts()
        {
            bodyParts.Clear();
            bodyParts[BodyPartType.Head] = new BodyPart(BodyPartType.Head, 100f);
            bodyParts[BodyPartType.Torso] = new BodyPart(BodyPartType.Torso, 100f);
            bodyParts[BodyPartType.LeftArm] = new BodyPart(BodyPartType.LeftArm, 100f);
            bodyParts[BodyPartType.RightArm] = new BodyPart(BodyPartType.RightArm, 100f);
            bodyParts[BodyPartType.LeftLeg] = new BodyPart(BodyPartType.LeftLeg, 100f);
            bodyParts[BodyPartType.RightLeg] = new BodyPart(BodyPartType.RightLeg, 100f);
            currentBlood = maxBlood;
            isDead = false;
            isIncapacitated = false;
        }

        public BodyPart GetPart(BodyPartType type)
        {
            bodyParts.TryGetValue(type, out var part);
            return part;
        }

        public IReadOnlyDictionary<BodyPartType, BodyPart> AllParts => bodyParts;

        public void TakeDamage(float rawDamage, DamageType damageType, float armorRating = 0f, BodyPartType? specificPart = null)
        {
            if (isDead) return;

            // Armor mitigation: armor rating reduces damage by percentage
            float effectiveDamage = Mathf.Max(1f, rawDamage * (1f - Mathf.Clamp01(armorRating / 100f)));

            // Random hit location if not specific
            BodyPartType target = specificPart ?? RollHitLocation();
            if (!bodyParts.TryGetValue(target, out var part)) return;

            bool causesBleeding = damageType == DamageType.Cut || damageType == DamageType.Pierce;
            part.ApplyDamage(effectiveDamage, causesBleeding);

            // Check incapacitation: Head or Torso dropped to 0
            if (part.partType == BodyPartType.Head || part.partType == BodyPartType.Torso)
            {
                if (part.currentHP <= 0f && !isIncapacitated)
                {
                    SetIncapacitated(true);
                }
            }

            OnHealthChanged?.Invoke();
            if (causesBleeding) OnBleedingStatusChanged?.Invoke();
        }

        public bool ApplyFirstAid(ItemData medicalItem)
        {
            if (isDead) return false;

            bool treatedAny = false;
            float healAmount = medicalItem != null ? medicalItem.healingValue : 25f;

            // Priority: bandage bleeding parts first
            foreach (var part in bodyParts.Values)
            {
                if (part.bleedingRate > 0f)
                {
                    part.Bandage();
                    part.Heal(healAmount * 0.5f);
                    treatedAny = true;
                }
            }

            // If no bleeding, heal the most damaged part
            if (!treatedAny)
            {
                BodyPart worstPart = null;
                float lowestHP = 999f;
                foreach (var part in bodyParts.Values)
                {
                    if (part.currentHP < part.maxHP && part.currentHP < lowestHP)
                    {
                        lowestHP = part.currentHP;
                        worstPart = part;
                    }
                }

                if (worstPart != null)
                {
                    worstPart.Heal(healAmount);
                    treatedAny = true;
                }
            }

            // Check recovery from unconsciousness
            if (isIncapacitated && bodyParts[BodyPartType.Head].currentHP > 15f && bodyParts[BodyPartType.Torso].currentHP > 15f && currentBlood > 30f)
            {
                SetIncapacitated(false);
            }

            if (treatedAny)
            {
                OnHealthChanged?.Invoke();
                OnBleedingStatusChanged?.Invoke();
            }

            return treatedAny;
        }

        public void TickMedicine(float dt)
        {
            if (isDead) return;

            // Bleeding ticks
            float bleed = TotalBleedingRate;
            if (bleed > 0f)
            {
                currentBlood = Mathf.Max(0f, currentBlood - bleed * dt);
                if (currentBlood <= 0f)
                {
                    Die("Bled out to death");
                    return;
                }
                else if (currentBlood < 25f && !isIncapacitated)
                {
                    SetIncapacitated(true);
                }
            }
            else if (currentBlood < maxBlood && !isIncapacitated)
            {
                // Natural blood recovery
                currentBlood = Mathf.Min(maxBlood, currentBlood + 0.3f * dt);
            }

            // Natural regeneration of treated parts
            foreach (var part in bodyParts.Values)
            {
                if (part.isBandaged && part.currentHP < part.maxHP)
                {
                    part.Heal(Constants.NATURAL_REGEN_PER_SEC * dt);
                }
            }

            // Recovery check
            if (isIncapacitated && bodyParts[BodyPartType.Head].currentHP > 20f && bodyParts[BodyPartType.Torso].currentHP > 20f && currentBlood > 35f)
            {
                SetIncapacitated(false);
            }
        }

        private void SetIncapacitated(bool val)
        {
            if (isIncapacitated != val)
            {
                isIncapacitated = val;
                if (isIncapacitated) OnIncapacitated?.Invoke();
            }
        }

        public void Die(string cause)
        {
            if (isDead) return;
            isDead = true;
            isIncapacitated = true;
            Debug.Log($"[HealthSystem] Entity died: {cause}");
            OnDeath?.Invoke();
        }

        private BodyPartType RollHitLocation()
        {
            float roll = UnityEngine.Random.value;
            if (roll < 0.15f) return BodyPartType.Head;
            if (roll < 0.50f) return BodyPartType.Torso;
            if (roll < 0.65f) return BodyPartType.LeftArm;
            if (roll < 0.80f) return BodyPartType.RightArm;
            if (roll < 0.90f) return BodyPartType.LeftLeg;
            return BodyPartType.RightLeg;
        }
    }
}
