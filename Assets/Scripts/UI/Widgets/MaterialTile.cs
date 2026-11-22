using System.Collections.Generic;
using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public static class MaterialTile
    {
        public static Color RarityColor(MaterialRarity rarity) => rarity switch
        {
            MaterialRarity.Common => new Color(0.82f, 0.82f, 0.82f),
            MaterialRarity.Uncommon => new Color(0.6f, 0.86f, 0.5f),
            MaterialRarity.Rare => new Color(0.55f, 0.74f, 1f),
            MaterialRarity.Epic => new Color(0.8f, 0.62f, 1f),
            _ => new Color(1f, 0.86f, 0.42f),
        };

        public static RectTransform Create(RectTransform parent, MaterialDefinition material, int count, Vector2 position, float size, Vector2? anchor = null) =>
            ItemTile.Create(parent, material.name, count, position, size, anchor);

        public static void Row(RectTransform parent, IReadOnlyList<MaterialStack> stacks, Vector2 centre, float size, float gap = 18f)
        {
            float first = centre.x - (stacks.Count - 1) * (size + gap) / 2f;
            for (int i = 0; i < stacks.Count; i++)
                Create(parent, stacks[i].Material, stacks[i].Count, new Vector2(first + i * (size + gap), centre.y), size);
        }
    }
}
