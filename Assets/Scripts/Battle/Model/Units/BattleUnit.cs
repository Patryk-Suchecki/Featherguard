using System;
using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Battle
{
    public sealed class BattleUnit
    {
        readonly StatBlock stats;

        public UnitDefinition Definition { get; }
        public Team Team => Definition.team;
        public int Slot { get; }
        public int Health { get; private set; }
        public int MaxHealth => Math.Max(1, (int)Math.Round(Stat(StatType.MaxHealth)));
        public float HealthFraction => (float)Health / MaxHealth;
        public bool IsAlive => Health > 0;
        public bool HasActed { get; set; }
        public int TurnsTaken { get; set; }
        public int ActionsLeftThisTurn { get; set; }
        public bool Enraged { get; set; }
        public StatusContainer Statuses { get; } = new();
        public IReadOnlyList<StatusEffect> ActiveStatuses => Statuses.All;
        public bool SkipsTurn => Statuses.Any(s => s.skipsTurn);
        public bool ForcesTargeting => Statuses.Any(s => s.forcesTargeting);
        public bool CanAct => IsAlive && !HasActed;

        public event Action Changed;

        public BattleUnit(UnitDefinition definition, StatBlock statBlock, int slot)
        {
            Definition = definition;
            stats = statBlock;
            Slot = slot;
            Health = MaxHealth;
            Statuses.Changed += NotifyChanged;
        }

        public float Stat(StatType stat) => stats.Get(stat, Statuses.Modifiers);

        public void AddModifier(StatModifier modifier)
        {
            stats.Add(modifier);
            Health = Math.Min(Health, MaxHealth);
            NotifyChanged();
        }

        public void SetHealth(int value)
        {
            Health = Math.Clamp(value, 0, MaxHealth);
            NotifyChanged();
        }

        public int ApplyDamage(int amount)
        {
            amount = Math.Clamp((int)Math.Round(amount * Stat(StatType.DamageTaken)), 0, Health);
            Health -= amount;
            NotifyChanged();
            return amount;
        }

        public int ApplyHeal(int amount)
        {
            if (!IsAlive) return 0;
            amount = Math.Clamp(amount, 0, MaxHealth - Health);
            Health += amount;
            NotifyChanged();
            return amount;
        }

        public UnitSnapshot Snapshot() => new()
        {
            Health = Health,
            MaxHealth = MaxHealth,
            Statuses = Statuses.All.Select(s => new StatusSnapshot(s.Definition, s.Turns)).ToList(),
        };

        void NotifyChanged() => Changed?.Invoke();
    }
}
