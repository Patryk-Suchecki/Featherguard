using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class TopBar
    {
        readonly Text progress;

        public TopBar(RectTransform safe)
        {
            var progressPill = Ui.Rect("Progress", safe, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -20f), new Vector2(470f, 76f));
            Ui.Sliced(progressPill, "Pill", Ui.PanelColor);
            Ui.Icon(progressPill, "Flag", new Vector2(44f, 0f), 54f, anchor: new Vector2(0f, 0.5f));
            progress = Ui.Label(Ui.Stretch("Label", progressPill), "", 32, TextAnchor.MiddleLeft, Color.white);
            progress.rectTransform.offsetMin = new Vector2(84f, 0f);
        }

        public void SetProgress(int encounter, int encounters, int wave, int waves) =>
            progress.text = encounters > 1
                ? (waves > 1 ? $"Battle {encounter}/{encounters} · Wave {wave}/{waves}" : $"Battle {encounter}/{encounters}")
                : (waves > 1 ? $"Wave {wave}/{waves}" : "Battle");
    }
}
