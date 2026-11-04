using Featherguard.Run;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class TopBar
    {
        readonly Text progress;
        readonly Text gold;
        Wallet wallet;

        public TopBar(RectTransform safe)
        {
            var progressPill = Ui.Rect("Progress", safe, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -20f), new Vector2(470f, 76f));
            Ui.Sliced(progressPill, "Pill", Ui.PanelColor);
            Ui.Icon(progressPill, "Flag", new Vector2(44f, 0f), 54f, anchor: new Vector2(0f, 0.5f));
            progress = Ui.Label(Ui.Stretch("Label", progressPill), "", 32, TextAnchor.MiddleLeft, Color.white);
            progress.rectTransform.offsetMin = new Vector2(84f, 0f);

            gold = Pill(safe, "Gold", "Coin", new Vector2(-134f, -20f), Ui.GoldColor);
        }

        public static Text Pill(RectTransform parent, string name, string icon, Vector2 position, Color color, int fontSize = 38)
        {
            var pill = Ui.Rect(name, parent, new Vector2(1f, 1f), new Vector2(1f, 1f), position, new Vector2(250f, 76f));
            Ui.Sliced(pill, "Pill", Ui.PanelColor);
            Ui.Icon(pill, icon, new Vector2(42f, 0f), 58f, anchor: new Vector2(0f, 0.5f));
            var label = Ui.Label(Ui.Stretch("Label", pill), "", fontSize, TextAnchor.MiddleCenter, color);
            label.rectTransform.offsetMin = new Vector2(70f, 0f);
            return label;
        }

        public void SetProgress(int encounter, int encounters, int wave, int waves) =>
            progress.text = waves > 1 ? $"Walka {encounter}/{encounters} · Fala {wave}/{waves}" : $"Walka {encounter}/{encounters}";

        public void Bind(Wallet target)
        {
            if (wallet != null) wallet.Changed -= Refresh;
            wallet = target;
            wallet.Changed += Refresh;
            Refresh();
        }

        public void Unbind()
        {
            if (wallet != null) wallet.Changed -= Refresh;
        }

        void Refresh() => gold.text = wallet.Gold.ToString();
    }
}
