using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;

namespace Featherguard.Meta
{
    public static class MaterialRewards
    {
        static readonly MaterialRarity[] LandRarities = { MaterialRarity.Common, MaterialRarity.Uncommon, MaterialRarity.Rare };

        public static IReadOnlyList<MaterialStack> Roll(ActivityDefinition activity, float daringMultiplier, MaterialConfig config, IRandomSource random)
        {
            if (activity == null || activity.reward != RewardKind.Materials || config == null) return new List<MaterialStack>();

            var drops = new List<MaterialDefinition>();
            var own = config.materials.Where(m => m != null && m.land == activity.land).ToList();
            int amount = (int)System.Math.Round(Amount(activity.type, config) * daringMultiplier);
            for (int i = 0; i < amount; i++)
            {
                var rarity = WeightedRandom.Pick(LandRarities, r => Weight(r, activity.threat, config), random);
                var options = own.Where(m => m.rarity == rarity).ToList();
                if (options.Count > 0) drops.Add(WeightedRandom.PickUniform(options, random));
            }
            Universal(MaterialRarity.Epic, config.epicChance * daringMultiplier, config, random, drops);
            Universal(MaterialRarity.Legendary, config.legendaryChance * daringMultiplier, config, random, drops);

            return drops.GroupBy(m => m).Select(g => new MaterialStack(g.Key, g.Count())).ToList();
        }

        public static float Weight(MaterialRarity rarity, int threat, MaterialConfig config)
        {
            int above = System.Math.Max(0, threat - 1);
            return rarity switch
            {
                MaterialRarity.Common => config.commonWeight,
                MaterialRarity.Uncommon => config.uncommonWeight + config.uncommonPerThreat * above,
                MaterialRarity.Rare => config.rareWeight + config.rarePerThreat * above,
                _ => 0f,
            };
        }

        static int Amount(ActivityType type, MaterialConfig config)
        {
            int index = (int)type;
            return index < config.amountByType.Length ? config.amountByType[index] : 1;
        }

        static void Universal(MaterialRarity rarity, float chance, MaterialConfig config, IRandomSource random, List<MaterialDefinition> drops)
        {
            if (random.Value >= chance) return;
            var options = config.materials.Where(m => m != null && m.land == null && m.rarity == rarity).ToList();
            if (options.Count > 0) drops.Add(WeightedRandom.PickUniform(options, random));
        }
    }
}
