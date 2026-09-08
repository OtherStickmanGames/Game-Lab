using System;
using UnityEngine;
using UnityEngine.UI;
using DwarfClone.Building;
using DwarfClone.Building.Zones;
using DwarfClone.Crafting;
using DwarfClone.UI.Elements;

namespace DwarfClone.UI.Panels
{
    public class ActionDockPanel : MonoBehaviour
    {
        [Header("Root Containers")]
        public Transform dockContainer;
        public GameObject buildSubmenu;
        public GameObject workbenchesSubmenu;
        public GameObject zonesSubmenu;
        public Text currentModeText;

        private void Start()
        {
            if (dockContainer == null)
            {
                BuildUIDynamically();
            }

            if (BuildingSystem.Instance != null)
            {
                BuildingSystem.Instance.OnModeChanged += HandleModeChanged;
            }

            UpdateModeDisplay(BuildingSystem.Instance != null ? BuildingSystem.Instance.CurrentMode : BuildingMode.None);
        }

        private void OnDestroy()
        {
            if (BuildingSystem.Instance != null)
            {
                BuildingSystem.Instance.OnModeChanged -= HandleModeChanged;
            }
        }

        private void BuildUIDynamically()
        {
            // Bottom dock bar container
            GameObject dockObj = UIBuilder.CreatePanel(transform, "DockBar",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 32f), new Vector2(800f, 50f),
                new Color(0.10f, 0.12f, 0.16f, 0.95f));
            dockContainer = dockObj.transform;

            // Current Mode Text above dock
            currentModeText = UIBuilder.CreateText(transform, "CurrentModeText",
                "Режим: Обычный", 15, Color.yellow, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 68f), new Vector2(900f, 24f), FontStyle.Bold);

            // Main Dock Buttons
            float btnW = 108f;
            float btnH = 38f;
            float spacing = 112f;
            float startX = -336f;

            Color btnColor = new Color(0.2f, 0.25f, 0.32f, 1f);
            Color txtColor = Color.white;

            UIBuilder.CreateButton(dockContainer, "BtnDig", "⛏️ Копать", 14, btnColor, txtColor,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + 0 * spacing, 0f), new Vector2(btnW, btnH),
                () => SetBuildingMode(BuildingMode.DigArea));

            UIBuilder.CreateButton(dockContainer, "BtnChop", "🪓 Рубить", 14, btnColor, txtColor,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + 1 * spacing, 0f), new Vector2(btnW, btnH),
                () => SetBuildingMode(BuildingMode.ChopTrees));

            UIBuilder.CreateButton(dockContainer, "BtnGather", "🌾 Сбор", 14, btnColor, txtColor,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + 2 * spacing, 0f), new Vector2(btnW, btnH),
                () => SetBuildingMode(BuildingMode.GatherPlants));

            UIBuilder.CreateButton(dockContainer, "BtnBuild", "🏗️ Стройка", 14, btnColor, txtColor,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + 3 * spacing, 0f), new Vector2(btnW, btnH),
                ToggleBuildSubmenu);

            UIBuilder.CreateButton(dockContainer, "BtnWorkbenches", "🔨 Верстаки", 14, btnColor, txtColor,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + 4 * spacing, 0f), new Vector2(btnW, btnH),
                ToggleWorkbenchesSubmenu);

            UIBuilder.CreateButton(dockContainer, "BtnZones", "📦 Зоны", 14, btnColor, txtColor,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + 5 * spacing, 0f), new Vector2(btnW, btnH),
                ToggleZonesSubmenu);

            UIBuilder.CreateButton(dockContainer, "BtnCancel", "❌ Отмена", 14, new Color(0.48f, 0.16f, 0.16f, 1f), txtColor,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + 6 * spacing, 0f), new Vector2(btnW, btnH),
                () => SetBuildingMode(BuildingMode.CancelOrders));

            // Create Submenus
            BuildSubmenuPanels();
        }

        private void BuildSubmenuPanels()
        {
            // 1. Build Submenu (Walls, Floors, Stairs, Doors)
            buildSubmenu = UIBuilder.CreatePanel(transform, "BuildSubmenu",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 96f), new Vector2(660f, 48f),
                new Color(0.14f, 0.17f, 0.22f, 0.98f));
            buildSubmenu.SetActive(false);

            float bW = 122f;
            float bH = 36f;
            Color subBtnCol = new Color(0.24f, 0.3f, 0.38f, 1f);

            UIBuilder.CreateButton(buildSubmenu.transform, "BtnWall", "🧱 Стена", 13, subBtnCol, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-244f, 0f), new Vector2(bW, bH),
                () => { SetBuildingMode(BuildingMode.BuildWall); CloseAllSubmenus(); });

            UIBuilder.CreateButton(buildSubmenu.transform, "BtnFloor", "🪵 Пол", 13, subBtnCol, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-122f, 0f), new Vector2(bW, bH),
                () => { SetBuildingMode(BuildingMode.BuildFloor); CloseAllSubmenus(); });

            UIBuilder.CreateButton(buildSubmenu.transform, "BtnStairsUp", "🪜 Вверх", 13, subBtnCol, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(bW, bH),
                () => { SetBuildingMode(BuildingMode.BuildStairsUp); CloseAllSubmenus(); });

            UIBuilder.CreateButton(buildSubmenu.transform, "BtnStairsDown", "🪜 Вниз", 13, subBtnCol, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(122f, 0f), new Vector2(bW, bH),
                () => { SetBuildingMode(BuildingMode.BuildStairsDown); CloseAllSubmenus(); });

            UIBuilder.CreateButton(buildSubmenu.transform, "BtnDoor", "🚪 Дверь", 13, subBtnCol, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(244f, 0f), new Vector2(bW, bH),
                () => { SetBuildingMode(BuildingMode.BuildDoor); CloseAllSubmenus(); });

            // 2. Zones Submenu (Stockpile, Farm, Workshop, Bedroom)
            zonesSubmenu = UIBuilder.CreatePanel(transform, "ZonesSubmenu",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 96f), new Vector2(580f, 48f),
                new Color(0.14f, 0.17f, 0.22f, 0.98f));
            zonesSubmenu.SetActive(false);

            float zW = 135f;
            UIBuilder.CreateButton(zonesSubmenu.transform, "BtnStockpile", "📦 Склад", 13, subBtnCol, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-205f, 0f), new Vector2(zW, bH),
                () => { BuildingSystem.Instance?.SelectZoneToDesignate(ZoneType.Stockpile); CloseAllSubmenus(); });

            UIBuilder.CreateButton(zonesSubmenu.transform, "BtnFarm", "🌾 Ферма", 13, subBtnCol, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-68f, 0f), new Vector2(zW, bH),
                () => { BuildingSystem.Instance?.SelectZoneToDesignate(ZoneType.Farm); CloseAllSubmenus(); });

            UIBuilder.CreateButton(zonesSubmenu.transform, "BtnWorkshop", "🔨 Цех", 13, subBtnCol, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(68f, 0f), new Vector2(zW, bH),
                () => { BuildingSystem.Instance?.SelectZoneToDesignate(ZoneType.Workshop); CloseAllSubmenus(); });

            UIBuilder.CreateButton(zonesSubmenu.transform, "BtnDorm", "🛏️ Спальня", 13, subBtnCol, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(205f, 0f), new Vector2(zW, bH),
                () => { BuildingSystem.Instance?.SelectZoneToDesignate(ZoneType.Bedroom); CloseAllSubmenus(); });

            // 3. Workbenches Submenu (All 25 crafting stations!)
            workbenchesSubmenu = UIBuilder.CreatePanel(transform, "WorkbenchesSubmenu",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 265f), new Vector2(880f, 310f),
                new Color(0.11f, 0.13f, 0.18f, 0.98f));
            workbenchesSubmenu.SetActive(false);

            UIBuilder.CreateText(workbenchesSubmenu.transform, "Title", "ВЫБЕРИТЕ ВЕРСТАК ДЛЯ ПОСТРОЙКИ (25 СТАНЦИЙ)",
                15, Color.yellow, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(700f, 26f), FontStyle.Bold);

            UIBuilder.CreateButton(workbenchesSubmenu.transform, "BtnCloseWB", "✕", 14,
                new Color(0.48f, 0.16f, 0.16f, 1f), Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -18f), new Vector2(30f, 28f),
                CloseAllSubmenus);

            // Grid of 25 stations (5 columns x 5 rows)
            var stations = (CraftingStationType[])Enum.GetValues(typeof(CraftingStationType));
            float itemW = 166f;
            float itemH = 40f;
            float gridStartX = -344f;
            float gridStartY = -54f;
            float stepX = 172f;
            float stepY = 46f;

            for (int i = 0; i < stations.Length; i++)
            {
                var st = stations[i];
                int col = i % 5;
                int row = i / 5;
                Vector2 pos = new Vector2(gridStartX + col * stepX, gridStartY - row * stepY);

                string displayName = GetStationShortName(st);

                UIBuilder.CreateButton(workbenchesSubmenu.transform, $"StationBtn_{st}", displayName, 13,
                    new Color(0.20f, 0.26f, 0.34f, 1f), Color.white,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), pos, new Vector2(itemW, itemH),
                    () =>
                    {
                        BuildingSystem.Instance?.SelectWorkbenchToBuild(st);
                        CloseAllSubmenus();
                    });
            }
        }

        private string GetStationShortName(CraftingStationType type)
        {
            switch (type)
            {
                case CraftingStationType.CarpenterBench: return "Плотницкий";
                case CraftingStationType.Sawmill: return "Лесопилка";
                case CraftingStationType.StonecutterTable: return "Камнерез";
                case CraftingStationType.Smelter: return "Плавильня";
                case CraftingStationType.BlacksmithAnvil: return "Наковальня";
                case CraftingStationType.WeaponsmithBench: return "Оружейная";
                case CraftingStationType.ArmorsmithForge: return "Бронницкая";
                case CraftingStationType.TanningRack: return "Дубильня";
                case CraftingStationType.LeatherworkerBench: return "Кожевник";
                case CraftingStationType.Loom: return "Ткацкий станок";
                case CraftingStationType.TailorBench: return "Портной";
                case CraftingStationType.CookingStove: return "Печь кухни";
                case CraftingStationType.Brewery: return "Пивоварня";
                case CraftingStationType.Mill: return "Мельница";
                case CraftingStationType.ButcherTable: return "Разделочная";
                case CraftingStationType.JewelerBench: return "Ювелирная";
                case CraftingStationType.ApothecaryTable: return "Аптека";
                case CraftingStationType.MasonryBench: return "Каменщик";
                case CraftingStationType.Kiln: return "Обжиг. печь";
                case CraftingStationType.GlassmakerFurnace: return "Стеклодув";
                case CraftingStationType.MechanicBench: return "Механика";
                case CraftingStationType.SiegeWorkshop: return "Осадная";
                case CraftingStationType.ResearchBench: return "Исследования";
                case CraftingStationType.ToolmakerBench: return "Инструменты";
                case CraftingStationType.FisheryStation: return "Рыболовная";
                default: return type.ToString();
            }
        }

        public void SetBuildingMode(BuildingMode mode)
        {
            BuildingSystem.Instance?.SetMode(mode);
            CloseAllSubmenus();
        }

        public void ToggleBuildSubmenu()
        {
            bool next = !buildSubmenu.activeSelf;
            CloseAllSubmenus();
            buildSubmenu.SetActive(next);
        }

        public void ToggleWorkbenchesSubmenu()
        {
            bool next = !workbenchesSubmenu.activeSelf;
            CloseAllSubmenus();
            workbenchesSubmenu.SetActive(next);
        }

        public void ToggleZonesSubmenu()
        {
            bool next = !zonesSubmenu.activeSelf;
            CloseAllSubmenus();
            zonesSubmenu.SetActive(next);
        }

        public void CloseAllSubmenus()
        {
            if (buildSubmenu != null) buildSubmenu.SetActive(false);
            if (workbenchesSubmenu != null) workbenchesSubmenu.SetActive(false);
            if (zonesSubmenu != null) zonesSubmenu.SetActive(false);
        }

        private void HandleModeChanged(BuildingMode mode)
        {
            UpdateModeDisplay(mode);
            if (mode == BuildingMode.None)
            {
                CloseAllSubmenus();
            }
        }

        private void UpdateModeDisplay(BuildingMode mode)
        {
            if (currentModeText == null) return;

            string hint = " (ЛКМ - выделить область, Esc - отмена)";
            switch (mode)
            {
                case BuildingMode.None:
                    currentModeText.text = "Режим: Обычный (ЛКМ - выбор, ПКМ - приказ / Shift+ПКМ - работа)";
                    break;
                case BuildingMode.DigArea:
                    currentModeText.text = $"Режим: ⛏️ Копать территорию{hint}";
                    break;
                case BuildingMode.ChopTrees:
                    currentModeText.text = $"Режим: 🪓 Рубить деревья{hint}";
                    break;
                case BuildingMode.GatherPlants:
                    currentModeText.text = $"Режим: 🌾 Собирать растения{hint}";
                    break;
                case BuildingMode.BuildWall:
                    currentModeText.text = $"Режим: 🧱 Строительство стен{hint}";
                    break;
                case BuildingMode.BuildFloor:
                    currentModeText.text = $"Режим: 🪵 Настил пола{hint}";
                    break;
                case BuildingMode.BuildStairsUp:
                    currentModeText.text = $"Режим: 🪜 Лестница ВВЕРХ{hint}";
                    break;
                case BuildingMode.BuildStairsDown:
                    currentModeText.text = $"Режим: 🪜 Лестница ВНИЗ{hint}";
                    break;
                case BuildingMode.BuildDoor:
                    currentModeText.text = $"Режим: 🚪 Установка двери{hint}";
                    break;
                case BuildingMode.BuildWorkbench:
                    var st = BuildingSystem.Instance != null ? BuildingSystem.Instance.SelectedWorkbench : CraftingStationType.CarpenterBench;
                    currentModeText.text = $"Режим: 🔨 Постройка верстака ({GetStationShortName(st)}) (ЛКМ - установить)";
                    break;
                case BuildingMode.DesignateZone:
                    var zn = BuildingSystem.Instance != null ? BuildingSystem.Instance.SelectedZone : ZoneType.Stockpile;
                    currentModeText.text = $"Режим: 📦 Разметка зоны ({zn}){hint}";
                    break;
                case BuildingMode.CancelOrders:
                    currentModeText.text = $"Режим: ❌ Отмена задач в области{hint}";
                    break;
            }
        }
    }
}
