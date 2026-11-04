using System;
using System.Linq;
using Featherguard.Core;
using UnityEngine;

namespace Featherguard.Run
{
    [Serializable]
    public class RarityTier
    {
        public Rarity rarity;
        public string label;
        public Color color = Color.white;
        public float weight;
        public float weightPerLuck;
        public float minWeight;
        public bool requiresLegendaryUnlock;

        public float WeightFor(int luck, bool legendariesUnlocked) =>
            requiresLegendaryUnlock && !legendariesUnlocked ? 0f : Mathf.Max(minWeight, weight + weightPerLuck * luck);
    }

    [Serializable]
    public class RarityTable
    {
        public RarityTier[] tiers = new RarityTier[0];

        public RarityTier Tier(Rarity rarity) => tiers.FirstOrDefault(t => t.rarity == rarity) ?? new RarityTier { rarity = rarity, label = rarity.ToString() };

        public Rarity Roll(Rarity minimum, int luck, bool legendariesUnlocked, IRandomSource random)
        {
            var eligible = tiers.Where(t => t.rarity >= minimum && t.WeightFor(luck, legendariesUnlocked) > 0f).ToList();
            if (eligible.Count == 0) return minimum;
            return WeightedRandom.Pick(eligible, t => t.WeightFor(luck, legendariesUnlocked), random).rarity;
        }
    }
}
