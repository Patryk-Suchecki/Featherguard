using System;
using UnityEngine;

namespace Featherguard.Battle
{
    public readonly struct PotionContext
    {
        public readonly BattleState State;
        public readonly BattleUnit Target;
        public readonly PotionDefinition Potion;

        public PotionContext(BattleState state, BattleUnit target, PotionDefinition potion)
        {
            State = state;
            Target = target;
            Potion = potion;
        }
    }

    [Serializable]
    public abstract class PotionEffect
    {
        public virtual bool Once => false;
        public virtual bool Revives => false;
        public abstract void Apply(in PotionContext context);
    }

    [Serializable]
    public sealed class HealPercentPotion : PotionEffect
    {
        [Tooltip("Part of the target's max health healed.")]
        [Range(0f, 1f)] public float fraction = 0.35f;

        public override void Apply(in PotionContext context) => context.State.HealPercent(context.Target, fraction);
    }

    [Serializable]
    public sealed class CleansePotion : PotionEffect
    {
        public override void Apply(in PotionContext context) => context.State.Cleanse(context.Target);
    }

    [Serializable]
    public sealed class RemoveStatusPotion : PotionEffect
    {
        public StatusDefinition status;

        public override void Apply(in PotionContext context) => context.State.RemoveStatus(context.Target, status);
    }

    [Serializable]
    public sealed class StatusPotion : PotionEffect
    {
        public StatusApplication application = new();
        [Tooltip("How many times the status is applied, for stacks.")]
        [Min(1)] public int times = 1;

        public override void Apply(in PotionContext context)
        {
            for (int i = 0; i < times; i++)
                context.State.ApplyStatus(Team.Heroes, context.Target, application);
        }
    }

    [Serializable]
    public sealed class ShieldPercentPotion : PotionEffect
    {
        public StatusDefinition shield;
        [Min(1)] public int turns = 1;
        [Tooltip("Shield points as a part of the target's max health.")]
        [Range(0f, 1f)] public float fraction = 0.15f;

        public override void Apply(in PotionContext context) =>
            context.State.GrantShield(Team.Heroes, context.Target, shield, turns, (int)Math.Round(context.Target.MaxHealth * fraction));
    }

    [Serializable]
    public sealed class DamagePotion : PotionEffect
    {
        [Min(0)] public int power = 30;

        public override void Apply(in PotionContext context) => context.State.DealDamage(Team.Heroes, context.Target, power);
    }

    [Serializable]
    public sealed class RagePotion : PotionEffect
    {
        [Min(0)] public float amount = 40f;
        [Tooltip("Fills the Rage meter instead of adding the amount.")]
        public bool fill;

        public override bool Once => true;

        public override void Apply(in PotionContext context) => context.State.AddRage(fill ? context.State.Rage.Max : amount);
    }

    [Serializable]
    public sealed class RevivePotion : PotionEffect
    {
        [Tooltip("Part of max health a fallen hero returns with.")]
        [Range(0f, 1f)] public float fraction = 0.4f;

        public override bool Revives => true;

        public override void Apply(in PotionContext context) => context.State.Revive(context.Target, fraction);
    }
}
