using System.Linq;

namespace Featherguard.Battle
{
    public sealed class ActionResolver
    {
        readonly BattleState state;

        public ActionResolver(BattleState state) => this.state = state;

        public void Execute(BattleUnit actor, AbilityDefinition ability, BattleUnit chosen, ActionTiming timing = default)
        {
            var targets = Targeting.Resolve(ability, chosen, state.AlliesOf(actor), state.OpponentsOf(actor));
            if (targets.Count == 0) return;
            if (!ability.IsDamaging) timing = ActionTiming.None;

            var action = new ActionPerformed { Actor = actor, Ability = ability, Targets = targets, Special = ability == actor.Definition.special, Timing = timing };
            state.Log.Emit(action);

            for (int hit = 0; hit < ability.hits; hit++)
            {
                foreach (var target in targets.Where(t => t.IsAlive))
                {
                    var impact = new Impact { Hit = hit, Target = target };
                    action.Impacts.Add(impact);
                    state.Log.Push(impact.Outcomes);
                    ResolveOn(actor, ability, target, timing);
                    state.Log.Pop();
                }
            }

            state.Log.Push(action.Aftermath);
            if (timing.AnyPerfect)
                state.AddRage(state.Rules.perfectTimingRage);
            if (ability.IsDamaging)
                actor.Statuses.RemoveWhere(s => s.ConsumedOnAttack);
            if (actor.IsAlive)
                foreach (var effect in ability.effects)
                    if (!effect.AppliesPerTarget)
                        effect.Apply(new EffectContext(state, actor, actor, ability, timing));
            state.Log.Pop();
        }

        void ResolveOn(BattleUnit actor, AbilityDefinition ability, BattleUnit target, ActionTiming timing)
        {
            foreach (var effect in ability.effects)
                if (effect.AppliesPerTarget && target.IsAlive)
                    effect.Apply(new EffectContext(state, actor, target, ability, timing));
        }
    }
}
