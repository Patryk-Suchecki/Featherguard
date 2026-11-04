using System.Collections.Generic;
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
            {
                var special = definition.special;
                if (special.TargetsAll) return new EnemyDecision(special, null);
                var candidates = Targeting.Candidates(special, allies, opponents).ToList();
                if (candidates.Count > 0) return new EnemyDecision(special, special.TargetsEnemies ? ChooseVictim(enemy, special, candidates, opponents) : WeightedRandom.PickUniform(candidates, random));
            }

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
            return new EnemyDecision(attack, ChooseVictim(enemy, attack, targets, opponents));
        }

        BattleUnit ChooseVictim(BattleUnit enemy, AbilityDefinition ability, IReadOnlyList<BattleUnit> targets, Squad opponents)
        {
            if (opponents.Taunting.Any())
                return WeightedRandom.PickUniform(targets, random);

            var profile = enemy.Definition.ai;
            float expected = (ability.Effect<DamageEffect>()?.power ?? 0) * ability.hits * enemy.Stat(StatType.Damage);
            return WeightedRandom.Pick(targets, h =>
            {
                float weight = 1f + profile.woundedWeight * (1f - h.HealthFraction);
                if (expected * h.Stat(StatType.DamageTaken) >= h.Health) weight += profile.finishWeight;
                return weight * h.Stat(StatType.DamageTaken);
            }, random);
        }
    }
}
