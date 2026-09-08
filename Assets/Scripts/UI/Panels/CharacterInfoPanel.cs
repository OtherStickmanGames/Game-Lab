using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DwarfClone.Core;
using DwarfClone.Entity.Character;
using DwarfClone.Entity.Selection;
using DwarfClone.Medicine;
using DwarfClone.Inventory;
using DwarfClone.UI.Elements;

namespace DwarfClone.UI.Panels
{
    public class CharacterInfoPanel : MonoBehaviour
    {
        public enum Tab { Anatomy, Skills, Inventory, Jobs }

        [Header("Root")]
        public GameObject panelRoot;
        public Text headerText;
        public Text statusText;

        [Header("Tabs")]
        private Tab currentTab = Tab.Anatomy;
        private GameObject tabContentAnatomy;
        private GameObject tabContentSkills;
        private GameObject tabContentInventory;
        private GameObject tabContentJobs;

        // Anatomy UI
        private readonly Dictionary<BodyPartType, Image> partFills = new Dictionary<BodyPartType, Image>();
        private readonly Dictionary<BodyPartType, Text> partTexts = new Dictionary<BodyPartType, Text>();
        private Text bloodText;
        private Button bandageButton;

        // Skills UI
        private Text skillsListText;

        // Inventory UI
        private Text equipListText;
        private Text backpackListText;

        // Jobs UI
        private Text jobsListText;

        private DwarfCharacterController boundDwarf;

        private void Start()
        {
            BuildUIDynamically();

            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnSelectionChanged += HandleSelectionChanged;
            }

            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick += RefreshDisplay;
            }

            HandleSelectionChanged();
        }

        private void OnDestroy()
        {
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnSelectionChanged -= HandleSelectionChanged;
            }
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick -= RefreshDisplay;
            }
        }

        private void BuildUIDynamically()
        {
            // Right docked panel (Width 400, Height 640)
            panelRoot = UIBuilder.CreatePanel(transform, "CharacterInfoPanelRoot",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-210f, 0f), new Vector2(400f, 640f),
                new Color(0.12f, 0.14f, 0.18f, 0.98f));

            // Header Title
            headerText = UIBuilder.CreateText(panelRoot.transform, "HeaderText", "Гном: Не выбран", 16,
                Color.yellow, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(15f, -22f), new Vector2(-70f, 30f));

            statusText = UIBuilder.CreateText(panelRoot.transform, "StatusText", "Статус: -", 12,
                Color.cyan, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(15f, -48f), new Vector2(-70f, 22f));

            // Close button ('X')
            UIBuilder.CreateButton(panelRoot.transform, "BtnClose", "X", 14,
                new Color(0.4f, 0.15f, 0.15f, 1f), Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-25f, -22f), new Vector2(32f, 32f),
                () => SelectionManager.Instance?.ClearSelection());

            // Tab Navigation Bar
            float tabW = 92f;
            float tabH = 32f;
            float tabY = -80f;

            Color tabCol = new Color(0.2f, 0.25f, 0.32f, 1f);
            UIBuilder.CreateButton(panelRoot.transform, "TabAnatomy", "Анатомия", 12, tabCol, Color.white,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(55f, tabY), new Vector2(tabW, tabH),
                () => SetTab(Tab.Anatomy));

            UIBuilder.CreateButton(panelRoot.transform, "TabSkills", "Навыки", 12, tabCol, Color.white,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(152f, tabY), new Vector2(tabW, tabH),
                () => SetTab(Tab.Skills));

            UIBuilder.CreateButton(panelRoot.transform, "TabInv", "Инвентарь", 12, tabCol, Color.white,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(249f, tabY), new Vector2(tabW, tabH),
                () => SetTab(Tab.Inventory));

            UIBuilder.CreateButton(panelRoot.transform, "TabJobs", "Работы", 12, tabCol, Color.white,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(346f, tabY), new Vector2(tabW, tabH),
                () => SetTab(Tab.Jobs));

            // Content Area (Y: -105 to -620)
            BuildAnatomyTab();
            BuildSkillsTab();
            BuildInventoryTab();
            BuildJobsTab();

            SetTab(Tab.Anatomy);
        }

        private void BuildAnatomyTab()
        {
            tabContentAnatomy = UIBuilder.CreatePanel(panelRoot.transform, "Content_Anatomy",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -50f), new Vector2(380f, 500f),
                new Color(0.08f, 0.1f, 0.13f, 0.5f));

            UIBuilder.CreateText(tabContentAnatomy.transform, "Title", "СОСТОЯНИЕ ТЕЛА (KENSHI SYSTEM)", 14,
                Color.yellow, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -15f), new Vector2(360f, 22f));

            var parts = (BodyPartType[])Enum.GetValues(typeof(BodyPartType));
            float startY = -55f;
            float stepY = 55f;

            for (int i = 0; i < parts.Length; i++)
            {
                var pt = parts[i];
                float y = startY - i * stepY;

                string partLabel = GetPartNameRu(pt);
                UIBuilder.CreateText(tabContentAnatomy.transform, $"Label_{pt}", partLabel, 13,
                    Color.white, TextAnchor.MiddleLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100f, y), new Vector2(180f, 20f));

                // HP Bar BG
                GameObject barBg = UIBuilder.CreatePanel(tabContentAnatomy.transform, $"BarBg_{pt}",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(250f, y), new Vector2(180f, 16f),
                    new Color(0.2f, 0.2f, 0.2f, 0.8f));

                Image fill = UIBuilder.CreateImage(barBg.transform, "Fill", null, Color.green,
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(90f, 0f), new Vector2(180f, 16f));
                partFills[pt] = fill;

                Text ptText = UIBuilder.CreateText(tabContentAnatomy.transform, $"Value_{pt}", "100/100", 11,
                    Color.white, TextAnchor.MiddleCenter,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(250f, y - 18f), new Vector2(180f, 16f));
                partTexts[pt] = ptText;
            }

            // Blood indicator
            bloodText = UIBuilder.CreateText(tabContentAnatomy.transform, "BloodText", "🩸 Кровь: 100 / 100", 14,
                Color.red, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 85f), new Vector2(360f, 24f));

            // Bandage Action Button
            bandageButton = UIBuilder.CreateButton(tabContentAnatomy.transform, "BtnBandage", "🩹 Наложить бинт (Аптечка)", 13,
                new Color(0.2f, 0.5f, 0.3f, 1f), Color.white,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(260f, 38f),
                OnBandageClicked);
        }

        private void BuildSkillsTab()
        {
            tabContentSkills = UIBuilder.CreatePanel(panelRoot.transform, "Content_Skills",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -50f), new Vector2(380f, 500f),
                new Color(0.08f, 0.1f, 0.13f, 0.5f));

            skillsListText = UIBuilder.CreateText(tabContentSkills.transform, "SkillsText", "Навыки загружаются...", 13,
                Color.white, TextAnchor.UpperLeft,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(360f, 470f));
        }

        private void BuildInventoryTab()
        {
            tabContentInventory = UIBuilder.CreatePanel(panelRoot.transform, "Content_Inventory",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -50f), new Vector2(380f, 500f),
                new Color(0.08f, 0.1f, 0.13f, 0.5f));

            UIBuilder.CreateText(tabContentInventory.transform, "EquipTitle", "--- ЭКИПИРОВКА ---", 13,
                Color.yellow, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -15f), new Vector2(360f, 20f));

            equipListText = UIBuilder.CreateText(tabContentInventory.transform, "EquipText", "", 12,
                Color.white, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -65f), new Vector2(360f, 90f));

            UIBuilder.CreateText(tabContentInventory.transform, "BackpackTitle", "--- РЮКЗАК / ПРЕДМЕТЫ ---", 13,
                Color.yellow, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(360f, 20f));

            backpackListText = UIBuilder.CreateText(tabContentInventory.transform, "BackpackText", "", 12,
                Color.white, TextAnchor.UpperLeft,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(360f, 280f));
        }

        private void BuildJobsTab()
        {
            tabContentJobs = UIBuilder.CreatePanel(panelRoot.transform, "Content_Jobs",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -50f), new Vector2(380f, 500f),
                new Color(0.08f, 0.1f, 0.13f, 0.5f));

            UIBuilder.CreateText(tabContentJobs.transform, "JobsTitle", "ПОСТОЯННЫЕ РАБОТЫ (KENSHI JOB QUEUE)", 13,
                Color.yellow, TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -15f), new Vector2(360f, 20f));

            jobsListText = UIBuilder.CreateText(tabContentJobs.transform, "JobsList", "Нет назначенных станций", 12,
                Color.white, TextAnchor.UpperLeft,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(360f, 180f));

            UIBuilder.CreateText(tabContentJobs.transform, "Hint",
                "Подсказка: чтобы назначить постоянную работу за верстаком, выберите этого гнома и нажмите Shift+ПКМ по верстаку в мире.",
                11, Color.cyan, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(350f, 60f));

            UIBuilder.CreateButton(tabContentJobs.transform, "BtnClearJobs", "Очистить все работы", 12,
                new Color(0.5f, 0.2f, 0.2f, 1f), Color.white,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 35f), new Vector2(220f, 36f),
                OnClearJobsClicked);
        }

        private void SetTab(Tab tab)
        {
            currentTab = tab;
            if (tabContentAnatomy != null) tabContentAnatomy.SetActive(tab == Tab.Anatomy);
            if (tabContentSkills != null) tabContentSkills.SetActive(tab == Tab.Skills);
            if (tabContentInventory != null) tabContentInventory.SetActive(tab == Tab.Inventory);
            if (tabContentJobs != null) tabContentJobs.SetActive(tab == Tab.Jobs);
            RefreshDisplay();
        }

        private void HandleSelectionChanged()
        {
            boundDwarf = SelectionManager.Instance != null ? SelectionManager.Instance.PrimarySelected : null;
            if (panelRoot != null)
            {
                panelRoot.SetActive(boundDwarf != null);
            }
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (boundDwarf == null || panelRoot == null || !panelRoot.activeSelf) return;

            // Header
            headerText.text = $"{boundDwarf.CharacterName} - {boundDwarf.Profession} ({boundDwarf.Archetype})";
            string ko = boundDwarf.IsDead ? "МЁРТВ" : (boundDwarf.Health.IsIncapacitated ? "БЕЗ СОЗНАНИЯ" : "В сознании");
            string bleed = boundDwarf.Health.IsBleeding ? " | 🩸 КРОВОТЕЧЕНИЕ!" : "";
            statusText.text = $"Статус: {ko}{bleed} | Задача: {boundDwarf.CurrentActionText}";

            // Refresh Active Tab
            if (currentTab == Tab.Anatomy) RefreshAnatomyTab();
            else if (currentTab == Tab.Skills) RefreshSkillsTab();
            else if (currentTab == Tab.Inventory) RefreshInventoryTab();
            else if (currentTab == Tab.Jobs) RefreshJobsTab();
        }

        private void RefreshAnatomyTab()
        {
            var parts = boundDwarf.Health.AllParts;
            foreach (var kvp in parts)
            {
                var pt = kvp.Key;
                var part = kvp.Value;

                if (partFills.TryGetValue(pt, out var fill))
                {
                    float pct = part.HealthNormalized;
                    fill.rectTransform.sizeDelta = new Vector2(180f * pct, 16f);
                    fill.color = part.bleedingRate > 0f ? Color.red : (pct > 0.5f ? Color.green : (pct > 0.25f ? Color.yellow : Color.red));
                }

                if (partTexts.TryGetValue(pt, out var txt))
                {
                    string extra = "";
                    if (part.bleedingRate > 0f) extra = $" 🩸 (-{part.bleedingRate:F1}/с)";
                    else if (part.isBandaged) extra = " 🩹 [Бинт]";
                    else if (part.IsCrippled) extra = " ⚠️ [Травма]";

                    txt.text = $"{(int)part.currentHP}/{(int)part.maxHP}{extra}";
                }
            }

            if (bloodText != null)
            {
                bloodText.text = $"🩸 Уровень крови: {(int)boundDwarf.Health.Blood} / {(int)boundDwarf.Health.MaxBlood}";
            }
        }

        private void RefreshSkillsTab()
        {
            if (skillsListText == null) return;

            var s = boundDwarf.Skills;
            string txt = $"АТРИБУТЫ:\n" +
                         $"  Сила (STR): {s.strength}  |  Ловкость (DEX): {s.dexterity}\n" +
                         $"  Стойкость (TOU): {s.toughness}  |  Атлетика (ATH): {s.athletics}\n\n" +
                         $"НАВЫКИ:\n";

            foreach (var kvp in s.AllSkills)
            {
                float mult = s.GetSkillMultiplier(kvp.Key);
                txt += $"  • {kvp.Key}: ур. {kvp.Value.level}  (XP: {(int)kvp.Value.currentXP}/{(int)kvp.Value.XPToNextLevel})  [x{mult:F2}]\n";
            }

            skillsListText.text = txt;
        }

        private void RefreshInventoryTab()
        {
            var inv = boundDwarf.Inventory;
            if (equipListText != null)
            {
                string weapon = inv.EquippedWeapon != null ? inv.EquippedWeapon.itemName : "Нет";
                string armor = inv.EquippedArmor != null ? inv.EquippedArmor.itemName : "Нет";
                string helmet = inv.EquippedHelmet != null ? inv.EquippedHelmet.itemName : "Нет";
                string tool = inv.EquippedTool != null ? inv.EquippedTool.itemName : "Нет";
                equipListText.text = $"Оружие: {weapon}\nБроня: {armor}\nШлем: {helmet}\nИнструмент: {tool}";
            }

            if (backpackListText != null)
            {
                string list = $"Заполненность: {inv.TotalItemCount}/{inv.MaxCapacity} предметов\n\n";
                for (int i = 0; i < inv.Backpack.Count; i++)
                {
                    var slot = inv.Backpack[i];
                    if (!slot.IsEmpty)
                    {
                        list += $"[{i + 1}] {slot.item.itemName} x{slot.count} ({slot.item.category})\n";
                    }
                }
                backpackListText.text = list;
            }
        }

        private void RefreshJobsTab()
        {
            if (jobsListText == null) return;

            var stations = boundDwarf.AI.AssignedStations;
            if (stations.Count == 0)
            {
                jobsListText.text = "Список постоянных работ пуст.\nГном будет выполнять общие задачи колонии (копка, рубка, перенос).";
                return;
            }

            string txt = "ПРИОРИТЕТЫ ОПЕРАЦИЙ:\n\n";
            for (int i = 0; i < stations.Count; i++)
            {
                var st = stations[i];
                txt += $"{i + 1}. Работать за {st.StationType} (координаты: {st.GridPosition.x}, {st.GridPosition.y})\n";
            }
            jobsListText.text = txt;
        }

        private void OnBandageClicked()
        {
            if (boundDwarf == null) return;
            boundDwarf.Health.ApplyFirstAid(null);
            RefreshDisplay();
        }

        private void OnClearJobsClicked()
        {
            if (boundDwarf == null) return;
            var copy = new List<Crafting.CraftingStation>(boundDwarf.AI.AssignedStations);
            for (int i = 0; i < copy.Count; i++)
            {
                boundDwarf.AI.RemovePermanentStation(copy[i]);
            }
            RefreshDisplay();
        }

        private string GetPartNameRu(BodyPartType type)
        {
            switch (type)
            {
                case BodyPartType.Head: return "Голова";
                case BodyPartType.Torso: return "Торс / Грудь";
                case BodyPartType.LeftArm: return "Левая рука";
                case BodyPartType.RightArm: return "Правая рука";
                case BodyPartType.LeftLeg: return "Левая нога";
                case BodyPartType.RightLeg: return "Правая нога";
                default: return type.ToString();
            }
        }
    }
}
