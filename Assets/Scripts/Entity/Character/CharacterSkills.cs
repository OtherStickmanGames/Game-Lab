using System;
using System.Collections.Generic;
using UnityEngine;

namespace DwarfClone.Entity.Character
{
    [Serializable]
    public class SkillEntry
    {
        public SkillType skill;
        public int level = 1;
        public float currentXP = 0f;

        public float XPToNextLevel => level * 100f;

        public SkillEntry(SkillType skill, int initialLevel = 1)
        {
            this.skill = skill;
            this.level = initialLevel;
            this.currentXP = 0f;
        }

        public void AddXP(float amount)
        {
            currentXP += amount;
            while (currentXP >= XPToNextLevel)
            {
                currentXP -= XPToNextLevel;
                level++;
            }
        }
    }

    public class CharacterSkills : MonoBehaviour
    {
        public event Action OnSkillsChanged;

        [Header("Attributes")]
        public int strength = 10;
        public int dexterity = 10;
        public int toughness = 10;
        public int athletics = 10;

        private readonly Dictionary<SkillType, SkillEntry> skills = new Dictionary<SkillType, SkillEntry>();

        public IReadOnlyDictionary<SkillType, SkillEntry> AllSkills => skills;

        private void Awake()
        {
            InitializeSkills();
        }

        public void InitializeSkills()
        {
            foreach (SkillType st in Enum.GetValues(typeof(SkillType)))
            {
                skills[st] = new SkillEntry(st, 1);
            }
        }

        public int GetSkillLevel(SkillType type)
        {
            if (skills.TryGetValue(type, out var entry))
            {
                return entry.level;
            }
            return 1;
        }

        public void AddXP(SkillType type, float amount)
        {
            if (skills.TryGetValue(type, out var entry))
            {
                entry.AddXP(amount);
                OnSkillsChanged?.Invoke();
            }
        }

        public float GetSkillMultiplier(SkillType type)
        {
            int lvl = GetSkillLevel(type);
            return 1.0f + (lvl - 1) * 0.15f; // +15% per skill level
        }

        public void SetSkillLevel(SkillType type, int level)
        {
            if (skills.TryGetValue(type, out var entry))
            {
                entry.level = Mathf.Max(1, level);
                OnSkillsChanged?.Invoke();
            }
        }
    }
}
