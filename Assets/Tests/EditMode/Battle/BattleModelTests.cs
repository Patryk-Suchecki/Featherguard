using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class BattleModelTests
    {
        static AbilityDefinition Strike(int power, AbilityTarget target = AbilityTarget.Enemy, int hits = 1)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.displayName = "Strike";
            ability.target = target;
            ability.hits = hits;
            ability.effects = new AbilityEffect[] { new DamageEffect { power = power } };
            return ability;
        }

        static AbilityDefinition Apply(StatusDefinition status, AbilityTarget target)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.target = target;
            ability.effects = new AbilityEffect[] { new ApplyStatusEffect { application = new StatusApplication { status = status, turns = 1 } } };
            return ability;
        }

        static UnitDefinition Unit(string name, Team team, int health, AbilityDefinition attack, AbilityDefinition support = null, AbilityDefinition special = null)
        {
            var unit = ScriptableObject.CreateInstance<UnitDefinition>();
            unit.name = name;
            unit.displayName = name;
            unit.team = team;
            unit.maxHealth = health;
            unit.attack = attack;
            unit.support = support;
            unit.special = special;
            return unit;
        }

        static EncounterDefinition Encounter(params UnitDefinition[][] waves)
        {
            var encounter = ScriptableObject.CreateInstance<EncounterDefinition>();
            encounter.displayName = "Test";
            encounter.waves = waves.Select(w => new EncounterDefinition.Wave { enemies = w }).ToArray();
            return encounter;
        }

        static BattleModel Create(EncounterDefinition encounter, float startRage = 0f, params UnitDefinition[] heroes)
        {
            var rules = ScriptableObject.CreateInstance<BattleRules>();
            rules.baseCritChance = 0f;
            rules.damageVariance = 0f;
            var setup = new BattleSetup
            {
                Encounter = encounter,
                Heroes = heroes.Select(h => new HeroLoadout { Definition = h }).ToList(),
                StartRage = startRage,
            };
            return new BattleModel(setup, rules, new FakeRandom(0.5f));
        }

        static BattleModel CreateWithBlocks(EncounterDefinition encounter, params UnitDefinition[] heroes)
        {
            var rules = ScriptableObject.CreateInstance<BattleRules>();
            rules.baseCritChance = 0f;
            rules.damageVariance = 0f;
            var setup = new BattleSetup
            {
                Encounter = encounter,
                Heroes = heroes.Select(h => new HeroLoadout { Definition = h }).ToList(),
                AwaitBlocks = true,
            };
            return new BattleModel(setup, rules, new FakeRandom(0.5f));
        }

        static void AutoPlay(BattleModel model, int maxActions = 200)
        {
            for (int i = 0; i < maxActions && !model.IsFinished; i++)
            {
                var hero = model.Heroes.First(model.CanCommand);
                var attack = hero.Definition.attack;
                var target = model.Candidates(hero, attack).OrderBy(e => e.Health).First();
                Assert.IsNotEmpty(model.Act(hero, attack, target));
            }
        }

        [Test]
        public void Start_SpawnsHeroesAndFirstWaveThenWaitsForInput()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(10));
            var enemy = Unit("Enemy", Team.Enemies, 50, Strike(5));
            var model = Create(Encounter(new[] { enemy, enemy }), 0f, hero);

            var events = model.Start();

            Assert.IsTrue(model.AwaitingInput);
            Assert.AreEqual(3, events.OfType<UnitSpawned>().Count());
            Assert.AreEqual(new[] { 0, 1 }, model.Enemies.Select(e => e.Slot).ToArray());
            Assert.IsInstanceOf<TurnStarted>(events.Last());
        }

        [Test]
        public void Act_RejectsInvalidCommands()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(10));
            var enemy = Unit("Enemy", Team.Enemies, 50, Strike(5));
            var model = Create(Encounter(new[] { enemy }), 0f, hero, hero);
            model.Start();
            var first = model.Heroes[0];
            var foe = model.Enemies[0];

            Assert.IsEmpty(model.Act(first, first.Definition.attack, model.Heroes[1]));
            Assert.IsEmpty(model.Act(foe, foe.Definition.attack, first));
            Assert.IsNotEmpty(model.Act(first, first.Definition.attack, foe));
            Assert.IsEmpty(model.Act(first, first.Definition.attack, foe));
        }

        [Test]
        public void Act_EndsPlayerTurnAndResolvesEnemyTurnSynchronously()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(10));
            var enemy = Unit("Enemy", Team.Enemies, 50, Strike(7));
            var model = Create(Encounter(new[] { enemy }), 0f, hero);
            model.Start();

            var events = model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);

            Assert.AreEqual(40, model.Enemies[0].Health);
            Assert.AreEqual(93, model.Heroes[0].Health);
            Assert.AreEqual(2, events.OfType<ActionPerformed>().Count());
            Assert.AreEqual(Team.Enemies, events.OfType<TurnStarted>().First().Team);
            Assert.IsTrue(model.AwaitingInput);
            Assert.IsTrue(model.Heroes[0].CanAct);
        }

        [Test]
        public void Action_RecordsOneImpactPerHitWithOutcomes()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(10, hits: 3));
            var enemy = Unit("Enemy", Team.Enemies, 100, Strike(1));
            var model = Create(Encounter(new[] { enemy }), 0f, hero);
            model.Start();

            var action = model.Act(model.Heroes[0], hero.attack, model.Enemies[0]).OfType<ActionPerformed>().First();

            Assert.AreEqual(new[] { 0, 1, 2 }, action.Impacts.Select(i => i.Hit).ToArray());
            Assert.IsTrue(action.Impacts.All(i => i.Outcomes.OfType<Damaged>().Single().Amount == 10));
            Assert.AreEqual(70, action.Impacts.Last().Outcomes.OfType<UnitChanged>().Last().State.Health);
        }

        [Test]
        public void Taunt_ForcesHeroesToTargetTheTaunter()
        {
            var taunt = ScriptableObject.CreateInstance<StatusDefinition>();
            taunt.forcesTargeting = true;
            var hero = Unit("Hero", Team.Heroes, 100, Strike(10));
            var guard = Unit("Guard", Team.Enemies, 100, Strike(1));
            var model = Create(Encounter(new[] { guard, guard }), 0f, hero);
            model.Start();
            model.Enemies[1].Statuses.Apply(new StatusApplication { status = taunt, turns = 2 }, false);

            Assert.IsFalse(model.CanUse(model.Heroes[0], hero.attack, model.Enemies[0]));
            Assert.IsTrue(model.CanUse(model.Heroes[0], hero.attack, model.Enemies[1]));
        }

        [Test]
        public void Special_RequiresFullRageAndConsumesIt()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(10), special: Strike(30, AbilityTarget.AllEnemies));
            var enemy = Unit("Enemy", Team.Enemies, 100, Strike(1));
            var model = Create(Encounter(new[] { enemy, enemy }), 100f, hero);
            model.Start();

            var events = model.Act(model.Heroes[0], hero.special, null);

            Assert.IsTrue(events.OfType<ActionPerformed>().First().Special);
            Assert.IsTrue(model.Enemies.All(e => e.Health == 70));
            Assert.IsFalse(model.Rage.Ready);
            Assert.IsFalse(model.CanUse(model.Heroes[0], hero.special, null));
        }

        [TestCase(TimingResult.Miss, 460)]
        [TestCase(TimingResult.Good, 450)]
        [TestCase(TimingResult.Perfect, 440)]
        public void AttackTiming_ScalesDamageOnEveryHit(TimingResult timing, int healthLeft)
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(20, hits: 2));
            var enemy = Unit("Enemy", Team.Enemies, 500, Strike(0));
            var model = Create(Encounter(new[] { enemy }), 0f, hero);
            model.Start();

            model.Act(model.Heroes[0], hero.attack, model.Enemies[0], timing);

            Assert.AreEqual(healthLeft, model.Enemies[0].Health);
        }

        [Test]
        public void AttackTiming_PerfectAddsRageOncePerAction()
        {
            float RageAfter(TimingResult timing)
            {
                var hero = Unit("Hero", Team.Heroes, 100, Strike(20, hits: 2));
                var enemy = Unit("Enemy", Team.Enemies, 500, Strike(0));
                var model = Create(Encounter(new[] { enemy }), 0f, hero);
                model.Start();
                model.Act(model.Heroes[0], hero.attack, model.Enemies[0], timing);
                return model.Rage.Value;
            }

            var rules = ScriptableObject.CreateInstance<BattleRules>();
            float damageRage = 60 * rules.heroRagePerDamage;
            Assert.AreEqual(damageRage + rules.perfectTimingRage, RageAfter(TimingResult.Perfect), 0.001f);
            Assert.AreEqual(50 * rules.heroRagePerDamage, RageAfter(TimingResult.Good), 0.001f);
        }

        [Test]
        public void Block_StopsBeforeADamagingEnemyActionAndResumes()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(1));
            var enemy = Unit("Enemy", Team.Enemies, 500, Strike(10));
            var model = CreateWithBlocks(Encounter(new[] { enemy }), hero);
            model.Start();

            var events = model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);

            Assert.IsTrue(model.AwaitingBlock);
            Assert.AreEqual(100, model.Heroes[0].Health);
            var windup = events.OfType<EnemyWindup>().Single();
            Assert.AreEqual(model.Enemies[0], windup.Actor);
            Assert.AreEqual(model.Heroes[0], windup.Target);

            model.Block(TimingResult.Miss);

            Assert.AreEqual(90, model.Heroes[0].Health);
            Assert.IsTrue(model.AwaitingInput);
        }

        [TestCase(TimingResult.Miss, 80)]
        [TestCase(TimingResult.Good, 86)]
        [TestCase(TimingResult.Perfect, 92)]
        public void Block_ReducesDamageForEveryHeroHit(TimingResult timing, int healthLeft)
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(1));
            var enemy = Unit("Enemy", Team.Enemies, 500, Strike(20, AbilityTarget.AllEnemies));
            var model = CreateWithBlocks(Encounter(new[] { enemy }), hero, hero);
            model.Start();
            model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);
            model.Act(model.Heroes[1], hero.attack, model.Enemies[0]);

            model.Block(timing);

            Assert.IsTrue(model.Heroes.All(h => h.Health == healthLeft));
        }

        [Test]
        public void Block_PerfectAddsRageOncePerAction()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(0));
            var enemy = Unit("Enemy", Team.Enemies, 500, Strike(20, AbilityTarget.AllEnemies));
            var model = CreateWithBlocks(Encounter(new[] { enemy }), hero, hero);
            model.Start();
            model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);
            model.Act(model.Heroes[1], hero.attack, model.Enemies[0]);

            model.Block(TimingResult.Perfect);

            var rules = ScriptableObject.CreateInstance<BattleRules>();
            Assert.AreEqual(2 * 8 * rules.enemyRagePerDamage + rules.perfectTimingRage, model.Rage.Value, 0.001f);
        }

        [Test]
        public void Block_EnemyActionsWithoutDamageDoNotStop()
        {
            var weaken = ScriptableObject.CreateInstance<StatusDefinition>();
            var hero = Unit("Hero", Team.Heroes, 100, Strike(1));
            var enemy = Unit("Enemy", Team.Enemies, 500, Apply(weaken, AbilityTarget.AllEnemies));
            var model = CreateWithBlocks(Encounter(new[] { enemy }), hero);
            model.Start();

            var events = model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);

            Assert.IsTrue(model.AwaitingInput);
            Assert.IsEmpty(events.OfType<EnemyWindup>());
            Assert.IsTrue(events.OfType<StatusApplied>().Any());
        }

        [Test]
        public void Block_EliteStopsForEachOfItsActions()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(1));
            var elite = Unit("Elite", Team.Enemies, 500, Strike(10));
            elite.ai.actionsPerTurn = 2;
            var model = CreateWithBlocks(Encounter(new[] { elite }), hero);
            model.Start();

            model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);
            Assert.IsTrue(model.AwaitingBlock);
            model.Block(TimingResult.Miss);
            Assert.IsTrue(model.AwaitingBlock);
            model.Block(TimingResult.Perfect);

            Assert.IsTrue(model.AwaitingInput);
            Assert.AreEqual(86, model.Heroes[0].Health);
        }

        [Test]
        public void Block_IsIgnoredWhenNotAwaitingOne()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(1));
            var enemy = Unit("Enemy", Team.Enemies, 500, Strike(10));
            var model = CreateWithBlocks(Encounter(new[] { enemy }), hero);
            model.Start();

            Assert.IsEmpty(model.Block(TimingResult.Perfect));
            Assert.IsTrue(model.AwaitingInput);
        }

        [Test]
        public void AttackTiming_IsIgnoredBySupportAndTheRageAction()
        {
            var heal = ScriptableObject.CreateInstance<AbilityDefinition>();
            heal.target = AbilityTarget.Ally;
            heal.effects = new AbilityEffect[] { new HealEffect { power = 10 } };
            var hero = Unit("Hero", Team.Heroes, 100, Strike(10), heal, Strike(30));
            var enemy = Unit("Enemy", Team.Enemies, 500, Strike(0));
            var model = Create(Encounter(new[] { enemy }), 100f, hero);
            model.Start();

            var events = model.Act(model.Heroes[0], hero.special, model.Enemies[0], TimingResult.Perfect);

            Assert.AreEqual(470, model.Enemies[0].Health);
            Assert.AreEqual(TimingResult.Miss, events.OfType<ActionPerformed>().First().Timing.Attack);

            events = model.Act(model.Heroes[0], heal, model.Heroes[0], TimingResult.Perfect);

            Assert.AreEqual(TimingResult.Miss, events.OfType<ActionPerformed>().First().Timing.Attack);
        }

        [Test]
        public void Waves_AdvanceAfterClearAndBattleEndsInVictory()
        {
            var hero = Unit("Hero", Team.Heroes, 200, Strike(40));
            var weak = Unit("Weak", Team.Enemies, 30, Strike(2));
            var model = Create(Encounter(new[] { weak }, new[] { weak, weak }), 0f, hero);
            var kills = new List<BattleUnit>();
            model.EnemyKilled += kills.Add;
            model.Start();

            var events = model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);
            Assert.AreEqual(1, events.OfType<WaveStarted>().Single().Wave);
            Assert.AreEqual(1, events.OfType<UnitRemoved>().Count());

            AutoPlay(model);

            Assert.IsTrue(model.IsFinished);
            Assert.IsTrue(model.Result.Victory);
            Assert.AreEqual(3, kills.Count);
        }

        [Test]
        public void Battle_EndsInDefeatWhenHeroesFall()
        {
            var hero = Unit("Hero", Team.Heroes, 20, Strike(1));
            var brute = Unit("Brute", Team.Enemies, 500, Strike(15));
            var model = Create(Encounter(new[] { brute }), 0f, hero);
            model.Start();

            AutoPlay(model);

            Assert.IsTrue(model.IsFinished);
            Assert.IsFalse(model.Result.Victory);
            Assert.IsFalse(model.Result.Heroes[0].IsAlive);
        }

        [Test]
        public void Stun_SkipsHeroTurnAndEnemyActsAgain()
        {
            var stun = ScriptableObject.CreateInstance<StatusDefinition>();
            stun.skipsTurn = true;
            var hero = Unit("Hero", Team.Heroes, 100, Strike(1));
            var stunner = Unit("Stunner", Team.Enemies, 100, Strike(3), special: Apply(stun, AbilityTarget.AllEnemies));
            stunner.ai.specialEvery = 2;
            var model = Create(Encounter(new[] { stunner }), 0f, hero);
            model.Start();

            var events = model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);

            Assert.IsTrue(events.OfType<Stunned>().Any());
            Assert.AreEqual(2, events.OfType<TurnStarted>().Count(t => t.Team == Team.Enemies));
            Assert.AreEqual(97, model.Heroes[0].Health);
            Assert.IsTrue(model.AwaitingInput);
        }

        [Test]
        public void Boss_EnragesOnceAndSummonsIntoFreeSlot()
        {
            var minion = Unit("Minion", Team.Enemies, 10, Strike(1));
            var boss = Unit("Boss", Team.Enemies, 100, Strike(1));
            boss.boss = new BossProfile { enrageAt = 0.5f, enrageDamageBonus = 0.5f, enrageSummons = new[] { minion } };
            var hero = Unit("Hero", Team.Heroes, 500, Strike(60));
            var model = Create(Encounter(new[] { boss }), 0f, hero);
            model.Start();

            var events = model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);

            Assert.AreEqual(1, events.OfType<Enraged>().Count());
            var summon = events.OfType<UnitSpawned>().Single();
            Assert.IsTrue(summon.Summoned);
            Assert.AreEqual(1, summon.Unit.Slot);
            Assert.AreEqual(1.5f, model.Enemies[0].Stat(StatType.Damage), 0.001f);
        }

        [Test]
        public void Elite_ActsTwiceInEachEnemyTurn()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(1));
            var elite = Unit("Elite", Team.Enemies, 50, Strike(7));
            elite.ai.actionsPerTurn = 2;
            var model = Create(Encounter(new[] { elite }), 0f, hero);
            model.Start();

            var events = model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);

            Assert.AreEqual(86, model.Heroes[0].Health);
            Assert.AreEqual(2, events.OfType<ActionPerformed>().Count(a => a.Actor == model.Enemies[0]));
            Assert.IsTrue(model.AwaitingInput);
        }

        [Test]
        public void Elite_DueSpecialReplacesItsLastAction()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(1));
            var elite = Unit("Elite", Team.Enemies, 500, Strike(5), special: Strike(20));
            elite.ai.actionsPerTurn = 2;
            elite.ai.specialEvery = 3;
            var model = Create(Encounter(new[] { elite }), 0f, hero);
            model.Start();

            model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);
            Assert.AreEqual(75, model.Heroes[0].Health);

            model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);
            Assert.AreEqual(65, model.Heroes[0].Health);
        }

        [Test]
        public void EnemySingleTargetSpecial_PicksAHeroTarget()
        {
            var hero = Unit("Hero", Team.Heroes, 100, Strike(1));
            var caster = Unit("Caster", Team.Enemies, 100, Strike(1), special: Strike(25));
            caster.ai.specialEvery = 1;
            var model = Create(Encounter(new[] { caster }), 0f, hero);
            model.Start();

            model.Act(model.Heroes[0], hero.attack, model.Enemies[0]);

            Assert.AreEqual(75, model.Heroes[0].Health);
        }

        [Test]
        public void Kill_IsReportedOnceEvenWithRepeatedDamageEffects()
        {
            var cleave = Strike(50);
            cleave.effects = new AbilityEffect[] { new DamageEffect { power = 50 }, new DamageEffect { power = 50 } };
            var hero = Unit("Hero", Team.Heroes, 100, cleave);
            var weak = Unit("Weak", Team.Enemies, 30, Strike(1));
            var model = Create(Encounter(new[] { weak, weak }), 0f, hero);
            int kills = 0;
            model.EnemyKilled += _ => kills++;
            model.Start();

            var events = model.Act(model.Heroes[0], cleave, model.Enemies[0]);

            Assert.AreEqual(1, kills);
            Assert.AreEqual(1, events.OfType<ActionPerformed>().First().Impacts.Single().Outcomes.OfType<Died>().Count());
        }

        [Test]
        public void Simulation_FullBattleRunsHeadless()
        {
            var hero = Unit("Hero", Team.Heroes, 150, Strike(18));
            var enemy = Unit("Enemy", Team.Enemies, 60, Strike(6));
            var model = Create(Encounter(new[] { enemy, enemy, enemy }, new[] { enemy, enemy }), 0f, hero, hero, hero);
            model.Start();

            AutoPlay(model);

            Assert.IsTrue(model.Result.Victory);
            Assert.Greater(model.Turn, 1);
        }
    }
}
