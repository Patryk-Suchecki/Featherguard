using System.Collections;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.Battle
{
    public sealed class ActionExecutor
    {
        readonly Combat combat;

        public ActionExecutor(Combat combat) => this.combat = combat;

        public IEnumerator Execute(BattleUnit actor, AbilityDefinition ability, BattleUnit chosen)
        {
            var targets = Targeting.Resolve(ability, chosen, combat.AlliesOf(actor), combat.OpponentsOf(actor));
            if (targets.Count == 0) yield break;

            if (ability == actor.Definition.special)
            {
                combat.View.Callout(ability.displayName);
                combat.Feedback.Play(SfxId.Rage);
                yield return new WaitForSeconds(combat.Rules.specialWindup);
            }

            var context = new DeliveryContext
            {
                Actor = actor,
                Targets = targets,
                Ability = ability,
                Combat = combat,
                Resolve = target => ResolveOn(actor, ability, target),
            };
            yield return ability.delivery.Deliver(context);

            if (ability.IsDamaging)
                actor.Statuses.RemoveWhere(s => s.consumedOnAttack);
            if (actor.IsAlive)
                foreach (var effect in ability.effects)
                    if (!effect.AppliesPerTarget)
                        effect.Apply(new EffectContext(combat, actor, actor, ability));
            yield return new WaitForSeconds(combat.Rules.afterAction);
        }

        void ResolveOn(BattleUnit actor, AbilityDefinition ability, BattleUnit target)
        {
            if (!target.IsAlive) return;
            if (!ability.HasDirectImpact)
            {
                combat.Feedback.Rise(target.transform.position, ability.fxColor, target.Radius);
                combat.Feedback.Play(ability.impactSound);
            }
            foreach (var effect in ability.effects)
                if (effect.AppliesPerTarget && (target.IsAlive || effect is DamageEffect))
                    effect.Apply(new EffectContext(combat, actor, target, ability));
        }
    }
}
