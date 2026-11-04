using System.Collections;
using Featherguard.Battle;
using Featherguard.Feel;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class CampScreen : MonoBehaviour
    {
        BattleHud hud;
        RectTransform dim;
        bool started;

        public static IEnumerator Show(BattleHud hud)
        {
            var camp = hud.gameObject.AddComponent<CampScreen>();
            camp.hud = hud;
            hud.SetHudVisible(false);
            camp.Build();
            yield return new WaitUntil(() => camp.started);
            hud.SetHudVisible(true);
            Destroy(camp.dim.gameObject);
            Destroy(camp);
        }

        static string IconFor(PerkId id) => id switch
        {
            PerkId.ToughFeathers => "Heart",
            PerkId.SharpBeaks => "Sword",
            PerkId.Horseshoe => "Clover",
            PerkId.Purse => "Coin",
            PerkId.Gems => "Gem",
            _ => "Crown",
        };

        void Build()
        {
            if (dim != null) Destroy(dim.gameObject);
            var data = MetaProgress.Data;
            var root = hud.Overlay("Camp", out dim, 0.55f);
            dim.SetSiblingIndex(hud.Safe.GetSiblingIndex() + 1);

            Ui.Label(Ui.Rect("Title", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(1200f, 120f)),
                "FEATHERGUARD", 104, TextAnchor.MiddleCenter, Ui.GoldColor);
            Ui.Label(Ui.Rect("Stats", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(1200f, 44f)),
                $"Wyprawy: {data.runs}   ·   Zwycięstwa: {data.wins}   ·   Rekord: {data.bestBattles} walk", 30, TextAnchor.MiddleCenter, Color.white);

            var feathers = Ui.Rect("Feathers", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-134f, -20f), new Vector2(260f, 76f));
            Ui.Sliced(feathers, "Pill", Ui.PanelColor);
            Ui.Icon(feathers, "Feather", new Vector2(42f, 0f), 58f, anchor: new Vector2(0f, 0.5f));
            Ui.Label(Ui.Stretch("Label", feathers), data.feathers.ToString(), 38, TextAnchor.MiddleCenter, Ui.GoldColor).rectTransform.offsetMin = new Vector2(70f, 0f);

            for (int i = 0; i < MetaProgress.Perks.Length; i++)
            {
                var perk = MetaProgress.Perks[i];
                int level = MetaProgress.Level(perk.Id);
                bool maxed = level >= perk.MaxLevel;
                bool affordable = MetaProgress.CanBuy(perk);
                var rect = Ui.Centered(perk.Name, root, new Vector2((i % 3 - 1) * 560f, 150f - (i / 3) * 250f), new Vector2(530f, 230f));
                var button = Ui.Button(rect, maxed ? new Color(0.16f, 0.36f, 0.2f, 0.95f) : Ui.PanelColor, () =>
                {
                    if (!MetaProgress.CanBuy(perk)) return;
                    MetaProgress.Buy(perk);
                    HitFeedback.Instance.Play(SfxId.Coin, 1f, 0f);
                    Build();
                });
                button.interactable = affordable || maxed;
                if (affordable)
                    Ui.Sliced(Ui.Stretch("Frame", rect, -2f), "Frame", Ui.GoldColor);
                Ui.Icon(rect, IconFor(perk.Id), new Vector2(80f, 0f), 130f, anchor: new Vector2(0f, 0.5f));
                Ui.Label(Ui.Rect("Name", rect, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(160f, -20f), new Vector2(350f, 46f)),
                    perk.Name, 38, TextAnchor.UpperLeft, Color.white);
                Ui.Wrapped(Ui.Rect("Description", rect, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(160f, 4f), new Vector2(350f, 70f)),
                    perk.Description, 26, TextAnchor.MiddleLeft, new Color(0.85f, 0.9f, 1f));
                var footer = Ui.Rect("Level", rect, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(160f, 16f), new Vector2(350f, 44f));
                if (maxed)
                    Ui.Label(footer, $"Poziom {level}/{perk.MaxLevel} · MAX", 28, TextAnchor.MiddleLeft, new Color(0.6f, 1f, 0.5f));
                else
                {
                    Ui.Label(footer, $"Poz. {level}/{perk.MaxLevel}   ", 28, TextAnchor.MiddleLeft, Color.white);
                    var cost = Ui.Rect("Cost", rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 14f), new Vector2(150f, 48f));
                    Ui.Sliced(cost, "Pill", affordable ? new Color(0.5f, 0.35f, 0.05f, 1f) : new Color(0.2f, 0.2f, 0.25f, 1f));
                    Ui.Icon(cost, "Feather", new Vector2(28f, 0f), 38f, anchor: new Vector2(0f, 0.5f));
                    Ui.Label(Ui.Stretch("Label", cost), perk.CostFor(level).ToString(), 30, TextAnchor.MiddleCenter, Color.white).rectTransform.offsetMin = new Vector2(40f, 0f);
                }
            }

            var go = Ui.TextButton(root, "Wyrusz!", new Vector2(0f, -400f), new Vector2(560f, 140f), Ui.RiskColor, () =>
            {
                HitFeedback.Instance.Play(SfxId.Rage, 1f, 0f);
                started = true;
            }, "Flag");
            go.gameObject.AddComponent<Pulser>();
        }
    }

    public class Pulser : MonoBehaviour
    {
        void Update() => transform.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.04f);
    }
}
