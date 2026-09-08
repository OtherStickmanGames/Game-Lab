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
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (defaultFont == null)
                {
                    defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
                }
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
            Vector2 sizeDelta)
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
            txt.color = color;
            txt.alignment = alignment;
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
                CreateText(obj.transform, "Label", label, fontSize, textColor, TextAnchor.MiddleCenter,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
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
    }
}
