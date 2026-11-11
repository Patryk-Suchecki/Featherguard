using System;
using Featherguard.Core;

namespace Featherguard.Battle
{
    public readonly struct DamageRoll
    {
        public readonly int Amount;
        public readonly bool Critical;

        public DamageRoll(int amount, bool critical)
        {
            Amount = amount;
            Critical = critical;
        }
    }

    public static class DamageCalculator
    {
        public static DamageRoll Roll(int power, float damageMultiplier, float critChance, float critDamage, float variance, IRandomSource random)
        {
            bool critical = random.Value < critChance;
            float amount = power * damageMultiplier * random.Range(1f - variance, 1f + variance);
            if (critical) amount *= critDamage;
            return new DamageRoll((int)Math.Round(amount), critical);
        }

        public static int Heal(int power, float healingMultiplier) => (int)Math.Round(power * healingMultiplier);
    }
}
