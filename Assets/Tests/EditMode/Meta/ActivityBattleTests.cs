using System.Linq;
using Featherguard.Battle;
using Featherguard.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class ActivityBattleTests
    {
        ActivityDefinition activity;
        UnitDefinition[] party;

        [SetUp]
        public void SetUp()
        {
            var encounter = ScriptableObject.CreateInstance<EncounterDefinition>();
            encounter.enemyHealth = 1.5f;
            encounter.enemyDamage = 1.2f;
            encounter.goldReward = 30;
            encounter.waves = new[] { new EncounterDefinition.Wave(), new EncounterDefinition.Wave() };
            activity = ScriptableObject.CreateInstance<ActivityDefinition>();
            activity.encounter = encounter;
            party = new[] { ScriptableObject.CreateInstance<UnitDefinition>(), ScriptableObject.CreateInstance<UnitDefinition>() };
        }

        [Test]
        public void Setup_FightsTheActivityEncounterWithTheParty()
        {
            var setup = ActivityBattle.CreateSetup(activity, party);

            Assert.AreSame(activity.encounter, setup.Encounter);
            CollectionAssert.AreEqual(party, setup.Heroes.Select(h => h.Definition));
            Assert.AreEqual(1.5f, setup.EnemyHealthScale);
            Assert.AreEqual(1.2f, setup.EnemyDamageScale);
            Assert.IsFalse(setup.Elite);
        }

        [Test]
        public void EliteFight_IsAnEliteBattle()
        {
            activity.type = ActivityType.EliteFight;

            Assert.IsTrue(ActivityBattle.CreateSetup(activity, party).Elite);
        }

        [Test]
        public void WavesAndGold_ComeFromTheEncounter()
        {
            Assert.AreEqual(2, ActivityBattle.Waves(activity));
            Assert.AreEqual(30, ActivityBattle.Gold(activity, 1f));
            Assert.AreEqual(45, ActivityBattle.Gold(activity, 1.5f));
        }
    }
}
