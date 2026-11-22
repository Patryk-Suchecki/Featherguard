using System;
using System.Collections.Generic;
using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public static class PotionBagView
    {
        const int Columns = 8;
        const float Tile = 150f;
        const float CellWidth = 190f;
        const float CellHeight = 230f;

        static readonly Color PanelColor = new(0.09f, 0.08f, 0.1f, 1f);
        static readonly Color MutedColor = new(0.66f, 0.66f, 0.7f);
        static readonly Color NameColor = new(0.9f, 0.86f, 0.78f);
        static readonly Color ButtonColor = new(0.16f, 0.17f, 0.22f, 1f);

        public static void Build(RectTransform content, IReadOnlyList<PotionStack> owned, Action onClose)
        {
            var panel = Ui.Centered("Potions", content, Vector2.zero, new Vector2(1640f, 920f));
            Ui.Panel(panel, PanelColor, true);
            Ui.Label(Ui.Rect("Title", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(900f, 70f)),
                "Potions", 60, TextAnchor.MiddleCenter, Ui.GoldColor);
            var back = Ui.Rect("Back", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(200f, 76f));
            Ui.Button(back, ButtonColor, () => onClose());
            Ui.Label(Ui.Stretch("Label", back), "Back", 32, TextAnchor.MiddleCenter, Color.white);

            if (owned.Count == 0)
            {
                Ui.Wrapped(Ui.Centered("Empty", panel, new Vector2(0f, -20f), new Vector2(1000f, 120f)),
                    "No potions yet. Brew them in the Wise Woman's Hut.", 34, TextAnchor.MiddleCenter, MutedColor);
                return;
            }

            float left = -(Columns - 1) * CellWidth / 2f;
            for (int i = 0; i < owned.Count; i++)
            {
                var cell = new Vector2(left + i % Columns * CellWidth, 190f - i / Columns * CellHeight);
                var potion = owned[i].Potion;
                ItemTile.Create(panel, potion.name, owned[i].Count, cell + new Vector2(0f, 24f), Tile);
                Ui.Wrapped(Ui.Centered("Name", panel, cell + new Vector2(0f, -84f), new Vector2(CellWidth - 8f, 56f)),
                    potion.displayName, 22, TextAnchor.MiddleCenter, NameColor);
            }
        }
    }
}
