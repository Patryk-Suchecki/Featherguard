using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class BannerView
    {
        const float BannerHold = 1.6f;
        const float BannerFade = 0.4f;
        const float CalloutHold = 1.1f;
        const float CalloutFade = 0.3f;
        const float CalloutPop = 0.15f;
        static readonly Color BannerColor = new(1f, 0.92f, 0.5f);
        static readonly Color CalloutColor = new(1f, 0.45f, 0.15f);

        readonly Text banner;
        readonly Text callout;
        float bannerUntil;
        float calloutStart = -10f;

        public BannerView(RectTransform safe)
        {
            banner = Ui.Label(Ui.Rect("Banner", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1200f, 90f)),
                "", 70, TextAnchor.MiddleCenter, BannerColor);
            callout = Ui.Label(Ui.Rect("Callout", safe, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1600f, 140f)),
                "", 100, TextAnchor.MiddleCenter, CalloutColor);
        }

        public void ShowBanner(string text)
        {
            banner.text = text;
            bannerUntil = Time.unscaledTime + BannerHold;
        }

        public void Callout(string text)
        {
            callout.text = text;
            calloutStart = Time.unscaledTime;
        }

        public void Update()
        {
            banner.color = WithAlpha(banner.color, Mathf.Clamp01((bannerUntil - Time.unscaledTime) / BannerFade));
            float age = Time.unscaledTime - calloutStart;
            callout.color = WithAlpha(callout.color, age < CalloutHold ? 1f : Mathf.Clamp01(1f - (age - CalloutHold) / CalloutFade));
            callout.rectTransform.localScale = Vector3.one * (age < CalloutPop ? Mathf.Lerp(1.6f, 1f, age / CalloutPop) : 1f);
        }

        static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);
    }
}
