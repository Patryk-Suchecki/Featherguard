using System;
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
        public abstract void Apply(in EffectContext context);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class DamageEffect : AbilityEffect
    {
        [Min(0)] public int power = 20;

        public override bool Damages => true;
        public override int ExpectedDamage => power;

        public override void Apply(in EffectContext context) => context.State.DealDamage(context.Actor, context.Target, power, context.Ability, context.Timing);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class HealEffect : AbilityEffect
    {
        [Min(0)] public int power = 30;

        public override bool Heals => true;

        public override void Apply(in EffectContext context) => context.State.Heal(context.Actor, context.Target, power, context.Ability);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class ApplyStatusEffect : AbilityEffect
    {
        public StatusApplication application = new();

        public override bool AppliesPerTarget => !application.onSelf;
        public override StatusDefinition StatusOnTarget => application.onSelf ? null : application.status;

        public override void Apply(in EffectContext context)
        {
            var recipient = application.onSelf ? context.Actor : context.Target;
            context.State.TryApplyStatus(context.Actor, recipient, application);
        }
    }
}
