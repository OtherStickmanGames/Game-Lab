using System;
using System.Collections.Generic;
using UnityEngine;
using DwarfClone.Entity.Base;
using DwarfClone.Inventory;
using DwarfClone.Medicine;

namespace DwarfClone.Entity.Character
{
    public class DwarfCharacterController : EntityBase
    {
        public static readonly List<DwarfCharacterController> Squad = new List<DwarfCharacterController>();

        [Header("Character Identity")]
        [SerializeField] private string characterName = "Dwarf";
        [SerializeField] private string profession = "Worker";
        [SerializeField] private string archetype = "miner";

        [Header("State")]
        [SerializeField] private string currentActionText = "Idle";
        [SerializeField] private bool isSelected = false;

        private HealthSystem health;
        private CharacterNeeds needs;
        private CharacterSkills skills;
        private InventorySystem inventory;
        private EntityMovement movement;
        private EntityAnimator animator;
        private CharacterAI ai;

        public string CharacterName => characterName;
        public string Profession => profession;
        public string Archetype => archetype;
        public string CurrentActionText => currentActionText;
        public bool IsSelected => isSelected;

        public HealthSystem Health => health;
        public CharacterNeeds Needs => needs;
        public CharacterSkills Skills => skills;
        public InventorySystem Inventory => inventory;
        public EntityMovement Movement => movement;
        public EntityAnimator Animator => animator;
        public CharacterAI AI => ai;

        public override bool IsDead => health != null && health.IsDead;

        protected override void Awake()
        {
            base.Awake();

            health = GetComponent<HealthSystem>() ?? gameObject.AddComponent<HealthSystem>();
            needs = GetComponent<CharacterNeeds>() ?? gameObject.AddComponent<CharacterNeeds>();
            skills = GetComponent<CharacterSkills>() ?? gameObject.AddComponent<CharacterSkills>();
            inventory = GetComponent<InventorySystem>() ?? gameObject.AddComponent<InventorySystem>();
            movement = GetComponent<EntityMovement>() ?? gameObject.AddComponent<EntityMovement>();
            animator = GetComponent<EntityAnimator>() ?? gameObject.AddComponent<EntityAnimator>();
            ai = GetComponent<CharacterAI>() ?? gameObject.AddComponent<CharacterAI>();
        }

        protected override void Start()
        {
            base.Start();
            Squad.Add(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Squad.Remove(this);
        }

        public void Initialize(string name, string profession, string archetype, Vector3Int startPos)
        {
            this.characterName = name;
            this.profession = profession;
            this.archetype = archetype;

            SetGridPosition(startPos);
            animator.InitializeSprites("Characters", $"char_{archetype}");

            // Initial skills and equipment based on archetype
            SetupArchetypeLoadout();
        }

        private void SetupArchetypeLoadout()
        {
            var db = ItemDatabase.Instance;
            if (db == null) return;

            switch (archetype)
            {
                case "warrior":
                    skills.SetSkillLevel(SkillType.MeleeCombat, 5);
                    skills.strength = 16;
                    skills.toughness = 15;
                    inventory.Equip(db.GetItem("item_sword_iron"));
                    inventory.Equip(db.GetItem("item_helmet_iron"));
                    inventory.Equip(db.GetItem("item_armor_plate"));
                    inventory.AddItem(db.GetItem("item_bandage"), 2);
                    inventory.AddItem(db.GetItem("item_bread"), 2);
                    break;
                case "miner":
                    skills.SetSkillLevel(SkillType.Mining, 5);
                    skills.strength = 14;
                    inventory.Equip(db.GetItem("item_pickaxe_miner"));
                    inventory.AddItem(db.GetItem("item_bread"), 3);
                    break;
                case "builder":
                    skills.SetSkillLevel(SkillType.Construction, 5);
                    skills.SetSkillLevel(SkillType.Carpentry, 4);
                    inventory.Equip(db.GetItem("item_hammer"));
                    inventory.Equip(db.GetItem("item_backpack"));
                    inventory.AddItem(db.GetItem("item_bread"), 3);
                    break;
                case "medic":
                    skills.SetSkillLevel(SkillType.Medicine, 5);
                    skills.dexterity = 15;
                    inventory.AddItem(db.GetItem("item_bandage"), 5);
                    inventory.AddItem(db.GetItem("item_healing_salve"), 3);
                    inventory.AddItem(db.GetItem("item_bread"), 2);
                    break;
                case "smith":
                    skills.SetSkillLevel(SkillType.Smithing, 5);
                    skills.SetSkillLevel(SkillType.Smelting, 4);
                    inventory.Equip(db.GetItem("item_axe_woodcutter"));
                    inventory.AddItem(db.GetItem("item_bread"), 3);
                    break;
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }

        public void SetActionText(string text)
        {
            currentActionText = text;
        }
    }
}
