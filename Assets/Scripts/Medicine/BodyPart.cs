using System;
using UnityEngine;

namespace DwarfClone.Medicine
{
    [Serializable]
    public class BodyPart
    {
        public BodyPartType partType;
        public float maxHP = 100f;
        public float currentHP = 100f;
        public float bleedingRate = 0f; // Blood loss per second
        public bool isBandaged = false;

        public bool IsCrippled => currentHP <= 0f;
        public float HealthNormalized => Mathf.Clamp01(currentHP / maxHP);

        public BodyPart(BodyPartType type, float maxHP = 100f)
        {
            this.partType = type;
            this.maxHP = maxHP;
            this.currentHP = maxHP;
            this.bleedingRate = 0f;
            this.isBandaged = false;
        }

        public void ApplyDamage(float amount, bool causesBleeding)
        {
            currentHP = Mathf.Max(0f, currentHP - amount);
            if (causesBleeding && amount > 5f)
            {
                bleedingRate += amount * 0.04f;
                isBandaged = false;
            }
        }

        public void Bandage()
        {
            bleedingRate = 0f;
            isBandaged = true;
        }

        public void Heal(float amount)
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            if (currentHP >= maxHP)
            {
                bleedingRate = 0f;
                isBandaged = false;
            }
        }
    }
}
