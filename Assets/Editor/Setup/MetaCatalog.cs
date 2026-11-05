using Featherguard.Battle;
using Featherguard.Meta;
using Featherguard.Run;

namespace Featherguard.EditorTools
{
    public static class MetaCatalog
    {
        public static MetaConfig Build(Seeder seeder, PresentationAssets art)
        {
            PerkDefinition Perk(string id, string name, string description, string icon, int maxLevel, int baseCost, PerkEffect effect) =>
                seeder.Seed<PerkDefinition>($"{ContentPaths.Perks}/{id}.asset", p =>
                {
                    p.displayName = name;
                    p.description = description;
                    p.maxLevel = maxLevel;
                    p.baseCost = baseCost;
                    p.effect = effect;
                }, p => p.icon = art.Icon(icon));

            var perks = new[]
            {
                Perk("ToughFeathers", "Tough Feathers", "+6% party max HP", "Heart", 5, 3, new PartyStatPerk { stat = StatType.MaxHealth, multiplyPerLevel = 0.06f }),
                Perk("SharpBeaks", "Sharp Beaks", "+5% party damage", "Sword", 5, 3, new PartyStatPerk { stat = StatType.Damage, multiplyPerLevel = 0.05f }),
                Perk("Horseshoe", "Horseshoe", "+1 starting luck", "Clover", 3, 5, new LuckPerk { perLevel = 1 }),
                Perk("Purse", "Purse", "+15 starting gold", "Coin", 4, 2, new StartingGoldPerk { perLevel = 15 }),
                Perk("Gems", "Gems", "Adds the Gem to the slot reels", "Gem", 1, 8, new UnlockPerk { unlock = RunUnlock.Gems }),
                Perk("Legends", "Legends", "Adds Legendary cards to the pool", "Crown", 1, 10, new UnlockPerk { unlock = RunUnlock.Legendaries }),
            };

            return seeder.Seed<MetaConfig>($"{ContentPaths.Data}/Meta.asset", m => m.perks = perks);
        }
    }
}
