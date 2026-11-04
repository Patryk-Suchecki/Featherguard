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
                Perk("ToughFeathers", "Twarde pióra", "+6% max HP drużyny", "Heart", 5, 3, new PartyStatPerk { stat = StatType.MaxHealth, multiplyPerLevel = 0.06f }),
                Perk("SharpBeaks", "Ostre dzioby", "+5% obrażeń drużyny", "Sword", 5, 3, new PartyStatPerk { stat = StatType.Damage, multiplyPerLevel = 0.05f }),
                Perk("Horseshoe", "Podkowa", "+1 szczęścia na start", "Clover", 3, 5, new LuckPerk { perLevel = 1 }),
                Perk("Purse", "Sakiewka", "+15 zł na start", "Coin", 4, 2, new StartingGoldPerk { perLevel = 15 }),
                Perk("Gems", "Klejnoty", "Klejnot na bębnach automatu", "Gem", 1, 8, new UnlockPerk { unlock = RunUnlock.Gems }),
                Perk("Legends", "Legendy", "Legendarne karty w puli", "Crown", 1, 10, new UnlockPerk { unlock = RunUnlock.Legendaries }),
            };

            return seeder.Seed<MetaConfig>($"{ContentPaths.Data}/Meta.asset", m => m.perks = perks);
        }
    }
}
