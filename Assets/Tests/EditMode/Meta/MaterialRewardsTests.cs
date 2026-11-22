using System.Linq;
using Featherguard.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class MaterialRewardsTests
    {
        LandDefinition land;
        MaterialDefinition common;
        MaterialDefinition uncommon;
        MaterialDefinition rare;
        MaterialDefinition otherLand;
        MaterialDefinition epic;
        MaterialDefinition legendary;
        MaterialConfig config;
        ActivityDefinition activity;

        MaterialDefinition Material(MaterialRarity rarity, LandDefinition from)
        {
            var material = ScriptableObject.CreateInstance<MaterialDefinition>();
            material.rarity = rarity;
            material.land = from;
            return material;
        }

        [SetUp]
        public void SetUp()
        {
            land = ScriptableObject.CreateInstance<LandDefinition>();
            common = Material(MaterialRarity.Common, land);
            uncommon = Material(MaterialRarity.Uncommon, land);
            rare = Material(MaterialRarity.Rare, land);
            otherLand = Material(MaterialRarity.Common, ScriptableObject.CreateInstance<LandDefinition>());
            epic = Material(MaterialRarity.Epic, null);
            legendary = Material(MaterialRarity.Legendary, null);
            config = ScriptableObject.CreateInstance<MaterialConfig>();
            config.materials = new[] { otherLand, common, uncommon, rare, epic, legendary };
            activity = ScriptableObject.CreateInstance<ActivityDefinition>();
            activity.land = land;
            activity.type = ActivityType.SingleFight;
            activity.threat = 1;
            activity.reward = RewardKind.Materials;
        }

        [Test]
        public void AFightWithAnotherReward_GivesNoMaterials()
        {
            activity.reward = RewardKind.Gold;

            Assert.IsEmpty(MaterialRewards.Roll(activity, 1f, config, new FakeRandom(0.5f)));
        }

        [Test]
        public void AWin_GivesTheTypesAmountOfTheLandsMaterials()
        {
            var drops = MaterialRewards.Roll(activity, 1f, config, new FakeRandom(0.5f));

            Assert.AreEqual(1, drops.Count);
            Assert.AreSame(common, drops[0].Material);
            Assert.AreEqual(config.amountByType[(int)ActivityType.SingleFight], drops[0].Count);
        }

        [Test]
        public void Daring_MultipliesTheAmount()
        {
            var drops = MaterialRewards.Roll(activity, 1.5f, config, new FakeRandom(0.5f));

            Assert.AreEqual(3, drops.Sum(d => d.Count));
        }

        [Test]
        public void AHighRoll_GivesTheLandsRareMaterial()
        {
            var drops = MaterialRewards.Roll(activity, 1f, config, new FakeRandom(0.5f, 0.99f, 0f, 0.99f, 0f));

            Assert.AreSame(rare, drops.Single().Material);
            Assert.AreEqual(2, drops.Single().Count);
        }

        [Test]
        public void HigherThreat_RaisesTheUncommonAndRareWeights()
        {
            Assert.AreEqual(config.commonWeight, MaterialRewards.Weight(MaterialRarity.Common, 5, config));
            Assert.AreEqual(config.uncommonWeight + 4 * config.uncommonPerThreat, MaterialRewards.Weight(MaterialRarity.Uncommon, 5, config));
            Assert.AreEqual(config.rareWeight + 4 * config.rarePerThreat, MaterialRewards.Weight(MaterialRarity.Rare, 5, config));
        }

        [Test]
        public void ALowUniversalRoll_AddsAnEpicMaterial()
        {
            var drops = MaterialRewards.Roll(activity, 1f, config, new FakeRandom(0.5f, 0.5f, 0f, 0.5f, 0f, 0f, 0f));

            CollectionAssert.AreEquivalent(new[] { common, epic }, drops.Select(d => d.Material));
            Assert.IsFalse(drops.Any(d => d.Material == otherLand || d.Material == legendary));
        }
    }
}
