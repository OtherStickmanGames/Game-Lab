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
        public Image hpFill;
        public Text hpText;
        public Image hungerFill;
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
                background.color = isSelected ? new Color(0.25f, 0.4f, 0.6f, 0.95f) : new Color(0.14f, 0.16f, 0.2f, 0.9f);
            }

            // Name & Archetype
            if (nameText != null)
            {
                string status = dwarf.IsDead ? " [DEAD]" : (dwarf.Health.IsIncapacitated ? " [KO]" : "");
                nameText.text = $"{dwarf.CharacterName} ({dwarf.Profession}){status}";
                nameText.color = dwarf.IsDead ? Color.gray : (dwarf.Health.IsIncapacitated ? Color.magenta : Color.white);
            }

            // Health calculation (Average of body parts or blood)
            float totalHp = 0f;
            float maxHp = 600f; // 6 parts * 100
            foreach (var part in dwarf.Health.AllParts.Values)
            {
                totalHp += part.currentHP;
            }
            float hpPct = Mathf.Clamp01(totalHp / maxHp);

            if (hpFill != null)
            {
                RectTransform rt = hpFill.rectTransform;
                rt.sizeDelta = new Vector2(170f * hpPct, 10f);
                hpFill.color = dwarf.Health.IsBleeding ? Color.red : (hpPct > 0.5f ? Color.green : (hpPct > 0.25f ? Color.yellow : Color.red));
            }

            if (hpText != null)
            {
                if (dwarf.Health.IsBleeding)
                {
                    hpText.text = $"HP: {(int)totalHp}/600 🩸 БЛЕЕДИНГ!";
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
            if (hungerFill != null)
            {
                RectTransform rt = hungerFill.rectTransform;
                rt.sizeDelta = new Vector2(170f * hungerPct, 6f);
                hungerFill.color = hungerPct > 0.4f ? new Color(0.9f, 0.7f, 0.2f) : Color.red;
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
            // Bottom-Left container: 5 cards horizontally
            float cardW = 180f;
            float cardH = 95f;
            float spacing = 185f;
            float startX = 100f;
            float startY = 120f;

            for (int i = 0; i < 5; i++)
            {
                GameObject cardObj = UIBuilder.CreatePanel(transform, $"DwarfCard_{i}",
                    new Vector2(0f, 0f), new Vector2(0f, 0f),
                    new Vector2(startX + i * spacing, startY), new Vector2(cardW, cardH),
                    new Color(0.14f, 0.16f, 0.2f, 0.9f));

                DwarfCardView view = new DwarfCardView();
                view.root = cardObj;
                view.background = cardObj.GetComponent<Image>();

                // Card button click component
                Button btn = cardObj.AddComponent<Button>();
                btn.onClick.AddListener(() => view.HandleClick());

                // Hotkey badge [1]
                view.hotkeyText = UIBuilder.CreateText(cardObj.transform, "Hotkey", $"[{i + 1}]", 13,
                    Color.yellow, TextAnchor.UpperLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -12f), new Vector2(30f, 20f));

                // Name text
                view.nameText = UIBuilder.CreateText(cardObj.transform, "Name", "Dwarf", 12,
                    Color.white, TextAnchor.UpperLeft,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(105f, -12f), new Vector2(140f, 20f));

                // HP Bar BG & Fill
                GameObject hpBg = UIBuilder.CreatePanel(cardObj.transform, "HP_BG",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(90f, -28f), new Vector2(170f, 10f),
                    new Color(0.1f, 0.1f, 0.1f, 0.8f));
                view.hpFill = UIBuilder.CreateImage(hpBg.transform, "HP_Fill", null, Color.green,
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(85f, 0f), new Vector2(170f, 10f));

                // HP text
                view.hpText = UIBuilder.CreateText(cardObj.transform, "HP_Text", "HP: 600/600", 11,
                    Color.white, TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(170f, 16f));

                // Hunger Bar BG & Fill
                GameObject hungerBg = UIBuilder.CreatePanel(cardObj.transform, "Hunger_BG",
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(90f, -54f), new Vector2(170f, 6f),
                    new Color(0.1f, 0.1f, 0.1f, 0.8f));
                view.hungerFill = UIBuilder.CreateImage(hungerBg.transform, "Hunger_Fill", null,
                    new Color(0.9f, 0.7f, 0.2f),
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(85f, 0f), new Vector2(170f, 6f));

                // Action status text
                view.actionText = UIBuilder.CreateText(cardObj.transform, "Action", "Idle", 11,
                    new Color(0.8f, 0.85f, 0.9f), TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 15f), new Vector2(170f, 20f));

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
