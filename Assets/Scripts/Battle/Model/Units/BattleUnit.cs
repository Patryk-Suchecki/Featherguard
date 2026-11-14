using System;
using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Battle
{
    public readonly struct DamageOutcome
    {
        public readonly int Dealt;
        public readonly int Absorbed;

        public DamageOutcome(int dealt, int absorbed)
        {
            Dealt = dealt;
            Absorbed = absorbed;
        }
    }

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
        public bool HasActed { get; internal set; }
        public int TurnsTaken { get; internal set; }
        public int ActionsLeftThisTurn { get; internal set; }
        public bool Enraged { get; internal set; }
        public StatusContainer Statuses { get; } = new();
        public IReadOnlyList<ActiveStatus> ActiveStatuses => Statuses.All;
        public bool SkipsTurn => Statuses.Any(s => s.SkipsTurn);
        public bool ForcesTargeting => Statuses.Any(s => s.ForcesTargeting);
        public bool ActsLast => Statuses.Any(s => s.ActsLast);
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

        public DamageOutcome TakeDamage(float amount)
        {
            int total = Math.Max(0, (int)Math.Round(amount * Stat(StatType.DamageTaken)));
            int absorbed = Statuses.Absorb(total);
            int dealt = Math.Min(total - absorbed, Health);
            Health -= dealt;
            NotifyChanged();
            return new DamageOutcome(dealt, absorbed);
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
            Statuses = Statuses.All.Select(s => new StatusSnapshot(s.Definition, s.Turns, s.Stacks, s.Points)).ToList(),
        };

        void NotifyChanged() => Changed?.Invoke();
    }
}
