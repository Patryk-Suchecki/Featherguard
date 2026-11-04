using System;
using System.Collections.Generic;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle
{
    public class BattleUnit : MonoBehaviour
    {
        StatBlock stats;

        public UnitDefinition Definition { get; private set; }
        public Team Team => Definition.team;
        public int Health { get; private set; }
        public int MaxHealth => Mathf.Max(1, Mathf.RoundToInt(Stat(StatType.MaxHealth)));
        public float HealthFraction => (float)Health / MaxHealth;
        public bool IsAlive => Health > 0;
        public bool HasActed { get; set; }
        public int TurnsTaken { get; set; }
        public bool Enraged { get; set; }
        public Vector3 HomePosition { get; set; }
        public StatusContainer Statuses { get; } = new();
        public IReadOnlyList<StatusEffect> ActiveStatuses => Statuses.All;
        public UnitAnimator Animator { get; private set; }
        public HitReaction Reaction { get; private set; }
        public SelectionRing Ring { get; private set; }
        public float Height => Definition.height * Definition.modelScale;
        public float Radius => 0.45f * Definition.modelScale;
        public Vector3 Center => transform.position + Vector3.up * Height * 0.5f;
        public Vector3 Top => transform.position + Vector3.up * (Height + 0.1f);
        public Vector3 Facing => Team == Team.Heroes ? Vector3.right : Vector3.left;
        public bool SkipsTurn => Statuses.Any(s => s.skipsTurn);
        public bool ForcesTargeting => Statuses.Any(s => s.forcesTargeting);

        public event Action Changed;

        internal void Bind(UnitDefinition definition, StatBlock statBlock, UnitAnimator animator, HitReaction reaction, SelectionRing ring)
        {
            Definition = definition;
            stats = statBlock;
            Animator = animator;
            Reaction = reaction;
            Ring = ring;
            Health = MaxHealth;
            Statuses.Changed += NotifyChanged;
        }

        public float Stat(StatType stat) => stats.Get(stat, Statuses.Modifiers);

        public void AddModifier(StatModifier modifier)
        {
            stats.Add(modifier);
            Health = Mathf.Min(Health, MaxHealth);
            NotifyChanged();
        }

        public void SetHealth(int value)
        {
            Health = Mathf.Clamp(value, 0, MaxHealth);
            NotifyChanged();
        }

        public int ApplyDamage(int amount)
        {
            amount = Mathf.Clamp(Mathf.RoundToInt(amount * Stat(StatType.DamageTaken)), 0, Health);
            Health -= amount;
            NotifyChanged();
            return amount;
        }

        public int ApplyHeal(int amount)
        {
            if (!IsAlive) return 0;
            amount = Mathf.Clamp(amount, 0, MaxHealth - Health);
            Health += amount;
            NotifyChanged();
            return amount;
        }

        public void NotifyChanged() => Changed?.Invoke();
    }
}
