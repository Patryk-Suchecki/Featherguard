using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using UnityEngine;

namespace Featherguard.Run
{
    public sealed class Party
    {
        readonly Dictionary<UnitDefinition, int> health = new();
        readonly Dictionary<UnitDefinition, List<StatModifier>> modifiers = new();

        public IReadOnlyList<UnitDefinition> Heroes { get; }

        public Party(IReadOnlyList<UnitDefinition> heroes) => Heroes = heroes;

        public IReadOnlyList<StatModifier> ModifiersOf(UnitDefinition hero) =>
            modifiers.TryGetValue(hero, out var list) ? list : (IReadOnlyList<StatModifier>)System.Array.Empty<StatModifier>();

        public int MaxHealthOf(UnitDefinition hero)
        {
            var relevant = ModifiersOf(hero).Where(m => m.stat == StatType.MaxHealth).ToList();
            float multiplier = 1f + relevant.Sum(m => m.multiply);
            return Mathf.Max(1, Mathf.RoundToInt((hero.maxHealth + relevant.Sum(m => m.add)) * Mathf.Max(0f, multiplier)));
        }

        public int HealthOf(UnitDefinition hero) => health.TryGetValue(hero, out int value) ? value : MaxHealthOf(hero);

        public void AddModifier(UnitDefinition hero, StatModifier modifier)
        {
            int before = MaxHealthOf(hero);
            int current = HealthOf(hero);
            if (!modifiers.TryGetValue(hero, out var list))
                modifiers[hero] = list = new List<StatModifier>();
            list.Add(modifier.Clone());
            int after = MaxHealthOf(hero);
            health[hero] = Mathf.Clamp(current + Mathf.Max(0, after - before), 1, after);
        }

        public void HealAll(float fraction)
        {
            foreach (var hero in Heroes)
                health[hero] = Mathf.Min(MaxHealthOf(hero), HealthOf(hero) + Mathf.RoundToInt(MaxHealthOf(hero) * fraction));
        }

        public void HurtAll(float fraction)
        {
            foreach (var hero in Heroes)
                health[hero] = Mathf.Max(1, HealthOf(hero) - Mathf.RoundToInt(HealthOf(hero) * fraction));
        }

        public void RecordBattle(IEnumerable<BattleUnit> units, float restHeal, float reviveHealth)
        {
            foreach (var unit in units)
            {
                int max = unit.MaxHealth;
                int value = unit.IsAlive ? unit.Health + Mathf.RoundToInt(max * restHeal) : Mathf.RoundToInt(max * reviveHealth);
                health[unit.Definition] = Mathf.Clamp(value, 1, max);
            }
        }

        public IReadOnlyList<HeroLoadout> Loadouts() => Heroes.Select(h => new HeroLoadout
        {
            Definition = h,
            Health = health.TryGetValue(h, out int value) ? value : null,
            Modifiers = ModifiersOf(h).Select(m => m.Clone()).ToList(),
        }).ToList();
    }
}
