using System;
using System.Collections.Generic;
using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public static class StorageView
    {
        const int Columns = 8;
        const float Tile = 150f;
        const float CellWidth = 190f;
        const float CellHeight = 210f;

        static readonly Color PanelColor = new(0.09f, 0.08f, 0.1f, 1f);
        static readonly Color MutedColor = new(0.66f, 0.66f, 0.7f);
        static readonly Color ButtonColor = new(0.16f, 0.17f, 0.22f, 1f);

        public static void Build(RectTransform content, IReadOnlyList<MaterialStack> owned, Action onClose)
        {
            var panel = Ui.Centered("Storage", content, Vector2.zero, new Vector2(1640f, 920f));
            Ui.Panel(panel, PanelColor, true);
            Ui.Label(Ui.Rect("Title", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(900f, 70f)),
                "Storage", 60, TextAnchor.MiddleCenter, Ui.GoldColor);
            Ui.Label(Ui.Rect("Tab", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(900f, 40f)),
                "Materials", 30, TextAnchor.MiddleCenter, Color.white);
            var back = Ui.Rect("Back", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(200f, 76f));
            Ui.Button(back, ButtonColor, () => onClose());
            Ui.Label(Ui.Stretch("Label", back), "Back", 32, TextAnchor.MiddleCenter, Color.white);

            if (owned.Count == 0)
            {
                Ui.Wrapped(Ui.Centered("Empty", panel, new Vector2(0f, -20f), new Vector2(1000f, 120f)),
                    "No materials yet. Fights that reward materials fill the Storage.", 34, TextAnchor.MiddleCenter, MutedColor);
                return;
            }

            float left = -(Columns - 1) * CellWidth / 2f;
            for (int i = 0; i < owned.Count; i++)
            {
                var cell = new Vector2(left + i % Columns * CellWidth, 196f - i / Columns * CellHeight);
                var material = owned[i].Material;
                MaterialTile.Create(panel, material, owned[i].Count, cell + new Vector2(0f, 18f), Tile);
                Ui.Label(Ui.Centered("Name", panel, cell + new Vector2(0f, -76f), new Vector2(CellWidth, 34f)),
                    material.displayName, 22, TextAnchor.MiddleCenter, MaterialTile.RarityColor(material.rarity));
            }
        }
    }
}
