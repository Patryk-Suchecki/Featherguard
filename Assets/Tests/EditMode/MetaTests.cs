using Featherguard.Battle;
using Featherguard.Meta;
using Featherguard.Run;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class MetaTests
    {
        static (MetaProgression meta, PerkDefinition perk, MemorySaveStore store) Create(int feathers)
        {
            var perk = ScriptableObject.CreateInstance<PerkDefinition>();
            perk.name = "Luck";
            perk.maxLevel = 2;
            perk.baseCost = 3;
            perk.effect = new LuckPerk { perLevel = 1 };
            var config = ScriptableObject.CreateInstance<MetaConfig>();
            config.perks = new[] { perk };
            var store = new MemorySaveStore();
            store.Save(new SaveData { feathers = feathers });
            return (new MetaProgression(config, store), perk, store);
        }

        [Test]
        public void Buying_ScalesCostAndStopsAtMaxLevel()
        {
            var (meta, perk, _) = Create(20);

            Assert.IsTrue(meta.Buy(perk));
            Assert.AreEqual(6, meta.Cost(perk));
            Assert.IsTrue(meta.Buy(perk));
            Assert.IsTrue(meta.IsMaxed(perk));
            Assert.IsFalse(meta.Buy(perk));
            Assert.AreEqual(11, meta.Data.feathers);
        }

        [Test]
        public void Buying_PersistsToStore()
        {
            var (meta, perk, store) = Create(5);
            meta.Buy(perk);

            var reloaded = new MetaProgression(meta.Config, store);

            Assert.AreEqual(1, reloaded.Level(perk));
            Assert.AreEqual(2, reloaded.Data.feathers);
        }

        [Test]
        public void ApplyTo_GrantsPerkEffectsToRun()
        {
            var (meta, perk, _) = Create(10);
            meta.Buy(perk);
            var run = ScriptableObject.CreateInstance<RunDefinition>();
            run.upgrades = new UpgradeDefinition[0];
            run.curses = new UpgradeDefinition[0];
            var session = new RunSession(run, new UnitDefinition[0], new FakeRandom(0.5f));

            meta.ApplyTo(session);

            Assert.AreEqual(1, session.Luck);
        }
    }
}
