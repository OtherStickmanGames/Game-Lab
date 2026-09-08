using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DwarfClone.UI.Panels;
using DwarfClone.UI.Elements;

namespace DwarfClone.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Canvas References")]
        public Canvas mainCanvas;
        public CanvasScaler canvasScaler;
        public GraphicRaycaster graphicRaycaster;

        [Header("Managed Panels")]
        public HUDPanel hudPanel;
        public ActionDockPanel actionDockPanel;
        public CharacterDockUI characterDockUI;
        public CharacterInfoPanel characterInfoPanel;
        public WorkbenchPanel workbenchPanel;

        [Header("Tooltip System")]
        private GameObject tooltipObj;
        private Text tooltipText;
        private RectTransform tooltipRect;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureEventSystem();
            EnsureCanvas();
            InitializePanels();
            InitializeTooltip();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                GameObject esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(esObj);
            }
        }

        private void EnsureCanvas()
        {
            mainCanvas = GetComponentInChildren<Canvas>();
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
            }

            if (mainCanvas == null)
            {
                GameObject canvasObj = new GameObject("MainCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObj.transform.SetParent(transform, false);

                mainCanvas = canvasObj.GetComponent<Canvas>();
                mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                mainCanvas.sortingOrder = 100;

                canvasScaler = canvasObj.GetComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1280, 720);
                canvasScaler.matchWidthOrHeight = 0.5f;

                graphicRaycaster = canvasObj.GetComponent<GraphicRaycaster>();
            }
            else
            {
                canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
                if (canvasScaler == null)
                {
                    canvasScaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
                }
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1280, 720);
                canvasScaler.matchWidthOrHeight = 0.5f;

                graphicRaycaster = mainCanvas.GetComponent<GraphicRaycaster>();
            }
        }

        private void InitializePanels()
        {
            Transform cTrans = mainCanvas.transform;

            // HUD Panel
            hudPanel = cTrans.GetComponentInChildren<HUDPanel>();
            if (hudPanel == null)
            {
                GameObject hudObj = new GameObject("HUDPanel");
                hudObj.transform.SetParent(cTrans, false);
                hudPanel = hudObj.AddComponent<HUDPanel>();
                BuildHUDVisuals(hudObj.transform);
            }

            // Action Dock Panel
            actionDockPanel = cTrans.GetComponentInChildren<ActionDockPanel>();
            if (actionDockPanel == null)
            {
                GameObject dockObj = new GameObject("ActionDockPanel");
                dockObj.transform.SetParent(cTrans, false);
                actionDockPanel = dockObj.AddComponent<ActionDockPanel>();
            }

            // Character Dock UI (Squad Cards)
            characterDockUI = cTrans.GetComponentInChildren<CharacterDockUI>();
            if (characterDockUI == null)
            {
                GameObject charDockObj = new GameObject("CharacterDockUI");
                charDockObj.transform.SetParent(cTrans, false);
                characterDockUI = charDockObj.AddComponent<CharacterDockUI>();
            }

            // Character Info Panel (Kenshi Inspector)
            characterInfoPanel = cTrans.GetComponentInChildren<CharacterInfoPanel>();
            if (characterInfoPanel == null)
            {
                GameObject charInfoObj = new GameObject("CharacterInfoPanel");
                charInfoObj.transform.SetParent(cTrans, false);
                characterInfoPanel = charInfoObj.AddComponent<CharacterInfoPanel>();
            }

            // Workbench Panel (Crafting & Production Queue)
            workbenchPanel = cTrans.GetComponentInChildren<WorkbenchPanel>();
            if (workbenchPanel == null)
            {
                GameObject wbObj = new GameObject("WorkbenchPanel");
                wbObj.transform.SetParent(cTrans, false);
                workbenchPanel = wbObj.AddComponent<WorkbenchPanel>();
            }
        }

        private void BuildHUDVisuals(Transform hudRoot)
        {
            // Top Bar - spans full width at the very top
            GameObject topBar = UIBuilder.CreatePanel(hudRoot, "TopBar",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -24f), new Vector2(0f, 48f),
                new Color(0.09f, 0.11f, 0.15f, 0.96f));

            // Z-Level text & buttons (Left side)
            hudPanel.zLevelText = UIBuilder.CreateText(topBar.transform, "ZLevelText", "Z-LEVEL: 4 (Surface) [< / >]", 15,
                Color.yellow, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(115f, 0f), new Vector2(210f, 36f), FontStyle.Bold);

            hudPanel.zUpButton = UIBuilder.CreateButton(topBar.transform, "BtnZUp", "▲ Z", 14,
                new Color(0.22f, 0.32f, 0.44f, 1f), Color.white,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(245f, 0f), new Vector2(44f, 32f),
                null);

            hudPanel.zDownButton = UIBuilder.CreateButton(topBar.transform, "BtnZDown", "▼ Z", 14,
                new Color(0.22f, 0.32f, 0.44f, 1f), Color.white,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(295f, 0f), new Vector2(44f, 32f),
                null);

            // Time & Speeds (Center)
            hudPanel.timeText = UIBuilder.CreateText(topBar.transform, "TimeText", "Day 01 - 08:00", 15,
                Color.white, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-90f, 0f), new Vector2(160f, 36f), FontStyle.Bold);

            hudPanel.pauseButton = UIBuilder.CreateButton(topBar.transform, "BtnPause", "⏸️", 14,
                new Color(0.28f, 0.28f, 0.38f, 1f), Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(10f, 0f), new Vector2(34f, 32f),
                null);

            hudPanel.speed1Button = UIBuilder.CreateButton(topBar.transform, "Btn1x", "1x", 13,
                new Color(0.24f, 0.36f, 0.3f, 1f), Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(48f, 0f), new Vector2(34f, 32f),
                null);

            hudPanel.speed2Button = UIBuilder.CreateButton(topBar.transform, "Btn2x", "2x", 13,
                new Color(0.24f, 0.36f, 0.3f, 1f), Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(86f, 0f), new Vector2(34f, 32f),
                null);

            hudPanel.speed5Button = UIBuilder.CreateButton(topBar.transform, "Btn5x", "5x", 13,
                new Color(0.24f, 0.36f, 0.3f, 1f), Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(124f, 0f), new Vector2(34f, 32f),
                null);

            // Colony Resources (Right side of top bar)
            hudPanel.coalText = UIBuilder.CreateText(topBar.transform, "CoalText", "🔥 0", 14,
                Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40f, 0f), new Vector2(75f, 32f), FontStyle.Bold);

            hudPanel.ironText = UIBuilder.CreateText(topBar.transform, "IronText", "⚔️ 0", 14,
                Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-115f, 0f), new Vector2(75f, 32f), FontStyle.Bold);

            hudPanel.stoneText = UIBuilder.CreateText(topBar.transform, "StoneText", "🪨 0", 14,
                Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-190f, 0f), new Vector2(75f, 32f), FontStyle.Bold);

            hudPanel.woodText = UIBuilder.CreateText(topBar.transform, "WoodText", "🪵 0", 14,
                Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-265f, 0f), new Vector2(75f, 32f), FontStyle.Bold);

            hudPanel.foodText = UIBuilder.CreateText(topBar.transform, "FoodText", "🍞 15", 14,
                Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-345f, 0f), new Vector2(85f, 32f), FontStyle.Bold);

            hudPanel.populationText = UIBuilder.CreateText(topBar.transform, "PopText", "👥 5/5", 14,
                Color.cyan, TextAnchor.MiddleRight,
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-440f, 0f), new Vector2(105f, 32f), FontStyle.Bold);
        }

        private void InitializeTooltip()
        {
            tooltipObj = UIBuilder.CreatePanel(mainCanvas.transform, "FloatingTooltip",
                Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(240f, 60f),
                new Color(0.05f, 0.05f, 0.08f, 0.95f));
            tooltipRect = tooltipObj.GetComponent<RectTransform>();

            tooltipText = UIBuilder.CreateText(tooltipObj.transform, "Text", "", 12, Color.white, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            tooltipObj.SetActive(false);
        }

        public void ShowTooltip(string text, Vector2 screenPos)
        {
            if (tooltipObj == null) return;
            tooltipText.text = text;
            tooltipRect.position = screenPos + new Vector2(15f, -15f);
            tooltipObj.SetActive(true);
        }

        public void HideTooltip()
        {
            if (tooltipObj != null) tooltipObj.SetActive(false);
        }
    }
}
