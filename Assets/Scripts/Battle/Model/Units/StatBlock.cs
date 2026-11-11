using System;
using System.Collections.Generic;

namespace Featherguard.Battle
{
    public sealed class StatBlock
    {
        readonly Dictionary<StatType, float> baseValues = new();
        readonly List<StatModifier> modifiers = new();

        public StatBlock(int maxHealth, float critChance, float critDamage)
        {
            baseValues[StatType.Damage] = 1f;
            baseValues[StatType.CritChance] = critChance;
            baseValues[StatType.CritDamage] = critDamage;
            baseValues[StatType.MaxHealth] = maxHealth;
            baseValues[StatType.Healing] = 1f;
            baseValues[StatType.DamageTaken] = 1f;
            baseValues[StatType.Lifesteal] = 0f;
            baseValues[StatType.RageGain] = 1f;
        }

        public IReadOnlyList<StatModifier> Modifiers => modifiers;

        public void Add(StatModifier modifier) => modifiers.Add(modifier);

        public float Get(StatType stat, IEnumerable<StatModifier> extra = null)
        {
            float value = baseValues.TryGetValue(stat, out float b) ? b : 0f;
            float multiplier = 1f;
            Accumulate(modifiers, stat, ref value, ref multiplier);
            if (extra != null) Accumulate(extra, stat, ref value, ref multiplier);
            return value * Math.Max(0f, multiplier);
        }

        static void Accumulate(IEnumerable<StatModifier> source, StatType stat, ref float value, ref float multiplier)
        {
            foreach (var modifier in source)
            {
                if (modifier.stat != stat) continue;
                value += modifier.add;
                multiplier += modifier.multiply;
            }
        }
    }
}
