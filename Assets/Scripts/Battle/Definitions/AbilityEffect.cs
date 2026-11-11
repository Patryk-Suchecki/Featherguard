using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Featherguard.Battle
{
    public readonly struct EffectContext
    {
        public readonly Combat Combat;
        public readonly BattleUnit Actor;
        public readonly BattleUnit Target;
        public readonly AbilityDefinition Ability;

        public EffectContext(Combat combat, BattleUnit actor, BattleUnit target, AbilityDefinition ability)
        {
            Combat = combat;
            Actor = actor;
            Target = target;
            Ability = ability;
        }
    }

    [Serializable]
    public abstract class AbilityEffect
    {
        public virtual bool AppliesPerTarget => true;
        public abstract void Apply(in EffectContext context);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class DamageEffect : AbilityEffect
    {
        [Min(0)] public int power = 20;

        public override void Apply(in EffectContext context) => context.Combat.DealDamage(context.Actor, context.Target, power, context.Ability);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class HealEffect : AbilityEffect
    {
        [Min(0)] public int power = 30;

        public override void Apply(in EffectContext context) => context.Combat.Heal(context.Actor, context.Target, power, context.Ability);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class ApplyStatusEffect : AbilityEffect
    {
        public StatusApplication application = new();

        public override bool AppliesPerTarget => !application.onSelf;

        public override void Apply(in EffectContext context)
        {
            var recipient = application.onSelf ? context.Actor : context.Target;
            context.Combat.TryApplyStatus(context.Actor, recipient, application);
        }
    }
}
