using System.Linq;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class CommandIntentTests
    {
        BattleUnit hero;
        BattleUnit ally;
        BattleUnit enemy;

        static AbilityDefinition Ability(AbilityTarget target)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.target = target;
            return ability;
        }

        static UnitDefinition Unit(Team team, AbilityDefinition attack = null, AbilityDefinition support = null, AbilityDefinition special = null)
        {
            var unit = ScriptableObject.CreateInstance<UnitDefinition>();
            unit.team = team;
            unit.maxHealth = 100;
            unit.attack = attack;
            unit.support = support;
            unit.special = special;
            return unit;
        }

        void Setup(AbilityDefinition support, AbilityDefinition special = null)
        {
            var rules = ScriptableObject.CreateInstance<BattleRules>();
            var state = new BattleState(rules, new FakeRandom(0.5f), new BattleLog(), 0f);
            var none = Enumerable.Empty<StatModifier>();
            hero = state.Spawn(Unit(Team.Heroes, Ability(AbilityTarget.Enemy), support, special), 0, none, null);
            ally = state.Spawn(Unit(Team.Heroes), 1, none, null);
            enemy = state.Spawn(Unit(Team.Enemies), 0, none, null);
        }

        [Test]
        public void DragOntoAnEnemy_Attacks()
        {
            Setup(Ability(AbilityTarget.Ally));

            Assert.IsTrue(CommandIntent.TryResolve(hero, enemy, true, false, out var ability, out var target));
            Assert.AreEqual(hero.Definition.attack, ability);
            Assert.AreEqual(enemy, target);
        }

        [Test]
        public void DragOntoAnAlly_UsesTheSupportOnIt()
        {
            Setup(Ability(AbilityTarget.Ally));

            Assert.IsTrue(CommandIntent.TryResolve(hero, ally, true, false, out var ability, out var target));
            Assert.AreEqual(hero.Definition.support, ability);
            Assert.AreEqual(ally, target);
        }

        [Test]
        public void Tap_UsesTheSupportOnItself()
        {
            Setup(Ability(AbilityTarget.Ally));

            Assert.IsTrue(CommandIntent.TryResolve(hero, hero, false, false, out var ability, out var target));
            Assert.AreEqual(hero.Definition.support, ability);
            Assert.AreEqual(hero, target);
        }

        [Test]
        public void Tap_WithAPartySupport_TargetsTheWholeParty()
        {
            Setup(Ability(AbilityTarget.AllAllies));

            Assert.IsTrue(CommandIntent.TryResolve(hero, hero, false, false, out var ability, out var target));
            Assert.AreEqual(hero.Definition.support, ability);
            Assert.IsNull(target);
        }

        [Test]
        public void Tap_WithoutASupport_DoesNothing()
        {
            Setup(null);

            Assert.IsFalse(CommandIntent.TryResolve(hero, hero, false, false, out _, out _));
        }

        [Test]
        public void DropOnNothing_DoesNothing()
        {
            Setup(Ability(AbilityTarget.Ally));

            Assert.IsFalse(CommandIntent.TryResolve(hero, null, true, false, out _, out _));
        }

        [Test]
        public void ArmedRage_OnAnEnemyUsesTheRageAction()
        {
            Setup(Ability(AbilityTarget.Ally), Ability(AbilityTarget.Enemy));

            Assert.IsTrue(CommandIntent.TryResolve(hero, enemy, true, true, out var ability, out var target));
            Assert.AreEqual(hero.Definition.special, ability);
            Assert.AreEqual(enemy, target);
            Assert.IsFalse(CommandIntent.TryResolve(hero, hero, false, true, out _, out _));
        }

        [Test]
        public void ArmedRage_ThatHitsEveryEnemy_WorksWithATap()
        {
            Setup(Ability(AbilityTarget.Ally), Ability(AbilityTarget.AllEnemies));

            Assert.IsTrue(CommandIntent.TryResolve(hero, hero, false, true, out var ability, out var target));
            Assert.AreEqual(hero.Definition.special, ability);
            Assert.IsNull(target);
        }
    }
}
