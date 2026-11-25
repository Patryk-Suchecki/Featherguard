using System;
using System.Linq;
using Featherguard.Core;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Featherguard.Battle
{
    public readonly struct EffectContext
    {
        public readonly BattleState State;
        public readonly BattleUnit Actor;
        public readonly BattleUnit Target;
        public readonly AbilityDefinition Ability;
        public readonly ActionTiming Timing;

        public EffectContext(BattleState state, BattleUnit actor, BattleUnit target, AbilityDefinition ability, ActionTiming timing = default)
        {
            State = state;
            Actor = actor;
            Target = target;
            Ability = ability;
            Timing = timing;
        }
    }

    [Serializable]
    public abstract class AbilityEffect
    {
        public virtual bool AppliesPerTarget => true;
        public virtual bool Damages => false;
        public virtual bool Heals => false;
        public virtual int ExpectedDamage => 0;
        public virtual StatusDefinition StatusOnTarget => null;
        public abstract string Summary { get; }
        public abstract void Apply(in EffectContext context);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class DamageEffect : AbilityEffect
    {
        [Min(0)] public int power = 20;

        public override bool Damages => true;
        public override int ExpectedDamage => power;
        public override string Summary => $"{power} damage";

        public override void Apply(in EffectContext context) => context.State.DealDamage(context.Actor, context.Target, power, context.Ability, context.Timing);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class HealEffect : AbilityEffect
    {
        [Min(0)] public int power = 30;
        [Tooltip("Optional: healing beyond full health becomes this absorbing status.")]
        public StatusDefinition overflowShield;
        [Min(1)] public int overflowShieldTurns = 2;

        public override bool Heals => true;
        public override string Summary => overflowShield != null ? $"heals {power}; healing past full health becomes a Shield for {overflowShieldTurns} turns" : $"heals {power}";

        public override void Apply(in EffectContext context)
        {
            int overflow = context.State.Heal(context.Actor, context.Target, power, context.Ability, context.Timing);
            if (overflowShield != null)
                context.State.GrantShield(context.Actor, context.Target, overflowShield, overflowShieldTurns, overflow);
        }
    }

    [Serializable]
    public sealed class ChainDamageEffect : AbilityEffect
    {
        [Min(0)] public int power = 20;
        [Tooltip("Each jump to the next enemy keeps this fraction of the previous hit's power.")]
        [Range(0f, 1f)] public float falloff = 0.6f;

        public override bool Damages => true;
        public override int ExpectedDamage => power;
        public override string Summary => $"{power} damage, then jumps to every other enemy, keeping {Math.Round(falloff * 100f)}% of the hit each time";

        public override void Apply(in EffectContext context)
        {
            var state = context.State;
            state.DealDamage(context.Actor, context.Target, power, context.Ability, context.Timing);
            float jump = power;
            var first = context.Target;
            var others = state.OpponentsOf(context.Actor).Living.Where(u => u != first).OrderBy(u => u.Slot).ToList();
            foreach (var next in others)
            {
                jump *= falloff;
                int hit = (int)Math.Round(jump);
                if (hit <= 0 || !context.Actor.IsAlive) break;
                state.DealDamage(context.Actor, next, hit, context.Ability, context.Timing);
            }
        }
    }

    [Serializable]
    public sealed class PerfectTimingEffect : AbilityEffect
    {
        [Tooltip("Applied only when the player times the action perfectly.")]
        [SerializeReference, SubclassSelector] public AbilityEffect effect;

        public override bool AppliesPerTarget => effect == null || effect.AppliesPerTarget;
        public override string Summary => effect != null ? $"on a perfect timing, {effect.Summary}" : "";

        public override void Apply(in EffectContext context)
        {
            if (effect != null && context.Timing.AttackPerfect)
                effect.Apply(context);
        }
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class ApplyStatusEffect : AbilityEffect
    {
        public StatusApplication application = new();

        public override bool AppliesPerTarget => !application.onSelf;
        public override StatusDefinition StatusOnTarget => application.onSelf ? null : application.status;
        public override string Summary => application.Summary;

        public override void Apply(in EffectContext context)
        {
            var recipient = application.onSelf ? context.Actor : context.Target;
            context.State.TryApplyStatus(context.Actor, recipient, application, context.Timing);
        }
    }

    [Serializable]
    public sealed class CleanseEffect : AbilityEffect
    {
        public override string Summary => "removes every harmful status";

        public override void Apply(in EffectContext context) => context.State.Cleanse(context.Target);
    }
}
