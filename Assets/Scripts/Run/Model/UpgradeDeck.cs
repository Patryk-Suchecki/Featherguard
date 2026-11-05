using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Core;

namespace Featherguard.Run
{
    public readonly struct UpgradeOffer
    {
        public readonly UpgradeDefinition Upgrade;
        public readonly UnitDefinition Hero;

        public UpgradeOffer(UpgradeDefinition upgrade, UnitDefinition hero)
        {
            Upgrade = upgrade;
            Hero = hero;
        }

        public bool IsValid => Upgrade != null;

        public string Target => Upgrade.scope switch
        {
            UpgradeScope.Hero => Hero.displayName,
            UpgradeScope.Party => "Whole party",
            _ => "Run",
        };
    }

    public sealed class UpgradeDeck
    {
        const int MaxAttempts = 50;

        readonly IReadOnlyList<UpgradeDefinition> pool;
        readonly IReadOnlyList<UpgradeDefinition> curses;
        readonly RarityTable rarity;
        readonly IRandomSource random;

        public UpgradeDeck(IReadOnlyList<UpgradeDefinition> pool, IReadOnlyList<UpgradeDefinition> curses, RarityTable rarity, IRandomSource random)
        {
            this.pool = pool;
            this.curses = curses;
            this.rarity = rarity;
            this.random = random;
        }

        public List<UpgradeOffer> Draw(int count, Rarity minimum, int luck, bool legendariesUnlocked, IReadOnlyList<UnitDefinition> heroes)
        {
            var offers = new List<UpgradeOffer>();
            for (int attempt = 0; offers.Count < count && attempt < MaxAttempts; attempt++)
            {
                var tier = rarity.Roll(minimum, luck, legendariesUnlocked, random);
                var candidates = pool.Where(u => u.rarity == tier && offers.All(o => o.Upgrade != u)).ToList();
                if (candidates.Count == 0) continue;
                offers.Add(new UpgradeOffer(WeightedRandom.PickUniform(candidates, random), WeightedRandom.PickUniform(heroes, random)));
            }
            return offers;
        }

        public UpgradeOffer DrawCurse(IReadOnlyList<UnitDefinition> heroes) =>
            curses.Count == 0 ? default : new UpgradeOffer(WeightedRandom.PickUniform(curses, random), WeightedRandom.PickUniform(heroes, random));
    }
}
