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
        RectTransform root;
        bool started;

        public static IEnumerator Show(BattleHud hud)
        {
            var camp = hud.gameObject.AddComponent<CampScreen>();
            camp.hud = hud;
            camp.Build();
            yield return new WaitUntil(() => camp.started);
            Destroy(camp.root.gameObject);
            Destroy(camp);
        }

        void Build()
        {
            if (root != null) Destroy(root.gameObject);
            var data = MetaProgress.Data;
            root = Ui.Stretch("Camp", hud.Canvas);
            Ui.Image(root, new Color(0.02f, 0.04f, 0.06f, 0.78f), true);
            Ui.Label(Ui.Rect("Title", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1700f, 120f)),
                "FEATHERGUARD", 110, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.25f));
            Ui.Label(Ui.Rect("Stats", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(1700f, 50f)),
                $"Złote pióra: {data.feathers}   ·   Wyprawy: {data.runs}   ·   Zwycięstwa: {data.wins}   ·   Rekord: {data.bestBattles} walk",
                32, TextAnchor.MiddleCenter, Color.white);

            for (int i = 0; i < MetaProgress.Perks.Length; i++)
            {
                var perk = MetaProgress.Perks[i];
                int level = MetaProgress.Level(perk.Id);
                bool maxed = level >= perk.MaxLevel;
                var position = new Vector2((i % 3 - 1) * 540f, 110f - (i / 3) * 230f);
                var rect = Ui.Rect(perk.Name, root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(500f, 200f));
                var button = Ui.Button(rect, maxed ? new Color(0.3f, 0.5f, 0.3f, 0.95f) : new Color(0.1f, 0.13f, 0.2f, 0.95f), () =>
                {
                    if (!MetaProgress.CanBuy(perk)) return;
                    MetaProgress.Buy(perk);
                    HitFeedback.Instance.Play(SfxId.Coin, 1f, 0f);
                    Build();
                });
                button.interactable = MetaProgress.CanBuy(perk) || maxed;
                Ui.Label(Ui.Rect("Name", rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(480f, 46f)),
                    perk.Name, 38, TextAnchor.MiddleCenter, Color.white);
                Ui.Label(Ui.Rect("Description", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -4f), new Vector2(480f, 40f)),
                    perk.Description, 26, TextAnchor.MiddleCenter, new Color(0.85f, 0.9f, 1f));
                Ui.Label(Ui.Rect("Level", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(480f, 40f)),
                    maxed ? $"Poziom {level}/{perk.MaxLevel} — MAX" : $"Poziom {level}/{perk.MaxLevel}  ·  Koszt: {perk.CostFor(level)} piór",
                    26, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.3f));
            }

            BattleHud.AddButton(root, "Wyrusz!", new Vector2(0f, -400f), new Color(0.75f, 0.2f, 0.12f, 1f), () =>
            {
                HitFeedback.Instance.Play(SfxId.Rage, 1f, 0f);
                started = true;
            }).sizeDelta = new Vector2(520f, 100f);
        }
    }
}
