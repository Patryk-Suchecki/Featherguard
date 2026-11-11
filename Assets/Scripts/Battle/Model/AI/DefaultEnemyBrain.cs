using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;

namespace Featherguard.Battle
{
    public sealed class DefaultEnemyBrain : IEnemyBrain
    {
        readonly IRandomSource random;

        public DefaultEnemyBrain(IRandomSource random) => this.random = random;

        public EnemyDecision Decide(BattleUnit enemy, BattleState state)
        {
            var definition = enemy.Definition;
            var profile = definition.ai;
            var allies = state.AlliesOf(enemy);
            var opponents = state.OpponentsOf(enemy);

            if (definition.special != null && profile.specialEvery > 0 && enemy.TurnsTaken % profile.specialEvery == 0 && enemy.ActionsLeftThisTurn == 0)
            {
                var special = definition.special;
                if (special.TargetsAll) return new EnemyDecision(special, null);
                var candidates = Targeting.Candidates(special, allies, opponents).ToList();
                if (candidates.Count > 0) return new EnemyDecision(special, special.TargetsEnemies ? ChooseVictim(enemy, special, candidates, opponents) : WeightedRandom.PickUniform(candidates, random));
            }

            var support = definition.support;
            if (support != null && support.Heals)
            {
                var wounded = allies.Living.Where(a => a.HealthFraction < profile.healThreshold).OrderBy(a => a.HealthFraction).FirstOrDefault();
                if (wounded != null && random.Value < profile.healChance)
                    return new EnemyDecision(support, wounded);
            }
            else if (support != null && random.Value < profile.supportChance)
            {
                var statuses = support.StatusesOnTarget.ToList();
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
            float expected = ability.ExpectedDamage * ability.hits * enemy.Stat(StatType.Damage);
            return WeightedRandom.Pick(targets, h =>
            {
                float weight = 1f + profile.woundedWeight * (1f - h.HealthFraction);
                if (expected * h.Stat(StatType.DamageTaken) >= h.Health) weight += profile.finishWeight;
                return weight * h.Stat(StatType.DamageTaken);
            }, random);
        }
    }
}
