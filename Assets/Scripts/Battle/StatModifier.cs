using System;

namespace Featherguard.Battle
{
    public enum StatType { Damage, CritChance, CritDamage, MaxHealth, Healing, DamageTaken }

    [Serializable]
    public class StatModifier
    {
        public StatType stat;
        public float add;
        public float multiply;

        public StatModifier(StatType stat, float add = 0f, float multiply = 0f)
        {
            this.stat = stat;
            this.add = add;
            this.multiply = multiply;
        }
    }
}
