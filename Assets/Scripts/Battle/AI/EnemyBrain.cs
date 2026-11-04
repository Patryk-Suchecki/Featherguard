using System.Linq;
using Featherguard.Core;

namespace Featherguard.Battle
{
    public readonly struct EnemyDecision
    {
        public readonly AbilityDefinition Ability;
        public readonly BattleUnit Target;

        public EnemyDecision(AbilityDefinition ability, BattleUnit target)
        {
            Ability = ability;
            Target = target;
        }
    }

    public interface IEnemyBrain
    {
        EnemyDecision Decide(BattleUnit enemy, Combat combat);
    }

    public sealed class DefaultEnemyBrain : IEnemyBrain
    {
        readonly IRandomSource random;

        public DefaultEnemyBrain(IRandomSource random) => this.random = random;

        public EnemyDecision Decide(BattleUnit enemy, Combat combat)
        {
            var definition = enemy.Definition;
            var profile = definition.ai;
            var allies = combat.AlliesOf(enemy);
            var opponents = combat.OpponentsOf(enemy);

            if (definition.special != null && profile.specialEvery > 0 && enemy.TurnsTaken % profile.specialEvery == 0)
                return new EnemyDecision(definition.special, null);

            var support = definition.support;
            if (support != null && support.Effect<HealEffect>() != null)
            {
                var wounded = allies.Living.Where(a => a.HealthFraction < profile.healThreshold).OrderBy(a => a.HealthFraction).FirstOrDefault();
                if (wounded != null && random.Value < profile.healChance)
                    return new EnemyDecision(support, wounded);
            }
            else if (support != null && random.Value < profile.supportChance)
            {
                var statuses = support.effects.OfType<ApplyStatusEffect>().Where(e => !e.application.onSelf).Select(e => e.application.status).ToList();
                var candidates = allies.Living.Where(a => !statuses.Any(a.Statuses.Has)).ToList();
                if (candidates.Count > 0)
                    return new EnemyDecision(support, WeightedRandom.PickUniform(candidates, random));
            }

            var attack = definition.attack;
            var targets = Targeting.Candidates(attack, allies, opponents).ToList();
            if (targets.Count == 0) return new EnemyDecision(attack, null);
            if (opponents.Taunting.Any())
                return new EnemyDecision(attack, WeightedRandom.PickUniform(targets, random));

            float expected = (attack.Effect<DamageEffect>()?.power ?? 0) * attack.hits * enemy.Stat(StatType.Damage);
            var target = WeightedRandom.Pick(targets, h =>
            {
                float weight = 1f + profile.woundedWeight * (1f - h.HealthFraction);
                if (expected * h.Stat(StatType.DamageTaken) >= h.Health) weight += profile.finishWeight;
                return weight * h.Stat(StatType.DamageTaken);
            }, random);
            return new EnemyDecision(attack, target);
        }
    }
}
