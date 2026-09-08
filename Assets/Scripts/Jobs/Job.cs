using System;
using UnityEngine;
using DwarfClone.Inventory;
using DwarfClone.Entity.Character;

namespace DwarfClone.Jobs
{
    [Serializable]
    public class Job
    {
        public JobType jobType;
        public Vector3Int targetPosition;
        public ToolType requiredTool = ToolType.None;
        public SkillType requiredSkill = SkillType.Mining;
        public float workDuration = 3.0f;
        public float currentProgress = 0f;
        public bool isComplete = false;
        public Action onComplete;

        public Job(
            JobType jobType,
            Vector3Int targetPos,
            ToolType tool = ToolType.None,
            SkillType skill = SkillType.Mining,
            float duration = 3.0f,
            Action onComplete = null)
        {
            this.jobType = jobType;
            this.targetPosition = targetPos;
            this.requiredTool = tool;
            this.requiredSkill = skill;
            this.workDuration = duration;
            this.currentProgress = 0f;
            this.isComplete = false;
            this.onComplete = onComplete;
        }

        public virtual bool CanExecute(DwarfCharacterController dwarf)
        {
            if (dwarf == null || dwarf.IsDead || dwarf.Health.IsIncapacitated) return false;

            // Check tool requirement
            if (requiredTool != ToolType.None)
            {
                if (dwarf.Inventory == null || !dwarf.Inventory.HasTool(requiredTool))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual void TickWork(float dt, float skillMultiplier)
        {
            if (isComplete) return;

            currentProgress += dt * skillMultiplier;
            if (currentProgress >= workDuration)
            {
                isComplete = true;
                onComplete?.Invoke();
            }
        }
    }
}
