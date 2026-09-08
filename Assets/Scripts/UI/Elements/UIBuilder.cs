using System;
using UnityEngine;
using UnityEngine.UI;

namespace DwarfClone.UI.Elements
{
    public static class UIBuilder
    {
        private static Font defaultFont;

        public static Font GetDefaultFont()
        {
            if (defaultFont != null) return defaultFont;

            // 1. Try modern Unity (2022.2+) built-in font
            try
            {
                defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch {}

            // 2. Try older Unity built-in font
            if (defaultFont == null)
            {
                try
                {
                    defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch {}
            }

            // 3. Fallback to OS font
            if (defaultFont == null)
            {
                try
                {
                    defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
                }
                catch {}
            }

            return defaultFont;
        }

        public static GameObject CreatePanel(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color bgColor)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            Image img = obj.GetComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            return obj;
        }

        public static Text CreateText(
            Transform parent,
            string name,
            string content,
            int fontSize,
            Color color,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            FontStyle fontStyle = FontStyle.Normal)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            Text txt = obj.GetComponent<Text>();
            txt.text = content;
            txt.font = GetDefaultFont();
            txt.fontSize = fontSize;
            txt.fontStyle = fontStyle;
            txt.color = color;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;

            return txt;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            string label,
            int fontSize,
            Color bgColor,
            Color textColor,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Action onClick)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            Image img = obj.GetComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;

            Button btn = obj.GetComponent<Button>();
            if (onClick != null)
            {
                btn.onClick.AddListener(() => onClick());
            }

            if (!string.IsNullOrEmpty(label))
            {
                Text lbl = CreateText(obj.transform, "Label", label, fontSize, textColor, TextAnchor.MiddleCenter,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);
                lbl.horizontalOverflow = HorizontalWrapMode.Overflow;
            }

            return btn;
        }

        public static Image CreateImage(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;

            Image img = obj.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;

            return img;
        }

        public static ProgressBarUI CreateProgressBar(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color bgColor,
            Color fillColor,
            string initialLabel = "",
            int labelFontSize = 11)
        {
            GameObject bgObj = CreatePanel(parent, name + "_BG", anchorMin, anchorMax, anchoredPosition, sizeDelta, bgColor);

            GameObject fillObj = new GameObject(name + "_Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObj.transform.SetParent(bgObj.transform, false);

            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.anchoredPosition = Vector2.zero;
            fillRt.sizeDelta = new Vector2(sizeDelta.x, 0f);

            Image fillImg = fillObj.GetComponent<Image>();
            fillImg.color = fillColor;
            fillImg.raycastTarget = false;

            Text labelTxt = null;
            if (!string.IsNullOrEmpty(initialLabel))
            {
                labelTxt = CreateText(bgObj.transform, name + "_Label", initialLabel, labelFontSize, Color.white, TextAnchor.MiddleCenter,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, FontStyle.Bold);
                labelTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
            }

            return new ProgressBarUI
            {
                root = bgObj,
                fill = fillImg,
                label = labelTxt,
                maxWidth = sizeDelta.x
            };
        }
    }

    public class ProgressBarUI
    {
        public GameObject root;
        public Image fill;
        public Text label;
        public float maxWidth;

        public void SetProgress(float pct, string labelText = null)
        {
            pct = Mathf.Clamp01(pct);
            if (fill != null)
            {
                fill.rectTransform.sizeDelta = new Vector2(maxWidth * pct, 0f);
            }
            if (label != null && labelText != null)
            {
                label.text = labelText;
            }
        }

        public void SetFillColor(Color color)
        {
            if (fill != null)
            {
                fill.color = color;
            }
        }
    }
}
