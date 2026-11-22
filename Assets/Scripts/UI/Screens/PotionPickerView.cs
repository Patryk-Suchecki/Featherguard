using System;
using Featherguard.Feedback;
using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public sealed class PotionPickerView
    {
        const int Columns = 3;
        const float CellWidth = 500f;
        const float CellHeight = 132f;

        static readonly Color PanelColor = new(0.09f, 0.08f, 0.1f, 1f);
        static readonly Color CellColor = new(0.14f, 0.13f, 0.16f, 1f);
        static readonly Color PickedColor = new(0.32f, 0.27f, 0.14f, 1f);
        static readonly Color ButtonColor = new(0.16f, 0.17f, 0.22f, 1f);
        static readonly Color TextColor = new(0.9f, 0.86f, 0.78f);
        static readonly Color MutedColor = new(0.66f, 0.66f, 0.7f);

        readonly RectTransform panel;
        readonly PotionBag bag;
        readonly PotionLoadout loadout;
        readonly Action onChanged;
        RectTransform body;

        PotionPickerView(RectTransform panel, PotionBag bag, PotionLoadout loadout, Action onChanged)
        {
            this.panel = panel;
            this.bag = bag;
            this.loadout = loadout;
            this.onChanged = onChanged;
        }

        public static void Build(RectTransform content, PotionBag bag, PotionLoadout loadout, Action onChanged, Action onClose)
        {
            var panel = Ui.Centered("Potions", content, Vector2.zero, new Vector2(1640f, 920f));
            Ui.Panel(panel, PanelColor, true);
            Ui.Label(Ui.Rect("Title", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(900f, 70f)),
                "Potions", 60, TextAnchor.MiddleCenter, Ui.GoldColor);
            var done = Ui.Rect("Done", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(200f, 76f));
            Ui.Button(done, ButtonColor, () => onClose());
            Ui.Label(Ui.Stretch("Label", done), "Done", 32, TextAnchor.MiddleCenter, Color.white);
            new PotionPickerView(panel, bag, loadout, onChanged).Redraw();
        }

        void Redraw()
        {
            if (body != null) UnityEngine.Object.Destroy(body.gameObject);
            body = Ui.Stretch("Body", panel);
            Ui.Label(Ui.Rect("Picked", body, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(900f, 44f)),
                $"Take up to {loadout.Slots} into this battle: {loadout.Total} / {loadout.Slots}", 32, TextAnchor.MiddleCenter, Color.white);

            var owned = bag.Owned;
            if (owned.Count == 0)
            {
                Ui.Wrapped(Ui.Centered("Empty", body, new Vector2(0f, -20f), new Vector2(1000f, 120f)),
                    "No potions yet. Brew them in the Wise Woman's Hut.", 34, TextAnchor.MiddleCenter, MutedColor);
                return;
            }

            float left = -(Columns - 1) * CellWidth / 2f;
            for (int i = 0; i < owned.Count; i++)
                Cell(owned[i], new Vector2(left + i % Columns * CellWidth, 210f - i / Columns * CellHeight));
        }

        void Cell(PotionStack stack, Vector2 position)
        {
            var potion = stack.Potion;
            int picked = loadout.Count(potion);
            var cell = Ui.Centered(potion.name, body, position, new Vector2(CellWidth - 16f, CellHeight - 12f));
            Ui.Panel(cell, picked > 0 ? PickedColor : CellColor);
            ItemTile.Create(cell, potion.name, 0, new Vector2(54f, 0f), 84f, new Vector2(0f, 0.5f));
            Ui.Wrapped(Ui.Rect("Name", cell, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(104f, 14f), new Vector2(186f, 56f)),
                potion.displayName, 22, TextAnchor.MiddleLeft, TextColor);
            Ui.Label(Ui.Rect("Owned", cell, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(104f, -30f), new Vector2(186f, 30f)),
                $"Owned {stack.Count}", 20, TextAnchor.MiddleLeft, MutedColor);
            Step(cell, "−", new Vector2(-150f, 0f), () => Change(loadout.Remove(potion)));
            Ui.Label(Ui.Rect("Count", cell, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-96f, 0f), new Vector2(50f, 60f)),
                picked.ToString(), 36, TextAnchor.MiddleCenter, Color.white);
            Step(cell, "+", new Vector2(-42f, 0f), () => Change(loadout.Add(potion)));
        }

        static void Step(RectTransform parent, string label, Vector2 position, Action onClick)
        {
            var rect = Ui.Rect(label, parent, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(70f, 70f));
            Ui.Button(rect, ButtonColor, () => onClick());
            Ui.Label(Ui.Stretch("Label", rect), label, 40, TextAnchor.MiddleCenter, Color.white);
        }

        void Change(bool changed)
        {
            if (!changed) return;
            FeedbackHub.Ui(SfxId.Click);
            onChanged?.Invoke();
            Redraw();
        }
    }
}
