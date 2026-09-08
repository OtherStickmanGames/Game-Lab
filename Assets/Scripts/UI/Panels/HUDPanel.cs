using System;
using UnityEngine;
using UnityEngine.UI;
using DwarfClone.Core;
using DwarfClone.World.ZLevel;
using DwarfClone.Inventory;
using DwarfClone.Entity.Character;

namespace DwarfClone.UI.Panels
{
    public class HUDPanel : MonoBehaviour
    {
        [Header("Z-Level UI")]
        public Text zLevelText;
        public Button zUpButton;
        public Button zDownButton;

        [Header("Time & Speed UI")]
        public Text timeText;
        public Button pauseButton;
        public Button speed1Button;
        public Button speed2Button;
        public Button speed5Button;

        [Header("Colony Resources")]
        public Text populationText;
        public Text foodText;
        public Text woodText;
        public Text stoneText;
        public Text ironText;
        public Text coalText;

        private void Start()
        {
            if (zUpButton != null) zUpButton.onClick.AddListener(() => ZLevelManager.Instance?.UpOneLevel());
            if (zDownButton != null) zDownButton.onClick.AddListener(() => ZLevelManager.Instance?.DownOneLevel());

            if (pauseButton != null) pauseButton.onClick.AddListener(() => TimeManager.Instance?.TogglePause());
            if (speed1Button != null) speed1Button.onClick.AddListener(() => TimeManager.Instance?.SetSpeed(1.0f));
            if (speed2Button != null) speed2Button.onClick.AddListener(() => TimeManager.Instance?.SetSpeed(2.0f));
            if (speed5Button != null) speed5Button.onClick.AddListener(() => TimeManager.Instance?.SetSpeed(5.0f));

            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged += UpdateZLevelDisplay;
            }
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeChanged += UpdateTimeDisplay;
                TimeManager.Instance.OnTick += UpdateResourceDisplay;
            }

            UpdateZLevelDisplay(ZLevelManager.Instance != null ? ZLevelManager.Instance.CurrentZ : Constants.SURFACE_Z_LEVEL);
            UpdateResourceDisplay();
        }

        private void OnDestroy()
        {
            if (ZLevelManager.Instance != null)
            {
                ZLevelManager.Instance.OnZLevelChanged -= UpdateZLevelDisplay;
            }
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeChanged -= UpdateTimeDisplay;
                TimeManager.Instance.OnTick -= UpdateResourceDisplay;
            }
        }

        public void UpdateZLevelDisplay(int z)
        {
            if (zLevelText == null) return;
            string label = (z == Constants.SURFACE_Z_LEVEL) ? "Surface" : (z < Constants.SURFACE_Z_LEVEL ? $"Cavern -{Constants.SURFACE_Z_LEVEL - z}" : $"+{z - Constants.SURFACE_Z_LEVEL}");
            zLevelText.text = $"Z-LEVEL: {z} ({label}) [< / >]";
        }

        public void UpdateTimeDisplay(int day, int hour, int minute)
        {
            if (timeText != null)
            {
                string paused = (TimeManager.Instance != null && TimeManager.Instance.IsPaused) ? " [PAUSED]" : "";
                timeText.text = $"Day {day:00} - {hour:00}:{minute:00}{paused}";
            }
        }

        public void UpdateResourceDisplay()
        {
            int pop = DwarfCharacterController.Squad.Count;
            int conscious = 0;
            int totalFood = 0;
            int totalWood = 0;
            int totalStone = 0;
            int totalIron = 0;
            int totalCoal = 0;

            for (int i = 0; i < pop; i++)
            {
                var d = DwarfCharacterController.Squad[i];
                if (!d.IsDead && !d.Health.IsIncapacitated) conscious++;

                totalFood += d.Inventory.GetItemCount("item_bread") + d.Inventory.GetItemCount("item_meat_stew") + d.Inventory.GetItemCount("item_berries");
                totalWood += d.Inventory.GetItemCount("item_wood_plank") + d.Inventory.GetItemCount("item_wood_log");
                totalStone += d.Inventory.GetItemCount("item_stone_block") + d.Inventory.GetItemCount("item_stone_rough");
                totalIron += d.Inventory.GetItemCount("item_ingot_iron") + d.Inventory.GetItemCount("item_ore_iron");
                totalCoal += d.Inventory.GetItemCount("item_coal");
            }

            if (populationText != null) populationText.text = $"👥 {conscious}/{pop}";
            if (foodText != null) foodText.text = $"🍞 {totalFood}";
            if (woodText != null) woodText.text = $"🪵 {totalWood}";
            if (stoneText != null) stoneText.text = $"🪨 {totalStone}";
            if (ironText != null) ironText.text = $"⚔️ {totalIron}";
            if (coalText != null) coalText.text = $"🔥 {totalCoal}";
        }
    }
}
