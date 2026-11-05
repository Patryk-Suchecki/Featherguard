using System.Collections;
using Featherguard.Feel;
using Featherguard.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class CampScreen
    {
        static readonly Color Maxed = new(0.16f, 0.36f, 0.2f, 0.95f);
        static readonly Color Affordable = new(0.5f, 0.35f, 0.05f, 1f);
        static readonly Color Locked = new(0.2f, 0.2f, 0.25f, 1f);

        readonly BattleHud hud;
        readonly MetaProgression meta;
        Overlay overlay;
        bool started;

        CampScreen(BattleHud hud, MetaProgression meta)
        {
            this.hud = hud;
            this.meta = meta;
        }

        public static IEnumerator Show(BattleHud hud, MetaProgression meta)
        {
            var camp = new CampScreen(hud, meta);
            hud.SetHudVisible(false);
            camp.Build();
            yield return new WaitUntil(() => camp.started);
            camp.overlay.Close();
            hud.SetHudVisible(true);
        }

        void Build()
        {
            overlay?.Close();
            overlay = hud.OpenOverlay("Camp", 0.55f);
            var root = overlay.Content;
            var data = meta.Data;

            Ui.Label(Ui.Rect("Title", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(1200f, 120f)),
                "FEATHERGUARD", 104, TextAnchor.MiddleCenter, Ui.GoldColor);
            Ui.Label(Ui.Rect("Stats", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(1200f, 44f)),
                $"Runs: {data.runs}   ·   Wins: {data.wins}   ·   Best: {data.bestBattles} battles", 30, TextAnchor.MiddleCenter, Color.white);
            TopBar.Pill(root, "Feathers", "Feather", new Vector2(-134f, -20f), Ui.GoldColor).text = data.feathers.ToString();

            for (int i = 0; i < meta.Perks.Count; i++)
                PerkCard(root, meta.Perks[i], new Vector2((i % 3 - 1) * 560f, 150f - (i / 3) * 250f));

            var go = Ui.TextButton(root, "Set out!", new Vector2(0f, -400f), new Vector2(560f, 140f), Ui.RiskColor, () =>
            {
                FeedbackHub.Ui(SfxId.Rage);
                started = true;
            }, "Flag");
            Pulser.Attach(go, 5f, 0.04f);
        }

        void PerkCard(RectTransform root, PerkDefinition perk, Vector2 position)
        {
            int level = meta.Level(perk);
            bool maxed = meta.IsMaxed(perk);
            bool affordable = meta.CanBuy(perk);
            var rect = Ui.Centered(perk.displayName, root, position, new Vector2(530f, 230f));
            var button = Ui.Button(rect, maxed ? Maxed : Ui.PanelColor, () =>
            {
                if (!meta.Buy(perk)) return;
                FeedbackHub.Ui(SfxId.Coin);
                Build();
            });
            button.interactable = affordable || maxed;
            if (affordable)
                Ui.Sliced(Ui.Stretch("Frame", rect, -2f), "Frame", Ui.GoldColor);

            var icon = Ui.Icon(rect, "Card", new Vector2(80f, 0f), 130f, anchor: new Vector2(0f, 0.5f));
            if (perk.icon != null) icon.sprite = perk.icon;
            Ui.Label(Ui.Rect("Name", rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(160f, -20f), new Vector2(350f, 46f)),
                perk.displayName, 38, TextAnchor.UpperLeft, Color.white);
            Ui.Wrapped(Ui.Rect("Description", rect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(160f, 4f), new Vector2(350f, 70f)),
                perk.description, 26, TextAnchor.MiddleLeft, new Color(0.85f, 0.9f, 1f));

            var footer = Ui.Rect("Level", rect, Vector2.zero, Vector2.zero, new Vector2(160f, 16f), new Vector2(350f, 44f));
            if (maxed)
            {
                Ui.Label(footer, $"Level {level}/{perk.maxLevel} · MAX", 28, TextAnchor.MiddleLeft, new Color(0.6f, 1f, 0.5f));
                return;
            }
            Ui.Label(footer, $"Lv. {level}/{perk.maxLevel}", 28, TextAnchor.MiddleLeft, Color.white);
            var cost = Ui.Rect("Cost", rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 14f), new Vector2(150f, 48f));
            Ui.Sliced(cost, "Pill", affordable ? Affordable : Locked);
            Ui.Icon(cost, "Feather", new Vector2(28f, 0f), 38f, anchor: new Vector2(0f, 0.5f));
            Ui.Label(Ui.Stretch("Label", cost), meta.Cost(perk).ToString(), 30, TextAnchor.MiddleCenter, Color.white).rectTransform.offsetMin = new Vector2(40f, 0f);
        }
    }
}
