using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.Entity.Base;
using DwarfClone.Entity.Character;
using DwarfClone.Combat;
using DwarfClone.Inventory;
using DwarfClone.Medicine;

namespace DwarfClone.Entity.Animal
{
    public class AnimalAI : EntityBase
    {
        public static readonly List<AnimalAI> AllAnimals = new List<AnimalAI>();

        [Header("Animal Identity")]
        [SerializeField] private AnimalType animalType = AnimalType.Wolf;
        [SerializeField] private bool isAggressive = true;

        [Header("Stats")]
        [SerializeField] private float maxHP = 80f;
        [SerializeField] private float currentHP = 80f;
        [SerializeField] private float attackDamage = 14f;
        [SerializeField] private DamageType damageType = DamageType.Cut;
        [SerializeField] private float attackCooldown = 1.8f;
        [SerializeField] private float aggroRadius = 8.0f;

        private EntityMovement movement;
        private EntityAnimator animator;
        private DwarfCharacterController currentTarget = null;
        private float attackTimer = 0f;
        private float wanderTimer = 0f;
        private bool isDead = false;

        public AnimalType AnimalType => animalType;
        public bool IsAggressive => isAggressive;
        public override bool IsDead => isDead;
        public float CurrentHP => currentHP;
        public float MaxHP => maxHP;

        protected override void Awake()
        {
            base.Awake();
            movement = GetComponent<EntityMovement>() ?? gameObject.AddComponent<EntityMovement>();
            animator = GetComponent<EntityAnimator>() ?? gameObject.AddComponent<EntityAnimator>();
        }

        protected override void Start()
        {
            base.Start();
            AllAnimals.Add(this);
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick += HandleTick;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AllAnimals.Remove(this);
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick -= HandleTick;
            }
        }

        public void Initialize(AnimalType type, Vector3Int startPos)
        {
            this.animalType = type;
            SetGridPosition(startPos);

            switch (type)
            {
                case AnimalType.Wolf:
                    isAggressive = true;
                    maxHP = 75f;
                    attackDamage = 15f;
                    damageType = DamageType.Cut;
                    aggroRadius = 9f;
                    animator.InitializeSprites("Animals", "animal_wolf");
                    break;
                case AnimalType.Bear:
                    isAggressive = true;
                    maxHP = 180f;
                    attackDamage = 28f;
                    damageType = DamageType.Blunt;
                    aggroRadius = 7f;
                    animator.InitializeSprites("Animals", "animal_bear");
                    break;
                case AnimalType.BloodSpider:
                    isAggressive = true;
                    maxHP = 60f;
                    attackDamage = 18f;
                    damageType = DamageType.Pierce;
                    aggroRadius = 11f;
                    animator.InitializeSprites("Animals", "animal_spider");
                    break;
                case AnimalType.Deer:
                    isAggressive = false;
                    maxHP = 50f;
                    attackDamage = 0f;
                    animator.InitializeSprites("Animals", "animal_deer");
                    break;
                case AnimalType.Rabbit:
                    isAggressive = false;
                    maxHP = 20f;
                    attackDamage = 0f;
                    animator.InitializeSprites("Animals", "animal_rabbit");
                    break;
            }
            currentHP = maxHP;
        }

        private void Update()
        {
            if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        }

        private void HandleTick()
        {
            if (isDead) return;

            if (isAggressive)
            {
                ProcessAggressiveBehavior();
            }
            else
            {
                ProcessPassiveBehavior();
            }
        }

        private void ProcessAggressiveBehavior()
        {
            // Find target dwarf if none
            if (currentTarget == null || currentTarget.IsDead || currentTarget.Health.IsIncapacitated)
            {
                currentTarget = FindNearestDwarf(aggroRadius);
            }

            if (currentTarget != null)
            {
                float dist = Vector3.Distance(gridPosition, currentTarget.GridPosition);

                if (dist <= 1.5f)
                {
                    // In attack range
                    if (attackTimer <= 0f)
                    {
                        attackTimer = attackCooldown;
                        animator.SetState(AnimState.Action);
                        currentTarget.Health.TakeDamage(attackDamage, damageType, currentTarget.Inventory.EquippedArmor?.armorRating ?? 0f);
                        Debug.Log($"[AnimalAI] {animalType} attacked {currentTarget.CharacterName} for {attackDamage} {damageType} damage!");

                        // Retaliation: target dwarf strikes back if conscious
                        if (!currentTarget.IsDead && !currentTarget.Health.IsIncapacitated)
                        {
                            float dwarfDmg = currentTarget.Inventory.EquippedWeapon?.damage ?? 10f;
                            TakeDamage(dwarfDmg);
                        }
                    }
                }
                else
                {
                    // Chase target
                    if (!movement.IsMoving)
                    {
                        movement.MoveTo(currentTarget.GridPosition);
                    }
                }
            }
            else
            {
                // Wander
                PerformWander();
            }
        }

        private void ProcessPassiveBehavior()
        {
            // Flee if a dwarf gets too close
            var nearbyDwarf = FindNearestDwarf(4.0f);
            if (nearbyDwarf != null)
            {
                Vector3Int fleeDir = gridPosition - nearbyDwarf.GridPosition;
                fleeDir.Clamp(new Vector3Int(-1, -1, 0), new Vector3Int(1, 1, 0));
                movement.MoveTo(gridPosition + fleeDir * 4);
                return;
            }

            PerformWander();
        }

        private void PerformWander()
        {
            wanderTimer += Constants.BASE_TICK_RATE;
            if (wanderTimer >= UnityEngine.Random.Range(8f, 15f))
            {
                wanderTimer = 0f;
                int rx = gridPosition.x + UnityEngine.Random.Range(-3, 4);
                int ry = gridPosition.y + UnityEngine.Random.Range(-3, 4);
                movement.MoveTo(new Vector3Int(rx, ry, gridPosition.z));
            }
        }

        public void TakeDamage(float amount)
        {
            if (isDead) return;
            currentHP = Mathf.Max(0f, currentHP - amount);
            Debug.Log($"[AnimalAI] {animalType} took {amount} damage, HP: {currentHP}/{maxHP}");

            if (currentHP <= 0f)
            {
                Die();
            }
        }

        public void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log($"[AnimalAI] {animalType} was killed!");

            // Drop meat & hides
            var db = ItemDatabase.Instance;
            if (db != null)
            {
                int meatCount = animalType == AnimalType.Bear ? 5 : (animalType == AnimalType.Rabbit ? 1 : 3);
                WorldItem.Spawn(db.GetItem("item_meat_raw"), meatCount, gridPosition);
                WorldItem.Spawn(db.GetItem("item_hide_raw"), animalType == AnimalType.Bear ? 3 : 1, gridPosition);
            }

            Destroy(gameObject);
        }

        private DwarfCharacterController FindNearestDwarf(float radius)
        {
            DwarfCharacterController nearest = null;
            float minDist = radius;

            var squad = DwarfCharacterController.Squad;
            for (int i = 0; i < squad.Count; i++)
            {
                var d = squad[i];
                if (d == null || d.IsDead || d.CurrentZ != gridPosition.z) continue;

                float dist = Vector3.Distance(gridPosition, d.GridPosition);
                if (dist <= minDist)
                {
                    minDist = dist;
                    nearest = d;
                }
            }

            return nearest;
        }
    }
}
