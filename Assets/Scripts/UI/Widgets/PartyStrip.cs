using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Run;
using UnityEngine;

namespace Featherguard.UI
{
    public static class PartyStrip
    {
        static readonly Color HealthFill = new(0.38f, 0.86f, 0.36f);
        static readonly Color LuckColor = new(0.6f, 1f, 0.5f);

        public static void Build(RectTransform parent, RunSession session, VisualsLibrary<UnitDefinition, UnitVisuals> visuals)
        {
            var strip = Ui.Rect("Party", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(330f, 290f));
            var party = session.Party;
            for (int i = 0; i < party.Heroes.Count; i++)
            {
                var hero = party.Heroes[i];
                var row = Ui.Rect(hero.displayName, strip, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -i * 96f), new Vector2(330f, 86f));
                Ui.Sliced(row, "Pill", Ui.PanelColor);
                Portrait.Create(row, visuals.Of(hero).portrait, new Vector2(6f, 0f), 74f, new Vector2(0f, 0.5f));
                int health = party.HealthOf(hero);
                int max = party.MaxHealthOf(hero);
                Ui.Bar(row, new Vector2(40f, -12f), new Vector2(210f, 22f), HealthFill, out var fill);
                Ui.SetFill(fill, (float)health / max);
                Ui.Label(Ui.Rect("Health", row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40f, 18f), new Vector2(210f, 30f)),
                    $"{hero.displayName}  {health}/{max}", 24, TextAnchor.MiddleLeft, Color.white);
            }

            var stats = Ui.Rect("Stats", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-134f, -20f), new Vector2(250f, 160f));
            TopBar.Pill(stats, "Gold", "Coin", Vector2.zero, Ui.GoldColor, 36).text = session.Wallet.Gold.ToString();
            TopBar.Pill(stats, "Luck", "Clover", new Vector2(0f, -84f), LuckColor, 28).text = $"Luck {session.Luck}";
        }
    }
}
