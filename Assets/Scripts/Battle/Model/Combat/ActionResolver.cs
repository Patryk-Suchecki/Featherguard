using System.Linq;

namespace Featherguard.Battle
{
    public sealed class ActionResolver
    {
        readonly Combat combat;

        public ActionResolver(Combat combat) => this.combat = combat;

        public void Execute(BattleUnit actor, AbilityDefinition ability, BattleUnit chosen, ActionTiming timing = default)
        {
            var targets = Targeting.Resolve(ability, chosen, combat.AlliesOf(actor), combat.OpponentsOf(actor));
            if (targets.Count == 0) return;
            if (!ability.IsDamaging) timing = ActionTiming.None;

            var action = new ActionPerformed { Actor = actor, Ability = ability, Targets = targets, Special = ability == actor.Definition.special, Timing = timing };
            combat.Log.Emit(action);

            for (int hit = 0; hit < ability.hits; hit++)
            {
                foreach (var target in targets.Where(t => t.IsAlive))
                {
                    var impact = new Impact { Hit = hit, Target = target };
                    action.Impacts.Add(impact);
                    combat.Log.Push(impact.Outcomes);
                    ResolveOn(actor, ability, target, timing);
                    combat.Log.Pop();
                }
            }

            combat.Log.Push(action.Aftermath);
            if (timing.AnyPerfect)
                combat.AddRage(combat.Rules.perfectTimingRage);
            if (ability.IsDamaging)
                actor.Statuses.RemoveWhere(s => s.consumedOnAttack);
            if (actor.IsAlive)
                foreach (var effect in ability.effects)
                    if (!effect.AppliesPerTarget)
                        effect.Apply(new EffectContext(combat, actor, actor, ability));
            combat.Log.Pop();
        }

        void ResolveOn(BattleUnit actor, AbilityDefinition ability, BattleUnit target, ActionTiming timing)
        {
            foreach (var effect in ability.effects)
                if (effect.AppliesPerTarget && target.IsAlive)
                    effect.Apply(new EffectContext(combat, actor, target, ability, timing));
        }
    }
}
