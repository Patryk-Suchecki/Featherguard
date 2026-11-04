using System;
using Featherguard.Run;
using UnityEngine;

namespace Featherguard.UI
{
    public static class RunEndScreen
    {
        static readonly Color DefeatColor = new(1f, 0.38f, 0.32f);

        public static void Show(BattleHud hud, RunSession session, bool victory, int feathers, Action onContinue)
        {
            hud.SetHudVisible(false);
            var root = hud.OpenOverlay("RunEnd").Content;
            var box = Ui.Centered("Box", root, Vector2.zero, new Vector2(1100f, 640f));
            Ui.Panel(box, Ui.PanelColor);
            Ui.Icon(box, victory ? "Crown" : "Skull", new Vector2(0f, 230f), 150f);
            Ui.Label(Ui.Centered("Title", box, new Vector2(0f, 100f), new Vector2(1000f, 110f)),
                victory ? "WYPRAWA UKOŃCZONA!" : "PORAŻKA", 86, TextAnchor.MiddleCenter, victory ? Ui.GoldColor : DefeatColor);
            Ui.Label(Ui.Centered("Stats", box, new Vector2(0f, 10f), new Vector2(1000f, 50f)),
                $"Walki: {session.BattlesWon}/{session.EncounterCount}   ·   Wrogowie: {session.Kills}   ·   Złoto: {session.Wallet.Earned}   ·   Karty: {session.Taken.Count}",
                32, TextAnchor.MiddleCenter, Color.white);
            var reward = Ui.Centered("Feathers", box, new Vector2(0f, -70f), new Vector2(420f, 80f));
            Ui.Icon(reward, "Feather", new Vector2(-150f, 0f), 72f);
            Ui.Label(Ui.Stretch("Label", reward), $"+{feathers} piór", 48, TextAnchor.MiddleCenter, Ui.GoldColor);
            Ui.TextButton(box, "Do obozu", new Vector2(0f, -210f), new Vector2(480f, 120f), Ui.GoColor, () => onContinue(), "Campfire");
        }
    }
}
