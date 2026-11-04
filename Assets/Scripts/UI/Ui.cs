using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public static class Ui
    {
        public static readonly Color PanelColor = new(0.07f, 0.08f, 0.13f, 0.92f);
        public static readonly Color GoColor = new(0.22f, 0.62f, 0.28f, 1f);
        public static readonly Color RiskColor = new(0.82f, 0.24f, 0.14f, 1f);
        public static readonly Color NeutralColor = new(0.26f, 0.32f, 0.44f, 1f);
        public static readonly Color GoldColor = new(1f, 0.82f, 0.25f);

        static Font font;

        public static UiSprites Sprites { get; set; }
        public static Font Font => font != null ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static Sprite Sprite(string name) => Sprites != null ? Sprites.Get(name) : null;

        public static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Centered(string name, Transform parent, Vector2 position, Vector2 size) =>
            Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);

        public static RectTransform Stretch(string name, Transform parent, float inset = 0f)
        {
            var rt = Rect(name, parent, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
            return rt;
        }

        public static Image Image(RectTransform rt, Color color, bool raycast = false)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public static Image Sliced(RectTransform rt, string sprite, Color color, bool raycast = false)
        {
            var image = Image(rt, color, raycast);
            image.sprite = Sprite(sprite);
            image.type = UnityEngine.UI.Image.Type.Sliced;
            return image;
        }

        public static Image Panel(RectTransform rt, Color color, bool raycast = false) => Sliced(rt, "RoundRect", color, raycast);

        public static Image Icon(RectTransform parent, string sprite, Vector2 position, float size, Color? color = null, Vector2? anchor = null)
        {
            var a = anchor ?? new Vector2(0.5f, 0.5f);
            var rt = Rect(sprite, parent, a, new Vector2(0.5f, 0.5f), position, Vector2.one * size);
            var image = Image(rt, color ?? Color.white);
            image.sprite = Sprite(sprite);
            image.preserveAspect = true;
            return image;
        }

        public static Text Label(RectTransform rt, string text, int size, TextAnchor alignment, Color color)
        {
            var label = rt.gameObject.AddComponent<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = size;
            label.fontStyle = FontStyle.Bold;
            label.alignment = alignment;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            var outline = rt.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2.5f, -2.5f);
            return label;
        }

        public static Text Wrapped(RectTransform rt, string text, int size, TextAnchor alignment, Color color)
        {
            var label = Label(rt, text, size, alignment, color);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            return label;
        }

        public static Button Button(RectTransform rt, Color color, UnityEngine.Events.UnityAction onClick, string sprite = "RoundRect")
        {
            var image = Sliced(rt, sprite, color, true);
            var lip = rt.gameObject.AddComponent<Shadow>();
            lip.effectColor = new Color(0f, 0f, 0f, 0.45f);
            lip.effectDistance = new Vector2(0f, -7f);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
            button.colors = colors;
            button.onClick.AddListener(onClick);
            rt.gameObject.AddComponent<PressFeel>();
            return button;
        }

        public static RectTransform TextButton(RectTransform parent, string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick, string icon = null)
        {
            var rect = Centered(label, parent, position, size);
            Button(rect, color, onClick);
            var text = Label(Stretch("Label", rect), label, Mathf.RoundToInt(Mathf.Min(42f, size.y * 0.38f, size.x * 0.19f)), TextAnchor.MiddleCenter, Color.white);
            if (icon != null)
            {
                float iconSize = size.y * 0.62f;
                Icon(rect, icon, new Vector2(size.y * 0.55f, 0f), iconSize, anchor: new Vector2(0f, 0.5f));
                text.rectTransform.offsetMin = new Vector2(size.y * 0.8f, 0f);
            }
            return rect;
        }

        public static Image Bar(RectTransform parent, Vector2 position, Vector2 size, Color fill, out RectTransform fillRect)
        {
            var background = Centered("Bar", parent, position, size);
            Sliced(background, "Pill", new Color(0.03f, 0.03f, 0.06f, 0.9f));
            fillRect = Stretch("Fill", background, 3f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            return Sliced(fillRect, "Pill", fill);
        }

        public static void SetFill(RectTransform fill, float fraction)
        {
            fraction = Mathf.Clamp01(fraction);
            fill.anchorMax = new Vector2(Mathf.Max(0.001f, fraction), 1f);
            fill.gameObject.SetActive(fraction > 0f);
        }
    }
}
