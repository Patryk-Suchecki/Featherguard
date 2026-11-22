using System.Collections.Generic;
using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public static class MaterialTile
    {
        static readonly Color BadgeColor = new(0.05f, 0.04f, 0.05f, 0.9f);

        public static Color RarityColor(MaterialRarity rarity) => rarity switch
        {
            MaterialRarity.Common => new Color(0.82f, 0.82f, 0.82f),
            MaterialRarity.Uncommon => new Color(0.6f, 0.86f, 0.5f),
            MaterialRarity.Rare => new Color(0.55f, 0.74f, 1f),
            MaterialRarity.Epic => new Color(0.8f, 0.62f, 1f),
            _ => new Color(1f, 0.86f, 0.42f),
        };

        public static RectTransform Create(RectTransform parent, MaterialDefinition material, int count, Vector2 position, float size, Vector2? anchor = null)
        {
            var a = anchor ?? new Vector2(0.5f, 0.5f);
            var tile = Ui.Rect(material.name, parent, a, new Vector2(0.5f, 0.5f), position, Vector2.one * size);
            var icon = Ui.Image(Ui.Stretch("Icon", tile), Color.white);
            icon.sprite = Ui.Sprite(material.name);
            icon.preserveAspect = true;
            if (count > 0)
            {
                var badge = Ui.Rect("Count", tile, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(4f, -4f), new Vector2(size * 0.5f, size * 0.3f));
                Ui.Panel(badge, BadgeColor);
                Ui.Label(Ui.Stretch("Label", badge), $"×{count}", Mathf.RoundToInt(size * 0.2f), TextAnchor.MiddleCenter, Color.white);
            }
            return tile;
        }

        public static void Row(RectTransform parent, IReadOnlyList<MaterialStack> stacks, Vector2 centre, float size, float gap = 18f)
        {
            float first = centre.x - (stacks.Count - 1) * (size + gap) / 2f;
            for (int i = 0; i < stacks.Count; i++)
                Create(parent, stacks[i].Material, stacks[i].Count, new Vector2(first + i * (size + gap), centre.y), size);
        }
    }
}
