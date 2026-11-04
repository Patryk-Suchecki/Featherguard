using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Run;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class RunCatalog
    {
        public static RunDefinition Build(Seeder seeder, IReadOnlyDictionary<string, UnitDefinition> units, PresentationAssets art)
        {
            var (upgrades, curses) = UpgradeCatalog.Build(seeder, art);
            var encounters = Encounters(seeder, units);
            var economy = seeder.Seed<EconomyConfig>($"{ContentPaths.Data}/Economy.asset", null);
            var slots = SlotMachine(seeder, art);

            return seeder.Seed<RunDefinition>($"{ContentPaths.Data}/Run.asset", run =>
            {
                run.rarity = RarityTiers();
                run.pathChoices = 2;
                run.pathNodes = new[] { Node(PathNodeType.Normal, art), Node(PathNodeType.Elite, art), Node(PathNodeType.Shop, art), Node(PathNodeType.Chest, art), Node(PathNodeType.Campfire, art) };
                run.beforeBossNodes = new[] { Node(PathNodeType.Shop, art), Node(PathNodeType.Campfire, art) };
                run.encounters = encounters;
                run.upgrades = upgrades;
                run.curses = curses;
            }, run =>
            {
                run.economy = economy;
                run.slots = slots;
                foreach (var node in run.pathNodes.Concat(run.beforeBossNodes).Where(n => n.icon == null))
                    node.icon = art.Icon(NodeIcon(node.type));
            });
        }

        static EncounterDefinition[] Encounters(Seeder seeder, IReadOnlyDictionary<string, UnitDefinition> u)
        {
            var g = u["Grunt"];
            var s = u["Shaman"];
            var b = u["Brute"];
            var c = u["Chieftain"];
            var p = u["Spitter"];
            var t = u["Guard"];

            EncounterDefinition Add(string id, string name, float health, float damage, int gold, bool boss, params UnitDefinition[][] waves) =>
                seeder.Seed<EncounterDefinition>($"{ContentPaths.Encounters}/{id}.asset", e =>
                {
                    e.displayName = name;
                    e.enemyHealth = health;
                    e.enemyDamage = damage;
                    e.goldReward = gold;
                    e.boss = boss;
                    e.waves = waves.Select(w => new EncounterDefinition.Wave { enemies = w }).ToArray();
                });

            return new[]
            {
                Add("Battle01", "Skraj lasu", 1f, 1f, 10, false, new[] { g, g }),
                Add("Battle02", "Leśna ścieżka", 1f, 1f, 12, false, new[] { g, s, g }),
                Add("Battle03", "Stary dąb", 1f, 1f, 14, false, new[] { g, g }, new[] { s, g }),
                Add("Battle04", "Wykrot", 1.05f, 1f, 16, false, new[] { g, b, p }),
                Add("Battle05", "Mglisty jar", 1.1f, 1.05f, 18, false, new[] { p, t, s }, new[] { g, b }),
                Add("Battle06", "Bagienko", 1.15f, 1.1f, 20, false, new[] { p, s, p }, new[] { g, t, g }),
                Add("Battle07", "Kamienny bród", 1.2f, 1.1f, 22, false, new[] { b, t, b }, new[] { s, p }),
                Add("Battle08", "Obóz jaszczurów", 1.3f, 1.15f, 25, false, new[] { t, s, b }, new[] { p, b, g }),
                Add("Battle09", "Herszt", 1.3f, 1.15f, 40, true, new[] { g, t, g }, new[] { p, c, s }),
            };
        }

        static SlotConfig SlotMachine(Seeder seeder, PresentationAssets art) =>
            seeder.Seed<SlotConfig>($"{ContentPaths.Data}/SlotMachine.asset", config =>
            {
                config.symbols = new[]
                {
                    Symbol("Coin", "ZŁOTO", new Color(1f, 0.78f, 0.15f), 30f, 0f, 0f,
                        new GoldReward { amount = 20, text = "+20 zł" }, new GoldReward { amount = 60, text = "+60 zł" }),
                    Symbol("Heart", "SERCE", new Color(0.95f, 0.3f, 0.35f), 20f, 0f, 0f,
                        new HealReward { fraction = 0.25f, text = "Leczenie +25%" }, new HealReward { fraction = 1f, text = "Pełne leczenie" }),
                    Symbol("Card", "KARTA", new Color(0.3f, 0.55f, 1f), 18f, 1f, 0f,
                        new CardReward { minimum = Run.Rarity.Common, text = "Dodatkowa karta" }, new CardReward { minimum = Run.Rarity.Epic, text = "Epicka karta" }),
                    Symbol("Rage", "FURIA", new Color(1f, 0.45f, 0.12f), 14f, 0f, 0f,
                        new StartRageReward { amount = 40f, text = "Furia +40 na start" }, new StartRageReward { amount = 100f, text = "Pełna Furia" }),
                    Symbol("Skull", "PECH", new Color(0.35f, 0.3f, 0.4f), 13f, -2f, 3f,
                        new PenaltyReward { goldLoss = 10, text = "Pech... -10 zł" }, new PenaltyReward { healthLoss = 0.15f, goldLoss = 20, text = "PECH! -15% HP, -20 zł" }),
                    Symbol("Gem", "KLEJNOT", new Color(0.3f, 0.95f, 0.95f), 6f, 1f, 0f,
                        new GoldReward { amount = 50, text = "+50 zł" }, new GoldReward { amount = 150, text = "+150 zł" }, RunUnlock.Gems, true),
                };
                config.matchChance = 0.3f;
                config.matchChancePerLuck = 0.04f;
                config.maxMatchChance = 0.6f;
                config.noMatch = new GoldReward { amount = 5, text = "Nic... +5 zł" };
                config.jackpotPrefix = "JACKPOT! ";
                config.megaJackpotPrefix = "MEGA JACKPOT! ";
            }, config =>
            {
                foreach (var symbol in config.symbols)
                    symbol.icon = art.Icon(symbol.id) ?? symbol.icon;
            });

        static SlotSymbol Symbol(string id, string label, Color tint, float weight, float perLuck, float minWeight, SlotReward pair, SlotReward triple,
            RunUnlock requires = RunUnlock.None, bool mega = false) => new()
        {
            id = id,
            label = label,
            tint = tint,
            weight = weight,
            weightPerLuck = perLuck,
            minWeight = minWeight,
            pair = pair,
            triple = triple,
            requires = requires,
            megaJackpot = mega,
        };

        static RarityTable RarityTiers() => new()
        {
            tiers = new[]
            {
                new RarityTier { rarity = Run.Rarity.Common, label = "ZWYKŁA", color = new Color(0.6f, 0.65f, 0.7f), weight = 60f, weightPerLuck = -6f, minWeight = 10f },
                new RarityTier { rarity = Run.Rarity.Rare, label = "RZADKA", color = new Color(0.25f, 0.55f, 1f), weight = 28f, weightPerLuck = 3f },
                new RarityTier { rarity = Run.Rarity.Epic, label = "EPICKA", color = new Color(0.7f, 0.3f, 1f), weight = 10f, weightPerLuck = 2f },
                new RarityTier { rarity = Run.Rarity.Legendary, label = "LEGENDARNA", color = new Color(1f, 0.7f, 0.1f), weight = 2f, weightPerLuck = 1f, requiresLegendaryUnlock = true },
            },
        };

        static PathNode Node(PathNodeType type, PresentationAssets art) => type switch
        {
            PathNodeType.Elite => new PathNode { type = type, title = "Elita", description = "Silniejsi wrogowie, podwójne złoto i lepsza karta", color = new Color(0.55f, 0.14f, 0.14f), icon = art.Icon("Elite") },
            PathNodeType.Shop => new PathNode { type = type, title = "Sklep", description = "Kup karty za złoto albo zapłać krwią", color = new Color(0.5f, 0.4f, 0.1f), icon = art.Icon("Shop") },
            PathNodeType.Chest => new PathNode { type = type, title = "Skrzynia", description = "Epicka nagroda... i klątwa w pakiecie", color = new Color(0.38f, 0.14f, 0.48f), icon = art.Icon("Chest") },
            PathNodeType.Campfire => new PathNode { type = type, title = "Ognisko", description = "Drużyna leczy 35% HP", color = new Color(0.18f, 0.42f, 0.24f), icon = art.Icon("Campfire") },
            _ => new PathNode { type = type, title = "Ścieżka", description = "Zwykła walka", color = new Color(0.26f, 0.32f, 0.44f), icon = art.Icon("Sword") },
        };

        static string NodeIcon(PathNodeType type) => type switch
        {
            PathNodeType.Elite => "Elite",
            PathNodeType.Shop => "Shop",
            PathNodeType.Chest => "Chest",
            PathNodeType.Campfire => "Campfire",
            _ => "Sword",
        };
    }
}
