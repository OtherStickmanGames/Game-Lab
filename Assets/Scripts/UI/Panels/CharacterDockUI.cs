using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DwarfClone.Core;
using DwarfClone.Entity.Character;
using DwarfClone.Entity.Selection;
using DwarfClone.UI.Elements;

namespace DwarfClone.UI.Panels
{
    public class DwarfCardView
    {
        public GameObject root;
        public Image background;
        public Text nameText;
        public Text hotkeyText;
        public ProgressBarUI hpBar;
        public Text hpText;
        public ProgressBarUI hungerBar;
        public Text actionText;
        public DwarfCharacterController dwarf;
        private float lastClickTime = 0f;

        public void Bind(DwarfCharacterController d, int index)
        {
            this.dwarf = d;
            if (hotkeyText != null) hotkeyText.text = $"[{index + 1}]";
            UpdateView();
        }

        public void HandleClick()
        {
            if (dwarf == null) return;

            float time = UnityEngine.Time.time;
            bool isDouble = (time - lastClickTime) < 0.35f;
            lastClickTime = time;

            if (isDouble)
            {
                // Double click: center camera on character
                CameraController.Instance?.FocusOn(dwarf.transform.position);
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                SelectionManager.Instance?.ToggleSelect(dwarf);
            }
            else
            {
                SelectionManager.Instance?.SelectSingle(dwarf);
            }
        }

        public void UpdateView()
        {
            if (dwarf == null)
            {
                if (root != null && root.activeSelf) root.SetActive(false);
                return;
            }

            if (root != null && !root.activeSelf) root.SetActive(true);

            // Selection highlight
            bool isSelected = dwarf.IsSelected;
            if (background != null)
            {
                background.color = isSelected ? new Color(0.24f, 0.44f, 0.70f, 0.98f) : new Color(0.11f, 0.13f, 0.17f, 0.92f);
            }

            // Name & Archetype
            if (nameText != null)
            {
                string status = dwarf.IsDead ? " [DEAD]" : (dwarf.Health.IsIncapacitated ? " [KO]" : "");
                nameText.text = $"{dwarf.CharacterName} ({dwarf.Profession}){status}";
                nameText.color = dwarf.IsDead ? Color.gray : (dwarf.Health.IsIncapacitated ? Color.magenta : Color.white);
            }

            // Health calculation (Sum of body parts)
            float totalHp = 0f;
            float maxHp = 600f; // 6 parts * 100
            foreach (var part in dwarf.Health.AllParts.Values)
            {
                totalHp += part.currentHP;
            }
            float hpPct = Mathf.Clamp01(totalHp / maxHp);

            if (hpBar != null)
            {
                hpBar.SetProgress(hpPct);
                Color barColor = dwarf.Health.IsBleeding ? Color.red : (hpPct > 0.5f ? Color.green : (hpPct > 0.25f ? Color.yellow : Color.red));
                hpBar.SetFillColor(barColor);
            }

            if (hpText != null)
            {
                if (dwarf.Health.IsBleeding)
                {
                    hpText.text = $"HP: {(int)totalHp}/600 🩸 БЛИДИНГ!";
                    hpText.color = Color.red;
                }
                else
                {
                    hpText.text = $"HP: {(int)totalHp}/600";
                    hpText.color = Color.white;
                }
            }

            // Hunger
            float hungerPct = Mathf.Clamp01(dwarf.Needs.Hunger / 100f);
            if (hungerBar != null)
            {
                hungerBar.SetProgress(hungerPct);
                hungerBar.SetFillColor(hungerPct > 0.4f ? new Color(0.95f, 0.75f, 0.2f) : Color.red);
            }

            // Action
            if (actionText != null)
            {
                actionText.text = dwarf.CurrentActionText;
            }
        }
    }

    public class CharacterDockUI : MonoBehaviour
    {
        private readonly List<DwarfCardView> cards = new List<DwarfCardView>();

        private void Start()
        {
            BuildCardsDynamically();
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick += RefreshCards;
            }
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnSelectionChanged += RefreshCards;
            }
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTick -= RefreshCards;
            }
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnSelectionChanged -= RefreshCards;
            }
        }

        private void BuildCardsDynamically()
        {
            // Top-Left colonist bar (RimWorld style directly below TopBar)
            float cardW = 148f;
            float cardH = 64f;
            float spacing = 152f;
            float startX = 12f + cardW / 2f;
            float startY = -86f; // TopBar is 0..-48, card is -54..-118

            for (int i = 0; i < 5; i++)
            {
                GameObject cardObj = UIBuilder.CreatePanel(transform, $"DwarfCard_{i}",
                    new Vector2(0f, 1f), new Vector2(0f, 1f),
                    new Vector2(startX + i * spacing, startY), new Vector2(cardW, cardH),
                    new Color(0.11f, 0.13f, 0.17f, 0.92f));

                DwarfCardView view = new DwarfCardView();
                view.root = cardObj;
                view.background = cardObj.GetComponent<Image>();

                // Card button click component
                Button btn = cardObj.AddComponent<Button>();
                btn.onClick.AddListener(() => view.HandleClick());

                // Hotkey badge [1]
                view.hotkeyText = UIBuilder.CreateText(cardObj.transform, "Hotkey", $"[{i + 1}]", 13,
                    Color.yellow, TextAnchor.MiddleLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(28f, 18f), FontStyle.Bold);

                // Name text
                view.nameText = UIBuilder.CreateText(cardObj.transform, "Name", "Dwarf", 12,
                    Color.white, TextAnchor.MiddleLeft,
                    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(80f, -12f), new Vector2(-40f, 18f), FontStyle.Bold);

                // HP Bar BG & Fill
                view.hpBar = UIBuilder.CreateProgressBar(cardObj.transform, "HPBar",
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(136f, 9f),
                    new Color(0.12f, 0.12f, 0.12f, 0.9f), Color.green);

                // HP text
                view.hpText = UIBuilder.CreateText(cardObj.transform, "HP_Text", "HP: 600/600", 11,
                    Color.white, TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(136f, 13f), FontStyle.Bold);

                // Hunger Bar BG & Fill
                view.hungerBar = UIBuilder.CreateProgressBar(cardObj.transform, "HungerBar",
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(136f, 5f),
                    new Color(0.12f, 0.12f, 0.12f, 0.9f), new Color(0.95f, 0.75f, 0.2f));

                // Action status text
                view.actionText = UIBuilder.CreateText(cardObj.transform, "Action", "Idle", 11,
                    new Color(0.85f, 0.9f, 0.95f), TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -54f), new Vector2(136f, 13f), FontStyle.Normal);

                cards.Add(view);
            }

            RefreshCards();
        }

        private void RefreshCards()
        {
            var squad = DwarfCharacterController.Squad;
            for (int i = 0; i < cards.Count; i++)
            {
                if (i < squad.Count)
                {
                    cards[i].Bind(squad[i], i);
                }
                else
                {
                    cards[i].Bind(null, i);
                }
            }
        }
    }
}
