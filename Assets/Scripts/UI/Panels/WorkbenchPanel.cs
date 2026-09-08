using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DwarfClone.Core;
using DwarfClone.Crafting;
using DwarfClone.Entity.Character;
using DwarfClone.Entity.Selection;
using DwarfClone.UI.Elements;

namespace DwarfClone.UI.Panels
{
    public class WorkbenchPanel : MonoBehaviour
    {
        [Header("Root")]
        public GameObject panelRoot;
        public Text stationTitleText;
        public Text operatorText;
        public Text queueStatusText;
        public ProgressBarUI progressBarUI;

        // Container for recipes & queue list
        public Transform recipesContainer;
        public Transform queueContainer;

        private CraftingStation boundStation;
        private readonly List<GameObject> recipeItemObjects = new List<GameObject>();
        private readonly List<GameObject> queueItemObjects = new List<GameObject>();

        private void Start()
        {
            BuildUIDynamically();

            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnStationSelected += HandleStationSelected;
            }

            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick += RefreshDynamicProgress;
            }

            HandleStationSelected(SelectionManager.Instance != null ? SelectionManager.Instance.SelectedStation : null);
        }

        private void OnDestroy()
        {
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnStationSelected -= HandleStationSelected;
            }
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick -= RefreshDynamicProgress;
            }
            if (boundStation != null)
            {
                boundStation.OnQueueChanged -= RefreshQueueDisplay;
            }
        }

        private void BuildUIDynamically()
        {
            // Centered panel (Width 480, Height 560)
            panelRoot = UIBuilder.CreatePanel(transform, "WorkbenchPanelRoot",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 10f), new Vector2(480f, 560f),
                new Color(0.10f, 0.12f, 0.16f, 0.98f));

            // Title
            stationTitleText = UIBuilder.CreateText(panelRoot.transform, "StationTitle", "Верстак", 16,
                Color.yellow, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -22f), new Vector2(-60f, 30f), FontStyle.Bold);

            // Close button ('X')
            UIBuilder.CreateButton(panelRoot.transform, "BtnClose", "✕", 14,
                new Color(0.48f, 0.16f, 0.16f, 1f), Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -22f), new Vector2(30f, 30f),
                () => SelectionManager.Instance?.SelectStation(null));

            // Operator Section
            operatorText = UIBuilder.CreateText(panelRoot.transform, "OperatorText", "Оператор: Нет", 13,
                Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -54f), new Vector2(-200f, 24f), FontStyle.Bold);

            UIBuilder.CreateButton(panelRoot.transform, "BtnAssignDwarf", "Назначить гнома", 12,
                new Color(0.20f, 0.35f, 0.50f, 1f), Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-155f, -54f), new Vector2(120f, 28f),
                AssignSelectedDwarf);

            UIBuilder.CreateButton(panelRoot.transform, "BtnClearWorker", "Снять", 12,
                new Color(0.38f, 0.20f, 0.20f, 1f), Color.white,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-45f, -54f), new Vector2(60f, 28f),
                ClearWorker);

            // Progress Bar (Current Craft)
            progressBarUI = UIBuilder.CreateProgressBar(panelRoot.transform, "CraftProgress",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(440f, 22f),
                new Color(0.18f, 0.18f, 0.18f, 0.9f), new Color(0.2f, 0.8f, 0.3f), "В ожидании работы", 12);

            // Section: Current Queue
            UIBuilder.CreateText(panelRoot.transform, "QueueTitle", "ТЕКУЩАЯ ОЧЕРЕДЬ ПРОИЗВОДСТВА:", 13,
                Color.yellow, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -122f), new Vector2(-40f, 22f), FontStyle.Bold);

            GameObject qContainerObj = UIBuilder.CreatePanel(panelRoot.transform, "QueueContainer",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(440f, 90f),
                new Color(0.08f, 0.1f, 0.14f, 0.6f));
            queueContainer = qContainerObj.transform;

            queueStatusText = UIBuilder.CreateText(queueContainer, "QueueEmpty", "Очередь заказов пуста", 12,
                Color.gray, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 75f));

            // Section: Available Recipes
            UIBuilder.CreateText(panelRoot.transform, "RecipesTitle", "ДОСТУПНЫЕ РЕЦЕПТЫ КРАФТА:", 13,
                Color.yellow, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -240f), new Vector2(-40f, 22f), FontStyle.Bold);

            GameObject rContainerObj = UIBuilder.CreatePanel(panelRoot.transform, "RecipesContainer",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 145f), new Vector2(440f, 260f),
                new Color(0.08f, 0.1f, 0.14f, 0.6f));
            recipesContainer = rContainerObj.transform;
        }

        private void HandleStationSelected(CraftingStation station)
        {
            if (boundStation != null)
            {
                boundStation.OnQueueChanged -= RefreshQueueDisplay;
            }

            boundStation = station;

            if (panelRoot != null)
            {
                panelRoot.SetActive(boundStation != null);
            }

            if (boundStation != null)
            {
                boundStation.OnQueueChanged += RefreshQueueDisplay;
                stationTitleText.text = $"{boundStation.StationType} (X:{boundStation.GridPosition.x}, Y:{boundStation.GridPosition.y})";
                RefreshOperatorDisplay();
                PopulateRecipes();
                RefreshQueueDisplay();
            }
        }

        private void RefreshOperatorDisplay()
        {
            if (boundStation == null || operatorText == null) return;

            if (boundStation.AssignedWorker != null)
            {
                string perm = boundStation.IsPermanentWorker ? "[ПОСТОЯННЫЙ]" : "[РАЗОВЫЙ]";
                operatorText.text = $"Оператор: {boundStation.AssignedWorker.CharacterName} {perm}";
                operatorText.color = Color.green;
            }
            else
            {
                operatorText.text = "Оператор: Не назначен (авто-выбор)";
                operatorText.color = Color.gray;
            }
        }

        private void AssignSelectedDwarf()
        {
            if (boundStation == null) return;
            var dwarf = SelectionManager.Instance != null ? SelectionManager.Instance.PrimarySelected : null;
            if (dwarf != null)
            {
                dwarf.AI.AssignPermanentStation(boundStation);
                RefreshOperatorDisplay();
            }
            else
            {
                Debug.Log("[WorkbenchPanel] Выберите гнома перед назначением на верстак!");
            }
        }

        private void ClearWorker()
        {
            if (boundStation == null) return;
            if (boundStation.AssignedWorker != null)
            {
                boundStation.AssignedWorker.AI.RemovePermanentStation(boundStation);
            }
            boundStation.ClearWorker();
            RefreshOperatorDisplay();
        }

        private void PopulateRecipes()
        {
            // Clear old recipe cards
            for (int i = 0; i < recipeItemObjects.Count; i++)
            {
                if (recipeItemObjects[i] != null) Destroy(recipeItemObjects[i]);
            }
            recipeItemObjects.Clear();

            if (boundStation == null || recipesContainer == null) return;

            var recipes = RecipeDatabase.Instance.GetRecipesForStation(boundStation.StationType);
            float itemH = 50f;
            float spacing = 54f;
            float startY = -30f;

            for (int i = 0; i < recipes.Count; i++)
            {
                var r = recipes[i];
                float y = startY - i * spacing;

                GameObject row = UIBuilder.CreatePanel(recipesContainer, $"Recipe_{r.recipeId}",
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(440f, itemH),
                    new Color(0.16f, 0.2f, 0.26f, 0.9f));
                recipeItemObjects.Add(row);

                // Recipe Name and Ingredients
                string ingrText = "";
                for (int j = 0; j < r.inputs.Count; j++)
                {
                    var ing = r.inputs[j];
                    string name = ing.item != null ? ing.item.itemName : "Ресурс";
                    ingrText += $"{ing.count}x {name} ";
                }
                string outName = r.output != null && r.output.item != null ? r.output.item.itemName : "Предмет";

                UIBuilder.CreateText(row.transform, "Title", $"{r.recipeName} ({r.craftTime}c)", 12,
                    Color.yellow, TextAnchor.UpperLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -8f), new Vector2(230f, 18f));

                UIBuilder.CreateText(row.transform, "Details", $"Требуется: {ingrText} -> 1x {outName}", 10,
                    new Color(0.75f, 0.8f, 0.85f), TextAnchor.LowerLeft,
                    new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(10f, 8f), new Vector2(240f, 18f));

                // +1 Button
                UIBuilder.CreateButton(row.transform, "BtnAdd1", "+1", 11,
                    new Color(0.2f, 0.4f, 0.25f, 1f), Color.white,
                    new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-155f, 0f), new Vector2(38f, 32f),
                    () => AddRecipeOrder(r, 1, CraftingMode.SingleOrder));

                // +5 Button
                UIBuilder.CreateButton(row.transform, "BtnAdd5", "+5", 11,
                    new Color(0.2f, 0.4f, 0.25f, 1f), Color.white,
                    new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-110f, 0f), new Vector2(38f, 32f),
                    () => AddRecipeOrder(r, 5, CraftingMode.SingleOrder));

                // Поток (Continuous) Button
                UIBuilder.CreateButton(row.transform, "BtnFlow", "🔄 Поток", 10,
                    new Color(0.4f, 0.3f, 0.15f, 1f), Color.white,
                    new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-45f, 0f), new Vector2(75f, 32f),
                    () => AddRecipeOrder(r, 1, CraftingMode.ContinuousOnFlow));
            }
        }

        private void AddRecipeOrder(Recipe recipe, int count, CraftingMode mode)
        {
            if (boundStation == null || recipe == null) return;
            boundStation.AddOrder(recipe, count, mode);
        }

        private void RefreshQueueDisplay()
        {
            for (int i = 0; i < queueItemObjects.Count; i++)
            {
                if (queueItemObjects[i] != null) Destroy(queueItemObjects[i]);
            }
            queueItemObjects.Clear();

            if (boundStation == null || queueContainer == null) return;

            var queue = boundStation.ProductionQueue;
            bool empty = queue.Count == 0;
            if (queueStatusText != null) queueStatusText.gameObject.SetActive(empty);

            float itemH = 28f;
            float spacing = 30f;
            float startY = -18f;

            for (int i = 0; i < Mathf.Min(queue.Count, 3); i++)
            {
                int orderIndex = i;
                var o = queue[i];
                float y = startY - i * spacing;

                GameObject qRow = UIBuilder.CreatePanel(queueContainer, $"Order_{i}",
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(440f, itemH),
                    new Color(0.18f, 0.22f, 0.28f, 0.95f));
                queueItemObjects.Add(qRow);

                string countStr = (o.mode == CraftingMode.ContinuousOnFlow) ? "[НА ПОТОКЕ 🔄]" : $"x{o.orderedCount}";
                UIBuilder.CreateText(qRow.transform, "Info", $"{i + 1}. {o.recipe.recipeName}  {countStr}", 11,
                    Color.white, TextAnchor.MiddleLeft,
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(15f, 0f), new Vector2(280f, 24f));

                // Toggle Flow Button
                UIBuilder.CreateButton(qRow.transform, "BtnToggleFlow", "🔄", 11,
                    new Color(0.3f, 0.3f, 0.4f, 1f), Color.white,
                    new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-55f, 0f), new Vector2(28f, 22f),
                    () => boundStation.ToggleContinuous(orderIndex));

                // Cancel Button
                UIBuilder.CreateButton(qRow.transform, "BtnCancel", "X", 11,
                    new Color(0.5f, 0.2f, 0.2f, 1f), Color.white,
                    new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(28f, 22f),
                    () => boundStation.RemoveOrder(orderIndex));
            }

            RefreshDynamicProgress();
        }

        private void RefreshDynamicProgress()
        {
            if (boundStation == null || progressBarUI == null) return;

            var order = boundStation.GetCurrentOrder();
            if (order != null && order.recipe != null && order.recipe.craftTime > 0f)
            {
                float pct = Mathf.Clamp01(boundStation.CurrentProgress / order.recipe.craftTime);
                progressBarUI.SetProgress(pct, $"{order.recipe.recipeName}: {(int)(pct * 100f)}%");
            }
            else
            {
                progressBarUI.SetProgress(0f, "В ожидании работы");
            }
        }
    }
}
