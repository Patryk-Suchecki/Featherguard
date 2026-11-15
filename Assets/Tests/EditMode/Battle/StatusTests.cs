using System.Linq;
using Featherguard.Battle;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class StatusTests
    {
        static StatusDefinition Status(params StatusBehaviour[] behaviours)
        {
            var status = ScriptableObject.CreateInstance<StatusDefinition>();
            status.behaviours = behaviours;
            return status;
        }

        static StatusBehaviour Modify(StatType stat, float add = 0f, float multiply = 0f) =>
            new ModifyStats { modifiers = new[] { new StatModifier(stat, add, multiply) } };

        static UnitDefinition Unit(Team team, int health = 100)
        {
            var unit = ScriptableObject.CreateInstance<UnitDefinition>();
            unit.team = team;
            unit.maxHealth = health;
            return unit;
        }

        static BattleState State(params float[] rolls)
        {
            var rules = ScriptableObject.CreateInstance<BattleRules>();
            rules.baseCritChance = 0f;
            rules.damageVariance = 0f;
            return new BattleState(rules, new FakeRandom(0.5f, rolls), new BattleLog(), 0f);
        }

        static BattleUnit Spawn(BattleState state, Team team, int health = 100, params StatModifier[] modifiers) =>
            state.Spawn(Unit(team, health), team == Team.Heroes ? state.Heroes.Count : state.Enemies.Count, modifiers, null);

        static StatusApplication Application(StatusDefinition status, int turns = 1, int potency = 0, StatusCondition condition = StatusCondition.Always) =>
            new() { status = status, turns = turns, potency = potency, condition = condition };

        static UnitDefinition Fighter(Team team, int health, int power)
        {
            var attack = ScriptableObject.CreateInstance<AbilityDefinition>();
            attack.target = AbilityTarget.Enemy;
            attack.effects = new AbilityEffect[] { new DamageEffect { power = power } };
            var unit = Unit(team, health);
            unit.attack = attack;
            return unit;
        }

        static BattleModel Model(UnitDefinition hero, params UnitDefinition[][] waves)
        {
            var rules = ScriptableObject.CreateInstance<BattleRules>();
            rules.baseCritChance = 0f;
            rules.damageVariance = 0f;
            var encounter = ScriptableObject.CreateInstance<EncounterDefinition>();
            encounter.waves = waves.Select(w => new EncounterDefinition.Wave { enemies = w }).ToArray();
            var setup = new BattleSetup { Encounter = encounter, Heroes = new[] { new HeroLoadout { Definition = hero } } };
            return new BattleModel(setup, rules, new FakeRandom(0.5f));
        }

        [Test]
        public void Burn_StacksUpToItsLimitAndDealsPotencyPerStack()
        {
            var burn = Status(new DamageOverTime());
            burn.stacking = StatusStacking.Stack;
            burn.maxStacks = 5;
            var statuses = new StatusContainer();
            for (int i = 0; i < 7; i++)
                statuses.Apply(Application(burn, 2, 4), false);

            Assert.AreEqual(1, statuses.All.Count);
            Assert.AreEqual(5, statuses.All[0].Stacks);
            Assert.AreEqual(20, statuses.All[0].DamagePerTurn);
        }

        [Test]
        public void FreshStatus_DoesNotLoseATurnAtTheEndOfTheTurnItWasAppliedIn()
        {
            var empower = Status(Modify(StatType.Damage, multiply: 0.25f));
            var statuses = new StatusContainer();
            statuses.Apply(Application(empower), true);

            statuses.EndTurn();
            Assert.IsTrue(statuses.Has(empower));
            statuses.EndTurn();
            Assert.IsFalse(statuses.Has(empower));
        }

        [Test]
        public void DamageOverTime_IsNotModifiedByTheSourceAndGivesRageToTheSourceSide()
        {
            var state = State();
            var enemy = Spawn(state, Team.Enemies);
            var bleed = Status(new DamageOverTime());
            enemy.Statuses.Apply(Application(bleed, 3, 6), false, Team.Heroes);

            state.DamageOverTime(enemy, enemy.ActiveStatuses[0]);

            Assert.AreEqual(94, enemy.Health);
            Assert.AreEqual(6 * state.Rules.heroRagePerDamage, state.Rage.Value, 0.001f);
        }

        [Test]
        public void Shield_PointsScaleWithTheCastersHealingAndNeverAddUp()
        {
            var state = State();
            var caster = Spawn(state, Team.Heroes, 100, new StatModifier(StatType.Healing, multiply: 0.5f));
            var ally = Spawn(state, Team.Heroes);
            var shield = Status(new AbsorbDamage());

            state.TryApplyStatus(caster, ally, Application(shield, 1, 30));
            state.TryApplyStatus(caster, ally, Application(shield, 1, 20));

            Assert.AreEqual(1, ally.ActiveStatuses.Count);
            Assert.AreEqual(45, ally.ActiveStatuses[0].Points);
        }

        [Test]
        public void Shield_SoaksDamageAndLeavesAtZero()
        {
            var state = State();
            var hero = Spawn(state, Team.Heroes);
            var enemy = Spawn(state, Team.Enemies);
            hero.Statuses.Apply(Application(Status(new AbsorbDamage()), 2, 30), false, Team.Heroes, 30);

            state.DealDamage(enemy, hero, 20, null);
            Assert.AreEqual(100, hero.Health);
            Assert.AreEqual(10, hero.ActiveStatuses[0].Points);

            state.DealDamage(enemy, hero, 25, null);
            Assert.AreEqual(85, hero.Health);
            Assert.IsEmpty(hero.ActiveStatuses);
        }

        [Test]
        public void Shield_SoaksDamageOverTime()
        {
            var state = State();
            var hero = Spawn(state, Team.Heroes);
            hero.Statuses.Apply(Application(Status(new AbsorbDamage()), 2), false, Team.Heroes, 30);
            hero.Statuses.Apply(Application(Status(new DamageOverTime()), 2, 8), false, Team.Enemies);

            state.DamageOverTime(hero, hero.ActiveStatuses[1]);

            Assert.AreEqual(100, hero.Health);
            Assert.AreEqual(22, hero.ActiveStatuses[0].Points);
        }

        [Test]
        public void DamageIsRoundedOnceAfterTheTargetsDamageTaken()
        {
            var state = State();
            var attacker = Spawn(state, Team.Heroes, 100, new StatModifier(StatType.Damage, multiply: 0.25f));
            var target = Spawn(state, Team.Enemies, 100, new StatModifier(StatType.DamageTaken, multiply: 0.3f));

            state.DealDamage(attacker, target, 7, null);

            Assert.AreEqual(89, target.Health);
        }

        [Test]
        public void EmpowerAndCurse_AddUp()
        {
            var state = State();
            var unit = Spawn(state, Team.Heroes);
            unit.Statuses.Apply(Application(Status(Modify(StatType.Damage, multiply: 0.25f))), false);
            unit.Statuses.Apply(Application(Status(Modify(StatType.Damage, multiply: -0.3f))), false);

            Assert.AreEqual(0.95f, unit.Stat(StatType.Damage), 0.001f);
        }

        [Test]
        public void Mark_RaisesCritChanceAgainstTheOwnerAndLeavesAfterTheFirstCriticalHit()
        {
            var state = State(0.4f, 0.5f);
            var hero = Spawn(state, Team.Heroes);
            var enemy = Spawn(state, Team.Enemies);
            enemy.Statuses.Apply(Application(Status(Modify(StatType.CritChanceTaken, add: 0.5f), new ConsumeOnCriticalHit()), 2), false);

            Assert.AreEqual(0.5f, enemy.Stat(StatType.CritChanceTaken), 0.001f);
            state.DealDamage(hero, enemy, 10, null);

            Assert.AreEqual(85, enemy.Health);
            Assert.IsEmpty(enemy.ActiveStatuses);
        }

        [Test]
        public void PerfectTimingApplication_NeedsAPerfectAttack()
        {
            var state = State();
            var hero = Spawn(state, Team.Heroes);
            var enemy = Spawn(state, Team.Enemies);
            var stun = Status(new SkipTurn());
            var application = Application(stun, condition: StatusCondition.PerfectTiming);

            state.TryApplyStatus(hero, enemy, application, ActionTiming.ForAttack(TimingResult.Good));
            Assert.IsFalse(enemy.SkipsTurn);
            state.TryApplyStatus(hero, enemy, application);
            Assert.IsFalse(enemy.SkipsTurn);
            state.TryApplyStatus(hero, enemy, application, ActionTiming.ForAttack(TimingResult.Perfect));
            Assert.IsTrue(enemy.SkipsTurn);
        }

        [Test]
        public void Cleanse_RemovesHarmfulStatusesOnly()
        {
            var state = State();
            var hero = Spawn(state, Team.Heroes);
            var bleed = Status(new DamageOverTime());
            bleed.harmful = true;
            var empower = Status(Modify(StatType.Damage, multiply: 0.25f));
            hero.Statuses.Apply(Application(bleed, 3, 6), false);
            hero.Statuses.Apply(Application(empower), false);

            state.Cleanse(hero);

            Assert.IsFalse(hero.Statuses.Has(bleed));
            Assert.IsTrue(hero.Statuses.Has(empower));
        }

        [Test]
        public void StunnedTaunter_StillForcesTargeting()
        {
            var state = State();
            var enemy = Spawn(state, Team.Enemies);
            enemy.Statuses.Apply(Application(Status(new SkipTurn())), false);
            enemy.Statuses.Apply(Application(Status(new ForceTargeting())), false);

            Assert.IsTrue(enemy.SkipsTurn);
            Assert.IsTrue(enemy.ForcesTargeting);
        }

        [Test]
        public void Freeze_RaisesDamageTakenAndMakesAnEnemyActLast()
        {
            var freeze = Status(Modify(StatType.DamageTaken, multiply: 0.3f), new ActLast());
            var hero = Fighter(Team.Heroes, 500, 10);
            var first = Fighter(Team.Enemies, 100, 1);
            var second = Fighter(Team.Enemies, 100, 1);
            var model = Model(hero, new[] { first, second });
            model.Start();
            model.Enemies[0].Statuses.Apply(Application(freeze, 2), false);

            var events = model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);

            Assert.AreEqual(87, model.Enemies[0].Health);
            var actors = events.OfType<ActionPerformed>().Where(a => a.Actor.Team == Team.Enemies).Select(a => a.Actor.Definition).ToList();
            CollectionAssert.AreEqual(new[] { second, first }, actors);
        }

        [Test]
        public void Heroes_KeepTheirStatusesBetweenWaves()
        {
            var empower = Status(Modify(StatType.Damage, multiply: 0.25f));
            var hero = Fighter(Team.Heroes, 500, 50);
            var weak = Fighter(Team.Enemies, 10, 1);
            var model = Model(hero, new[] { weak }, new[] { weak });
            model.Start();
            model.Heroes[0].Statuses.Apply(Application(empower, 5), false);

            model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);

            Assert.AreEqual(1, model.Enemies.Count(e => e.IsAlive));
            Assert.IsTrue(model.Heroes[0].Statuses.Has(empower));
        }
    }
}
