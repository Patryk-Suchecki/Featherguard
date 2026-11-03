using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle
{
    public static class RunState
    {
        static readonly Dictionary<UnitDefinition, int> health = new();
        static readonly Dictionary<UnitDefinition, List<StatModifier>> modifiers = new();

        public static RunDefinition Run { get; private set; }
        public static bool Active => Run != null;
        public static int EncounterIndex { get; private set; }
        public static int Gold { get; set; }
        public static int BattlesWon { get; private set; }
        public static int Kills { get; set; }
        public static int GoldEarned { get; private set; }

        public static EncounterDefinition Current => Run.encounters[EncounterIndex];
        public static int EncounterCount => Run.encounters.Length;
        public static bool IsFinal => EncounterIndex >= Run.encounters.Length - 1;

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
            health.Clear();
            modifiers.Clear();
        }

        public static void Begin(RunDefinition run)
        {
            Clear();
            Run = run;
        }

        public static void AddGold(int amount)
        {
            Gold += amount;
            GoldEarned += amount;
        }

        public static void Advance() => EncounterIndex++;

        public static void RecordVictory(IEnumerable<BattleUnit> heroes)
        {
            BattlesWon++;
            AddGold(Current.goldReward);
            foreach (var hero in heroes)
            {
                int max = hero.MaxHealth;
                int value = hero.IsAlive
                    ? hero.Health + Mathf.RoundToInt(max * Run.restHeal)
                    : Mathf.RoundToInt(max * Run.reviveHealth);
                health[hero.Definition] = Mathf.Clamp(value, 1, max);
            }
        }

        public static int? StoredHealth(UnitDefinition hero) => health.TryGetValue(hero, out int value) ? value : null;

        public static IReadOnlyList<StatModifier> ModifiersOf(UnitDefinition hero) =>
            modifiers.TryGetValue(hero, out var list) ? list : (IReadOnlyList<StatModifier>)System.Array.Empty<StatModifier>();

        public static void AddModifier(UnitDefinition hero, StatModifier modifier)
        {
            if (!modifiers.TryGetValue(hero, out var list))
                modifiers[hero] = list = new List<StatModifier>();
            list.Add(modifier);
        }
    }
}
