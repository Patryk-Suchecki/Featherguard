using System.Collections;
using UnityEngine;

namespace Featherguard.UI
{
    public static class Tweens
    {
        public static IEnumerator Flip(RectTransform rect, float duration = 0.22f)
        {
            var baseScale = rect.localScale;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
            {
                if (rect == null) yield break;
                rect.localScale = new Vector3(Mathf.SmoothStep(0f, 1f, t) * baseScale.x, baseScale.y, 1f);
                yield return null;
            }
            if (rect != null) rect.localScale = baseScale;
        }

        public static IEnumerator Pop(RectTransform rect, float duration = 0.25f, float from = 1.8f)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
            {
                if (rect == null) yield break;
                rect.localScale = Vector3.one * Mathf.Lerp(from, 1f, 1f - Mathf.Pow(1f - t, 3f));
                yield return null;
            }
            if (rect != null) rect.localScale = Vector3.one;
        }

        public static IEnumerator Fade(UnityEngine.UI.Graphic graphic, float duration)
        {
            var color = graphic.color;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
            {
                if (graphic == null) yield break;
                graphic.color = new Color(color.r, color.g, color.b, color.a * (1f - t));
                yield return null;
            }
        }
    }
}
