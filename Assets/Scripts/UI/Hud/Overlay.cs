using UnityEngine;

namespace Featherguard.UI
{
    public sealed class Overlay
    {
        public RectTransform Dim { get; }
        public RectTransform Content { get; }

        Overlay(RectTransform dim, RectTransform content)
        {
            Dim = dim;
            Content = content;
        }

        public static Overlay Create(RectTransform canvas, string name, float alpha, int siblingIndex = -1)
        {
            var dim = Ui.Stretch(name, canvas);
            Ui.Image(dim, new Color(0f, 0f, 0f, alpha), true);
            if (siblingIndex >= 0) dim.SetSiblingIndex(siblingIndex);
            return new Overlay(dim, SafeArea.Create(dim));
        }

        public void Close()
        {
            if (Dim != null) Object.Destroy(Dim.gameObject);
        }

        public static void Title(RectTransform parent, string title, string subtitle, Color? color = null)
        {
            Ui.Label(Ui.Rect("Title", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(1100f, 100f)),
                title, 78, TextAnchor.MiddleCenter, color ?? Ui.GoldColor);
            if (!string.IsNullOrEmpty(subtitle))
                Ui.Label(Ui.Rect("Subtitle", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(1100f, 44f)),
                    subtitle, 32, TextAnchor.MiddleCenter, Color.white);
        }
    }
}
