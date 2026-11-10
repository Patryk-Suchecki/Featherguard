using System.Collections;
using System.Linq;
using Featherguard.Feedback;
using Featherguard.Run;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class RewardScreen
    {
        static readonly Vector2 ButtonSize = new(460f, 120f);
        static readonly string[] HeaderParts = { "Title", "Subtitle", "Party", "Stats" };

        readonly BattleHud hud;
        readonly RunSession session;
        readonly MonoBehaviour host;
        Overlay overlay;

        public RectTransform Root => overlay.Content;
        public RectTransform Dim => overlay.Dim;
        public int Choice { get; private set; } = -1;

        public RewardScreen(BattleHud hud, RunSession session, MonoBehaviour host)
        {
            this.hud = hud;
            this.session = session;
            this.host = host;
        }

        public RectTransform Open(string title, string subtitle)
        {
            overlay?.Close();
            overlay = hud.OpenOverlay("Rewards");
            Header(title, subtitle);
            Choice = -1;
            return Root;
        }

        public void RefreshHeader(string title, string subtitle)
        {
            foreach (var part in Root.Cast<Transform>().Where(t => HeaderParts.Contains(t.name)).ToList())
                Object.Destroy(part.gameObject);
            Header(title, subtitle);
        }

        void Header(string title, string subtitle)
        {
            Overlay.Title(Root, title, subtitle);
            PartyStrip.Build(Root, session);
        }

        public void Close() => overlay?.Close();

        public Coroutine Run(IEnumerator routine) => host.StartCoroutine(routine);

        public RectTransform Button(string label, Vector2 position, Color color, int value, string icon = null, Vector2? size = null) =>
            Ui.TextButton(Root, label, position, size ?? ButtonSize, color, () => Choose(value), icon);

        public RectTransform Card(UpgradeOffer offer, Vector2 position, string footer, int value) =>
            UpgradeCardView.Create(Root, offer, session.Definition.rarity, position, footer, value >= 0 ? () => Choose(value) : null);

        public void Choose(int value)
        {
            FeedbackHub.Ui(SfxId.Click);
            Choice = value;
        }

        public IEnumerator WaitChoice()
        {
            Choice = -1;
            yield return new WaitUntil(() => Choice >= 0);
        }

        public Text BigText(string text, Color color, float y)
        {
            var label = Ui.Label(Ui.Centered("Result", Root, new Vector2(0f, y), new Vector2(1400f, 110f)), text, 70, TextAnchor.MiddleCenter, color);
            Run(Tweens.Pop(label.rectTransform));
            return label;
        }

        public IEnumerator Jackpot(string text, bool mega, float y)
        {
            var feedback = FeedbackHub.Instance;
            feedback.Play(SfxId.Jackpot, 1f, 0f);
            feedback.Shake(mega ? 0.3f : 0.2f, 0.6f);
            var flash = Ui.Image(Ui.Stretch("Flash", Dim), new Color(1f, 0.95f, 0.7f, 0.85f));
            var label = BigText(text, new Color(1f, 0.85f, 0.15f), y);
            label.fontSize = mega ? 92 : 80;
            Pulser.Attach(label);
            Run(CoinShower(mega ? 90 : 50));
            yield return Tweens.Fade(flash, 0.5f);
            Object.Destroy(flash.gameObject);
        }

        IEnumerator CoinShower(int count)
        {
            var layer = Dim;
            for (int i = 0; i < count; i++)
            {
                if (layer == null) yield break;
                var coin = Ui.Icon(layer, "Coin", new Vector2(Random.Range(-200f, 200f), 60f), Random.Range(44f, 70f));
                coin.gameObject.AddComponent<FallingCoin>().Launch(new Vector2(Random.Range(-900f, 900f), Random.Range(500f, 1300f)));
                if (i % 6 == 0) FeedbackHub.Ui(SfxId.Coin, 0.35f);
                yield return new WaitForSecondsRealtime(0.015f);
            }
        }

        public IEnumerator Continue(float y = -400f)
        {
            yield return new WaitForSecondsRealtime(0.6f);
            Button("Continue", new Vector2(0f, y), Ui.GoColor, 0, "Flag");
            yield return WaitChoice();
        }
    }
}
