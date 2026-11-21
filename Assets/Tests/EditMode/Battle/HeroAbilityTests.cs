using System.Linq;
using Featherguard.Battle;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class HeroAbilityTests
    {
        static BattleState State()
        {
            var rules = ScriptableObject.CreateInstance<BattleRules>();
            rules.baseCritChance = 0f;
            rules.damageVariance = 0f;
            return new BattleState(rules, new FakeRandom(0.5f), new BattleLog(), 0f);
        }

        static UnitDefinition Unit(Team team, int health = 100)
        {
            var unit = ScriptableObject.CreateInstance<UnitDefinition>();
            unit.team = team;
            unit.maxHealth = health;
            return unit;
        }

        static BattleUnit Spawn(BattleState state, Team team, int health = 100, int? current = null) =>
            state.Spawn(Unit(team, health), team == Team.Heroes ? state.Heroes.Count : state.Enemies.Count, Enumerable.Empty<StatModifier>(), current);

        static AbilityDefinition Ability(AbilityTarget target, params AbilityEffect[] effects)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.target = target;
            ability.effects = effects;
            return ability;
        }

        static StatusDefinition Status(params StatusBehaviour[] behaviours)
        {
            var status = ScriptableObject.CreateInstance<StatusDefinition>();
            status.behaviours = behaviours;
            return status;
        }

        static void Act(BattleState state, BattleUnit actor, AbilityDefinition ability, BattleUnit target, TimingResult timing = TimingResult.Miss) =>
            new ActionResolver(state).Execute(actor, ability, target, ActionTiming.ForAttack(timing));

        [TestCase(TimingResult.Miss, 40)]
        [TestCase(TimingResult.Good, 48)]
        [TestCase(TimingResult.Perfect, 56)]
        public void TimedHeal_GetsTheSupportBonus(TimingResult timing, int expected)
        {
            var state = State();
            var healer = Spawn(state, Team.Heroes);
            var ally = Spawn(state, Team.Heroes, 200, 100);
            var heal = Ability(AbilityTarget.Ally, new HealEffect { power = 40 });
            heal.alwaysTimed = true;

            Act(state, healer, heal, ally, timing);

            Assert.AreEqual(100 + expected, ally.Health);
        }

        [Test]
        public void UntimedHeal_IgnoresTheTiming()
        {
            var state = State();
            var healer = Spawn(state, Team.Heroes);
            var ally = Spawn(state, Team.Heroes, 200, 100);

            Act(state, healer, Ability(AbilityTarget.Ally, new HealEffect { power = 40 }), ally, TimingResult.Perfect);

            Assert.AreEqual(140, ally.Health);
        }

        [Test]
        public void OverflowHeal_BecomesAShield()
        {
            var state = State();
            var shield = Status(new AbsorbDamage());
            var healer = Spawn(state, Team.Heroes);
            var ally = Spawn(state, Team.Heroes, 100, 90);

            Act(state, healer, Ability(AbilityTarget.Ally, new HealEffect { power = 40, overflowShield = shield }), ally);

            Assert.AreEqual(100, ally.Health);
            var active = ally.Statuses.All.Single(s => s.Definition == shield);
            Assert.AreEqual(30, active.Points);
        }

        [Test]
        public void HealAtFullHealthWithoutOverflowShield_AddsNoStatus()
        {
            var state = State();
            var healer = Spawn(state, Team.Heroes);
            var ally = Spawn(state, Team.Heroes);

            Act(state, healer, Ability(AbilityTarget.Ally, new HealEffect { power = 40 }), ally);

            Assert.IsEmpty(ally.Statuses.All);
        }

        [Test]
        public void ChainDamage_JumpsToEveryOtherEnemyWeakerEachTime()
        {
            var state = State();
            var mage = Spawn(state, Team.Heroes);
            var first = Spawn(state, Team.Enemies);
            var second = Spawn(state, Team.Enemies);
            var third = Spawn(state, Team.Enemies);

            Act(state, mage, Ability(AbilityTarget.Enemy, new ChainDamageEffect { power = 20, falloff = 0.5f }), second);

            Assert.AreEqual(80, second.Health);
            Assert.AreEqual(90, first.Health);
            Assert.AreEqual(95, third.Health);
        }

        [Test]
        public void Retaliation_DamagesEachAttackerAndAppliesItsStatus()
        {
            var state = State();
            var stun = Status(new SkipTurn());
            var thorns = Status(new Retaliate { onAttacker = new StatusApplication { status = stun, turns = 1 } });
            var hero = Spawn(state, Team.Heroes);
            var enemy = Spawn(state, Team.Enemies);
            state.TryApplyStatus(hero, hero, new StatusApplication { status = thorns, turns = 2, potency = 7 });

            var claws = Ability(AbilityTarget.Enemy, new DamageEffect { power = 10 });
            claws.hits = 2;
            Act(state, enemy, claws, hero);

            Assert.AreEqual(80, hero.Health);
            Assert.AreEqual(86, enemy.Health);
            Assert.IsTrue(enemy.Statuses.Has(stun));
        }

        [Test]
        public void Retaliation_ThatKillsTheAttackerStopsItsRemainingHits()
        {
            var state = State();
            var thorns = Status(new Retaliate());
            var hero = Spawn(state, Team.Heroes);
            var enemy = Spawn(state, Team.Enemies, 5);
            state.TryApplyStatus(hero, hero, new StatusApplication { status = thorns, turns = 2, potency = 10 });

            var claws = Ability(AbilityTarget.Enemy, new DamageEffect { power = 10 });
            claws.hits = 3;
            Act(state, enemy, claws, hero);

            Assert.IsFalse(enemy.IsAlive);
            Assert.AreEqual(90, hero.Health);
        }

        [Test]
        public void PerfectTimingEffect_OnlyAppliesOnAPerfectResult()
        {
            var weaken = Status(new ModifyStats());
            var ability = Ability(AbilityTarget.Enemy, new DamageEffect { power = 10 },
                new PerfectTimingEffect { effect = new ApplyStatusEffect { application = new StatusApplication { status = weaken } } });

            foreach (var timing in new[] { TimingResult.Miss, TimingResult.Good, TimingResult.Perfect })
            {
                var state = State();
                var hero = Spawn(state, Team.Heroes);
                var enemy = Spawn(state, Team.Enemies);

                Act(state, hero, ability, enemy, timing);

                Assert.AreEqual(timing == TimingResult.Perfect, enemy.Statuses.Has(weaken), timing.ToString());
            }
        }

        [Test]
        public void TimedShield_GetsTheSupportBonus()
        {
            var state = State();
            var shield = Status(new AbsorbDamage());
            var hero = Spawn(state, Team.Heroes);
            var ward = Ability(AbilityTarget.Ally, new ApplyStatusEffect { application = new StatusApplication { status = shield, potency = 30 } });
            ward.alwaysTimed = true;

            Act(state, hero, ward, hero, TimingResult.Perfect);

            Assert.AreEqual(42, hero.Statuses.All.Single().Points);
        }

        [Test]
        public void AlwaysTimed_MakesSupportAndRageActionsTimed()
        {
            var support = Ability(AbilityTarget.Ally, new HealEffect { power = 10 });
            support.alwaysTimed = true;
            var rage = Ability(AbilityTarget.AllEnemies, new DamageEffect { power = 30 });
            rage.alwaysTimed = true;
            var hero = Unit(Team.Heroes);
            hero.attack = Ability(AbilityTarget.Enemy, new DamageEffect { power = 10 });
            hero.support = support;
            hero.special = rage;
            var enemy = Unit(Team.Enemies, 500);
            enemy.attack = Ability(AbilityTarget.Enemy, new DamageEffect { power = 0 });
            var encounter = ScriptableObject.CreateInstance<EncounterDefinition>();
            encounter.waves = new[] { new EncounterDefinition.Wave { enemies = new[] { enemy } } };
            var rules = ScriptableObject.CreateInstance<BattleRules>();
            rules.baseCritChance = 0f;
            rules.damageVariance = 0f;
            var setup = new BattleSetup { Encounter = encounter, Heroes = new[] { new HeroLoadout { Definition = hero } }, StartRage = 100f };
            var model = new BattleModel(setup, rules, new FakeRandom(0.5f));
            model.Start();

            Assert.IsTrue(model.IsTimed(model.Heroes[0], support));
            Assert.IsTrue(model.IsTimed(model.Heroes[0], rage));

            model.Act(model.Heroes[0], rage, null, TimingResult.Perfect);

            Assert.AreEqual(455, model.Enemies[0].Health);
        }
    }
}
