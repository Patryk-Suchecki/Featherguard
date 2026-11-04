using Featherguard.Battle;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class CombatMathTests
    {
        [Test]
        public void StatBlock_CombinesAdditiveAndMultiplicativeModifiers()
        {
            var stats = new StatBlock(100, 0.15f, 1.5f);
            stats.Add(new StatModifier(StatType.MaxHealth, add: 20f));
            stats.Add(new StatModifier(StatType.MaxHealth, multiply: 0.5f));

            Assert.AreEqual(180f, stats.Get(StatType.MaxHealth), 0.001f);
            Assert.AreEqual(0.15f, stats.Get(StatType.CritChance), 0.001f);
        }

        [Test]
        public void StatBlock_ExtraModifiersApplyWithoutBeingStored()
        {
            var stats = new StatBlock(100, 0.15f, 1.5f);
            var weaken = new[] { new StatModifier(StatType.Damage, multiply: -0.3f) };

            Assert.AreEqual(0.7f, stats.Get(StatType.Damage, weaken), 0.001f);
            Assert.AreEqual(1f, stats.Get(StatType.Damage), 0.001f);
        }

        [Test]
        public void StatBlock_NeverGoesNegative()
        {
            var stats = new StatBlock(100, 0.15f, 1.5f);
            stats.Add(new StatModifier(StatType.DamageTaken, multiply: -2f));

            Assert.AreEqual(0f, stats.Get(StatType.DamageTaken));
        }

        [Test]
        public void DamageCalculator_AppliesMultiplierVarianceAndCrit()
        {
            var crit = DamageCalculator.Roll(20, 1.5f, 0.5f, 2f, 0.1f, new FakeRandom(0.5f, 0.1f, 0.5f));
            var normal = DamageCalculator.Roll(20, 1f, 0.5f, 2f, 0.1f, new FakeRandom(0.5f, 0.9f, 1f));

            Assert.IsTrue(crit.Critical);
            Assert.AreEqual(60, crit.Amount);
            Assert.IsFalse(normal.Critical);
            Assert.AreEqual(22, normal.Amount);
        }

        [Test]
        public void RageMeter_ClampsAndFiresReadyOnce()
        {
            var rage = new RageMeter(100f);
            int fired = 0;
            rage.BecameReady += () => fired++;

            rage.Add(60f);
            rage.Add(60f);
            rage.Add(10f);

            Assert.AreEqual(100f, rage.Value);
            Assert.IsTrue(rage.Ready);
            Assert.AreEqual(1, fired);
            rage.Consume();
            Assert.AreEqual(0f, rage.Fraction);
        }

        [Test]
        public void StatusContainer_OwnTurnBuffSurvivesUntilNextOwnTurnEnds()
        {
            var shield = ScriptableObject.CreateInstance<StatusDefinition>();
            var statuses = new StatusContainer();
            statuses.Apply(new StatusApplication { status = shield, turns = 1 }, true);

            statuses.EndTurn();
            Assert.IsTrue(statuses.Has(shield));
            statuses.EndTurn();
            Assert.IsFalse(statuses.Has(shield));
        }

        [Test]
        public void StatusContainer_OpponentDebuffExpiresAtEndOfOwnersTurn()
        {
            var stun = ScriptableObject.CreateInstance<StatusDefinition>();
            var statuses = new StatusContainer();
            statuses.Apply(new StatusApplication { status = stun, turns = 1 }, false);

            Assert.IsTrue(statuses.Has(stun));
            statuses.EndTurn();
            Assert.IsFalse(statuses.Has(stun));
        }

        [Test]
        public void StatusContainer_ReapplyingKeepsStrongestValues()
        {
            var poison = ScriptableObject.CreateInstance<StatusDefinition>();
            poison.damagePerTurn = true;
            var statuses = new StatusContainer();
            statuses.Apply(new StatusApplication { status = poison, turns = 3, potency = 4 }, false);
            statuses.Apply(new StatusApplication { status = poison, turns = 1, potency = 6 }, false);

            Assert.AreEqual(1, statuses.All.Count);
            Assert.AreEqual(3, statuses.All[0].Turns);
            Assert.AreEqual(6, statuses.Potency(s => s.damagePerTurn));
        }
    }
}
