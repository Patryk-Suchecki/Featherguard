using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public enum PopupStyle { Damage, Crit, Heal, Buff }

    public class Popup : MonoBehaviour
    {
        const float Life = 0.95f;

        Vector3 anchor;
        Vector3 jitter;
        Camera view;
        RectTransform rect;
        Text label;
        Color color;
        float pixelScale;
        float age;

        public static void Spawn(RectTransform parent, Camera view, Vector3 anchor, string message, PopupStyle style)
        {
            var (size, color) = style switch
            {
                PopupStyle.Crit => (64, new Color(1f, 0.85f, 0.15f)),
                PopupStyle.Heal => (44, new Color(0.45f, 1f, 0.45f)),
                PopupStyle.Buff => (36, new Color(0.55f, 0.85f, 1f)),
                _ => (46, Color.white),
            };
            var rect = Ui.Rect("Popup", parent, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(240f, 80f));
            var popup = rect.gameObject.AddComponent<Popup>();
            popup.anchor = anchor;
            popup.jitter = new Vector3(Random.Range(-28f, 28f), 0f, 0f);
            popup.view = view;
            popup.rect = rect;
            popup.color = color;
            popup.pixelScale = parent.lossyScale.x;
            popup.label = Ui.Label(Ui.Stretch("Text", rect), message, size, TextAnchor.LowerCenter, color);
            popup.Update();
        }

        void Update()
        {
            age += Time.unscaledDeltaTime;
            float t = age / Life;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }
            float rise = 30f + 90f * (1f - Mathf.Pow(1f - t, 3f));
            rect.position = view.WorldToScreenPoint(anchor) + (jitter + Vector3.up * rise) * pixelScale;
            float punch = t < 0.15f ? Mathf.Lerp(1.8f, 1f, t / 0.15f) : 1f;
            rect.localScale = Vector3.one * punch;
            label.color = new Color(color.r, color.g, color.b, t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f);
        }
    }
}
