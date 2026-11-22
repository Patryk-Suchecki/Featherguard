using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class PotionBeltView
    {
        const float Slot = 132f;
        const float Gap = 12f;
        const float Spent = 0.35f;

        static readonly Color TileColor = new(0.07f, 0.08f, 0.13f, 0.85f);
        static readonly Color BadgeColor = new(0.05f, 0.04f, 0.05f, 0.9f);

        readonly IPotionCommands commands;
        readonly PotionPouch pouch;
        readonly List<(PotionDefinition Potion, CanvasGroup Group, Text Count)> slots = new();

        public PotionBeltView(RectTransform parent, IPotionCommands commands, PotionPouch pouch)
        {
            this.commands = commands;
            this.pouch = pouch;
            if (pouch.Potions.Count == 0) return;

            float width = pouch.Potions.Count * (Slot + Gap) + Gap;
            var tile = Ui.Rect("Potions", parent, Vector2.zero, Vector2.zero, new Vector2(24f, 24f), new Vector2(width, Slot + Gap * 2f));
            Ui.Panel(tile, TileColor);
            for (int i = 0; i < pouch.Potions.Count; i++)
            {
                var potion = pouch.Potions[i];
                var rect = Ui.Rect(potion.name, tile, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(Gap + i * (Slot + Gap), 0f), Vector2.one * Slot);
                Ui.Image(rect, Color.clear, true);
                var icon = Ui.Image(Ui.Stretch("Icon", rect), Color.white);
                icon.sprite = Ui.Sprite(potion.name);
                icon.preserveAspect = true;
                var badge = Ui.Rect("Count", rect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(4f, -4f), new Vector2(Slot * 0.46f, Slot * 0.3f));
                Ui.Panel(badge, BadgeColor);
                var count = Ui.Label(Ui.Stretch("Label", badge), "", Mathf.RoundToInt(Slot * 0.2f), TextAnchor.MiddleCenter, Color.white);
                rect.gameObject.AddComponent<PotionSlot>().Init(commands, potion);
                slots.Add((potion, rect.gameObject.AddComponent<CanvasGroup>(), count));
            }
        }

        public void Update()
        {
            foreach (var (potion, group, count) in slots)
            {
                int left = pouch.Count(potion);
                count.text = $"×{left}";
                group.alpha = left > 0 && commands.CanUsePotions ? 1f : Spent;
            }
        }
    }
}
