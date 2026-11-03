using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Featherguard.Battle
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

        public string Target => Upgrade.scope switch
        {
            UpgradeScope.Hero => Hero.displayName,
            UpgradeScope.Party => "Cała drużyna",
            _ => "Wyprawa",
        };
    }

    public static class RunState
    {
        static readonly Dictionary<UnitDefinition, int> health = new();
        static readonly Dictionary<UnitDefinition, List<StatModifier>> modifiers = new();
        static readonly List<UpgradeDefinition> taken = new();
        static UnitDefinition[] heroes = new UnitDefinition[0];

        public static RunDefinition Run { get; private set; }
        public static bool Active => Run != null;
        public static int EncounterIndex { get; private set; }
        public static int Gold { get; set; }
        public static int BattlesWon { get; private set; }
        public static int Kills { get; set; }
        public static int GoldEarned { get; private set; }
        public static int Luck { get; set; }
        public static float GoldGain { get; set; } = 1f;
        public static float StartRage { get; set; }
        public static bool NextElite { get; set; }
        public static bool CurrentElite { get; private set; }
        public static bool GemsUnlocked { get; set; }
        public static bool LegendariesUnlocked { get; set; }
        public static IReadOnlyList<UpgradeDefinition> Taken => taken;
        public static IReadOnlyList<UnitDefinition> Heroes => heroes;

        public static EncounterDefinition Current => Run.encounters[EncounterIndex];
        public static int EncounterCount => Run.encounters.Length;
        public static bool IsFinal => EncounterIndex >= Run.encounters.Length - 1;
        public static bool NextIsFinal => EncounterIndex + 1 >= Run.encounters.Length - 1;
        public static int CurrentGoldReward => Current.goldReward * (CurrentElite ? 2 : 1);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLoad() => Clear();

        static void Clear()
        {
            Run = null;
            EncounterIndex = 0;
            Gold = 0;
            BattlesWon = 0;
            Kills = 0;
            GoldEarned = 0;
            Luck = 0;
            GoldGain = 1f;
            StartRage = 0f;
            NextElite = false;
            CurrentElite = false;
            GemsUnlocked = false;
            LegendariesUnlocked = false;
            health.Clear();
            modifiers.Clear();
            taken.Clear();
        }

        public static void Begin(RunDefinition run, UnitDefinition[] party)
        {
            Clear();
            Run = run;
            heroes = party;
            MetaProgress.ApplyToRun();
        }

        public static void End() => Clear();

        public static void AddGold(int amount)
        {
            if (amount > 0)
            {
                amount = Mathf.RoundToInt(amount * GoldGain);
                GoldEarned += amount;
            }
            Gold = Mathf.Max(0, Gold + amount);
        }

        public static void Advance()
        {
            EncounterIndex++;
            CurrentElite = NextElite;
            NextElite = false;
        }

        public static void RecordVictory(IEnumerable<BattleUnit> party)
        {
            BattlesWon++;
            AddGold(CurrentGoldReward);
            foreach (var hero in party)
            {
                int max = hero.MaxHealth;
                int value = hero.IsAlive
                    ? hero.Health + Mathf.RoundToInt(max * Run.restHeal)
                    : Mathf.RoundToInt(max * Run.reviveHealth);
                health[hero.Definition] = Mathf.Clamp(value, 1, max);
            }
        }

        public static int MaxHealthOf(UnitDefinition hero)
        {
            float add = 0f, multiply = 1f;
            foreach (var modifier in ModifiersOf(hero).Where(m => m.stat == StatType.MaxHealth))
            {
                add += modifier.add;
                multiply += modifier.multiply;
            }
            return Mathf.Max(1, Mathf.RoundToInt((hero.maxHealth + add) * Mathf.Max(0f, multiply)));
        }

        public static int HealthOf(UnitDefinition hero) => health.TryGetValue(hero, out int value) ? value : MaxHealthOf(hero);

        public static int? StoredHealth(UnitDefinition hero) => health.TryGetValue(hero, out int value) ? value : null;

        public static void HealParty(float fraction)
        {
            foreach (var hero in heroes)
                health[hero] = Mathf.Min(MaxHealthOf(hero), HealthOf(hero) + Mathf.RoundToInt(MaxHealthOf(hero) * fraction));
        }

        public static void HurtParty(float fraction)
        {
            foreach (var hero in heroes)
                health[hero] = Mathf.Max(1, HealthOf(hero) - Mathf.RoundToInt(HealthOf(hero) * fraction));
        }

        public static IReadOnlyList<StatModifier> ModifiersOf(UnitDefinition hero) =>
            modifiers.TryGetValue(hero, out var list) ? list : (IReadOnlyList<StatModifier>)System.Array.Empty<StatModifier>();

        public static void AddModifier(UnitDefinition hero, StatModifier modifier)
        {
            int before = MaxHealthOf(hero);
            int current = HealthOf(hero);
            if (!modifiers.TryGetValue(hero, out var list))
                modifiers[hero] = list = new List<StatModifier>();
            list.Add(modifier);
            int after = MaxHealthOf(hero);
            health[hero] = Mathf.Clamp(current + Mathf.Max(0, after - before), 1, after);
        }

        public static void Apply(UpgradeOffer offer)
        {
            var upgrade = offer.Upgrade;
            taken.Add(upgrade);
            var targets = upgrade.scope switch
            {
                UpgradeScope.Hero => new[] { offer.Hero },
                UpgradeScope.Party => heroes,
                _ => System.Array.Empty<UnitDefinition>(),
            };
            foreach (var hero in targets)
                foreach (var modifier in upgrade.modifiers)
                    AddModifier(hero, new StatModifier(modifier.stat, modifier.add, modifier.multiply));

            switch (upgrade.runBonus)
            {
                case RunBonus.Luck: Luck += Mathf.RoundToInt(upgrade.runAmount); break;
                case RunBonus.GoldGain: GoldGain += upgrade.runAmount; break;
            }
        }

        public static Rarity RollRarity(Rarity minimum = Rarity.Common)
        {
            float[] weights =
            {
                Mathf.Max(10f, 60f - Luck * 6f),
                28f + Luck * 3f,
                10f + Luck * 2f,
                LegendariesUnlocked ? 2f + Luck : 0f,
            };
            for (int i = 0; i < (int)minimum; i++)
                weights[i] = 0f;
            float roll = Random.value * weights.Sum();
            for (int i = 0; i < weights.Length; i++)
            {
                roll -= weights[i];
                if (roll <= 0f && weights[i] > 0f) return (Rarity)i;
            }
            return LegendariesUnlocked ? Rarity.Legendary : Rarity.Epic;
        }

        public static List<UpgradeOffer> Offer(int count, Rarity minimum = Rarity.Common)
        {
            var offers = new List<UpgradeOffer>();
            var pool = Run.upgrades.Where(u => !u.curse).ToList();
            for (int attempt = 0; offers.Count < count && attempt < 50; attempt++)
            {
                var rarity = RollRarity(minimum);
                var candidates = pool.Where(u => u.rarity == rarity && offers.All(o => o.Upgrade != u)).ToList();
                if (candidates.Count == 0) continue;
                var upgrade = candidates[Random.Range(0, candidates.Count)];
                offers.Add(new UpgradeOffer(upgrade, heroes[Random.Range(0, heroes.Length)]));
            }
            return offers;
        }

        public static UpgradeOffer RandomCurse()
        {
            var curses = Run.upgrades.Where(u => u.curse).ToList();
            return new UpgradeOffer(curses[Random.Range(0, curses.Count)], heroes[Random.Range(0, heroes.Length)]);
        }
    }
}
