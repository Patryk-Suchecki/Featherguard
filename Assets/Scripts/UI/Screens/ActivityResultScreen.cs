using System;
using System.Collections.Generic;
using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public static class ActivityResultScreen
    {
        static readonly Color DefeatColor = new(1f, 0.38f, 0.32f);

        public static void ShowVictory(BattleHud hud, string activity, string reward, string progress, IReadOnlyList<MaterialStack> materials, Action onContinue)
        {
            bool items = materials != null && materials.Count > 0;
            float height = items ? 800f : 640f;
            var box = Open(hud, "Crown", "VICTORY", Ui.GoldColor, activity, height);
            float y = height / 2f - 330f;
            Ui.Label(Ui.Centered("Reward", box, new Vector2(0f, y), new Vector2(1000f, 60f)), reward, 44, TextAnchor.MiddleCenter, Ui.GoldColor);
            if (items)
            {
                y -= 110f;
                MaterialTile.Row(box, materials, new Vector2(0f, y), 112f);
                y -= 40f;
            }
            if (!string.IsNullOrEmpty(progress))
                Ui.Label(Ui.Centered("Progress", box, new Vector2(0f, y - 70f), new Vector2(1000f, 50f)), progress, 34, TextAnchor.MiddleCenter, Color.white);
            Ui.TextButton(box, "World map", new Vector2(0f, -height / 2f + 110f), new Vector2(480f, 120f), Ui.GoColor, () => onContinue());
        }

        public static void ShowDefeat(BattleHud hud, string activity, string rating, Action onRetry, Action onMap)
        {
            var box = Open(hud, "Skull", "DEFEAT", DefeatColor, activity);
            Ui.Label(Ui.Centered("Rating", box, new Vector2(0f, -10f), new Vector2(1000f, 50f)), $"Threat: {rating}   ·   No rewards", 36, TextAnchor.MiddleCenter, Color.white);
            Ui.Label(Ui.Centered("Tip", box, new Vector2(0f, -70f), new Vector2(1000f, 44f)),
                "Try another party, or grow stronger in easier activities first", 28, TextAnchor.MiddleCenter, new Color(0.75f, 0.75f, 0.8f));
            Ui.TextButton(box, "Retry", new Vector2(-260f, -210f), new Vector2(440f, 120f), Ui.RiskColor, () => onRetry());
            Ui.TextButton(box, "World map", new Vector2(260f, -210f), new Vector2(440f, 120f), Ui.NeutralColor, () => onMap());
        }

        static RectTransform Open(BattleHud hud, string icon, string title, Color color, string activity, float height = 640f)
        {
            hud.SetHudVisible(false);
            var root = hud.OpenOverlay("ActivityResult").Content;
            var box = Ui.Centered("Box", root, Vector2.zero, new Vector2(1100f, height));
            Ui.Panel(box, Ui.PanelColor);
            float top = height / 2f;
            Ui.Icon(box, icon, new Vector2(0f, top - 80f), 130f);
            Ui.Label(Ui.Centered("Title", box, new Vector2(0f, top - 190f), new Vector2(1000f, 110f)), title, 86, TextAnchor.MiddleCenter, color);
            Ui.Label(Ui.Centered("Activity", box, new Vector2(0f, top - 260f), new Vector2(1000f, 50f)), activity, 36, TextAnchor.MiddleCenter, Color.white);
            return box;
        }
    }
}
