using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Entity.Character;

namespace DwarfClone.Jobs
{
    public class JobSystem : MonoBehaviour
    {
        public static JobSystem Instance { get; private set; }

        public event Action OnJobsUpdated;

        private readonly List<Job> availableJobs = new List<Job>();

        public IReadOnlyList<Job> AvailableJobs => availableJobs;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddJob(Job job)
        {
            if (job == null) return;
            availableJobs.Add(job);
            OnJobsUpdated?.Invoke();
        }

        public void RemoveJob(Job job)
        {
            if (job != null && availableJobs.Remove(job))
            {
                OnJobsUpdated?.Invoke();
            }
        }

        public void CancelJobsAt(Vector3Int pos)
        {
            availableJobs.RemoveAll(j => j.targetPosition == pos);
            OnJobsUpdated?.Invoke();
        }

        public bool HasJobAt(Vector3Int pos)
        {
            for (int i = 0; i < availableJobs.Count; i++)
            {
                if (availableJobs[i].targetPosition == pos) return true;
            }
            return false;
        }

        public Job GetBestJobFor(DwarfCharacterController dwarf)
        {
            if (dwarf == null || availableJobs.Count == 0) return null;

            Job bestJob = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < availableJobs.Count; i++)
            {
                var job = availableJobs[i];
                if (job.isComplete) continue;

                if (job.CanExecute(dwarf))
                {
                    float dist = Vector3.Distance(dwarf.GridPosition, job.targetPosition);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestJob = job;
                    }
                }
            }

            return bestJob;
        }
    }
}
