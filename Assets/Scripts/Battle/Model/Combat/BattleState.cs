using System;
using System.Collections.Generic;
using Featherguard.Core;

namespace Featherguard.Battle
{
    public sealed class BattleState
    {
        public BattleRules Rules { get; }
        public IRandomSource Random { get; }
        public BattleLog Log { get; }
        public RageMeter Rage { get; }
        public Squad Heroes { get; } = new(Team.Heroes);
        public Squad Enemies { get; } = new(Team.Enemies);

        public event Action<BattleUnit> UnitKilled;

        public BattleState(BattleRules rules, IRandomSource random, BattleLog log, float startRage)
        {
            Rules = rules;
            Random = random;
            Log = log;
            Rage = new RageMeter(rules.maxRage, startRage);
        }

        public Squad AlliesOf(BattleUnit unit) => unit.Team == Team.Heroes ? Heroes : Enemies;
        public Squad OpponentsOf(BattleUnit unit) => unit.Team == Team.Heroes ? Enemies : Heroes;

        public BattleUnit Spawn(UnitDefinition definition, int slot, IEnumerable<StatModifier> modifiers, int? health, bool summoned = false)
        {
            var unit = new BattleUnit(definition, new StatBlock(definition.maxHealth, Rules.baseCritChance, Rules.baseCritDamage), slot);
            foreach (var modifier in modifiers)
                unit.AddModifier(modifier);
            unit.SetHealth(health ?? unit.MaxHealth);
            unit.Changed += () => Log.Emit(new UnitChanged { Unit = unit, State = unit.Snapshot() });
            (unit.Team == Team.Heroes ? Heroes : Enemies).Add(unit);
            Log.Emit(new UnitSpawned { Unit = unit, State = unit.Snapshot(), Summoned = summoned });
            return unit;
        }

        public void Remove(BattleUnit unit)
        {
            if (AlliesOf(unit).Remove(unit))
                Log.Emit(new UnitRemoved { Unit = unit });
        }

        public void AddRage(float amount)
        {
            bool wasReady = Rage.Ready;
            float before = Rage.Value;
            Rage.Add(amount);
            if (Rage.Value != before)
                Log.Emit(new RageChanged { Value = Rage.Value, Max = Rage.Max, BecameReady = !wasReady && Rage.Ready });
        }

        public void ConsumeRage()
        {
            Rage.Consume();
            Log.Emit(new RageChanged { Value = Rage.Value, Max = Rage.Max });
        }

        public void DealDamage(BattleUnit actor, BattleUnit target, int power, AbilityDefinition ability, ActionTiming timing = default)
        {
            if (!target.IsAlive) return;
            float multiplier = actor.Stat(StatType.Damage) * timing.DamageMultiplier(Rules);
            var roll = DamageCalculator.Roll(power, multiplier, actor.Stat(StatType.CritChance) + target.Stat(StatType.CritChanceTaken), actor.Stat(StatType.CritDamage), Rules.damageVariance, Random);
            var outcome = target.TakeDamage(roll.Amount);
            int dealt = outcome.Dealt;
            Log.Emit(new Damaged { Source = actor, Target = target, Ability = ability, Amount = dealt, Absorbed = outcome.Absorbed, Critical = roll.Critical });
            AddRage(dealt * (actor.Team == Team.Heroes ? Rules.heroRagePerDamage * actor.Stat(StatType.RageGain) : Rules.enemyRagePerDamage));
            if (roll.Critical && target.IsAlive)
                target.Statuses.RemoveWhere(s => s.ConsumedByCriticalHit);

            float lifesteal = actor.Stat(StatType.Lifesteal);
            if (lifesteal > 0f && actor.IsAlive)
            {
                int stolen = actor.ApplyHeal((int)Math.Round(dealt * lifesteal));
                if (stolen > 0) Log.Emit(new Healed { Target = actor, Amount = stolen });
            }
            if (actor != target && actor.Team != target.Team)
                Retaliate(target, actor);
            KillIfDead(target);
        }

        public int Heal(BattleUnit actor, BattleUnit target, int power, AbilityDefinition ability, ActionTiming timing = default)
        {
            if (!target.IsAlive) return 0;
            int amount = DamageCalculator.Heal(power, actor.Stat(StatType.Healing) * timing.SupportMultiplier(Rules));
            int healed = target.ApplyHeal(amount);
            Log.Emit(new Healed { Target = target, Ability = ability, Amount = healed });
            return Math.Max(0, amount - healed);
        }

        public void TryApplyStatus(BattleUnit actor, BattleUnit target, StatusApplication application, ActionTiming timing = default)
        {
            if (application.status == null || !target.IsAlive || target.Statuses.Blocks(application.status)) return;
            if (application.condition == StatusCondition.PerfectTiming && timing.Attack != TimingResult.Perfect) return;
            if (Random.Value > application.chance) return;
            int points = application.status.AbsorbsDamage ? (int)Math.Round(application.potency * actor.Stat(StatType.Healing) * timing.SupportMultiplier(Rules)) : 0;
            target.Statuses.Apply(application, target.Team == actor.Team, actor.Team, points);
            Log.Emit(new StatusApplied { Target = target, Status = application.status });
        }

        public void GrantShield(BattleUnit actor, BattleUnit target, StatusDefinition shield, int turns, int points) =>
            GrantShield(actor.Team, target, shield, turns, points);

        public void GrantShield(Team source, BattleUnit target, StatusDefinition shield, int turns, int points)
        {
            if (shield == null || points <= 0 || !target.IsAlive) return;
            var application = new StatusApplication { status = shield, turns = turns };
            target.Statuses.Apply(application, target.Team == source, source, points);
            Log.Emit(new StatusApplied { Target = target, Status = shield });
        }

        public void ApplyStatus(Team source, BattleUnit target, StatusApplication application)
        {
            if (application.status == null || !target.IsAlive || target.Statuses.Blocks(application.status)) return;
            target.Statuses.Apply(application, target.Team == source, source, application.potency);
            Log.Emit(new StatusApplied { Target = target, Status = application.status });
        }

        public void DealDamage(Team source, BattleUnit target, int power)
        {
            if (!target.IsAlive) return;
            var outcome = target.TakeDamage(power);
            Log.Emit(new Damaged { Target = target, Amount = outcome.Dealt, Absorbed = outcome.Absorbed });
            AddRage(outcome.Dealt * (source == Team.Heroes ? Rules.heroRagePerDamage : Rules.enemyRagePerDamage));
            KillIfDead(target);
        }

        public void HealPercent(BattleUnit target, float fraction)
        {
            if (!target.IsAlive) return;
            int healed = target.ApplyHeal((int)Math.Round(target.MaxHealth * fraction));
            Log.Emit(new Healed { Target = target, Amount = healed });
        }

        public void Revive(BattleUnit unit, float fraction)
        {
            if (unit.IsAlive) return;
            unit.SetHealth(Math.Max(1, (int)Math.Round(unit.MaxHealth * fraction)));
            Log.Emit(new Revived { Unit = unit });
        }

        void Retaliate(BattleUnit holder, BattleUnit attacker)
        {
            int power = holder.Statuses.Potency(s => s.Retaliates);
            if (power <= 0 || !attacker.IsAlive) return;
            var outcome = attacker.TakeDamage(power);
            Log.Emit(new Damaged { Source = holder, Target = attacker, Amount = outcome.Dealt, Absorbed = outcome.Absorbed });
            foreach (var application in holder.Statuses.RetaliationStatuses)
            {
                if (!attacker.IsAlive || application.status == null || attacker.Statuses.Blocks(application.status) || Random.Value > application.chance) continue;
                attacker.Statuses.Apply(application, true, holder.Team);
                Log.Emit(new StatusApplied { Target = attacker, Status = application.status });
            }
            KillIfDead(attacker);
        }

        public void DamageOverTime(BattleUnit unit, ActiveStatus status)
        {
            if (!unit.IsAlive) return;
            var outcome = unit.TakeDamage(status.DamagePerTurn);
            Log.Emit(new StatusTicked { Unit = unit, Status = status.Definition, Amount = outcome.Dealt, Absorbed = outcome.Absorbed });
            AddRage(outcome.Dealt * (status.Source == Team.Heroes ? Rules.heroRagePerDamage : Rules.enemyRagePerDamage));
            KillIfDead(unit);
        }

        public void RemoveStatus(BattleUnit unit, StatusDefinition status)
        {
            if (status == null || !unit.IsAlive || !unit.Statuses.Has(status)) return;
            unit.Statuses.RemoveWhere(s => s == status);
            Log.Emit(new Cleansed { Unit = unit });
        }

        public void Cleanse(BattleUnit unit)
        {
            if (!unit.IsAlive || !unit.Statuses.Any(s => s.harmful)) return;
            unit.Statuses.RemoveWhere(s => s.harmful);
            Log.Emit(new Cleansed { Unit = unit });
        }

        void KillIfDead(BattleUnit unit)
        {
            if (unit.IsAlive) return;
            unit.Statuses.Clear();
            Log.Emit(new Died { Unit = unit });
            UnitKilled?.Invoke(unit);
        }
    }
}
