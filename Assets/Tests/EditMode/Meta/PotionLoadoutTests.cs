using Featherguard.Battle;
using Featherguard.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class PotionLoadoutTests
    {
        PotionDefinition healing;
        PotionDefinition flask;
        PotionConfig config;
        MemorySaveStore store;
        MetaProgression meta;
        PotionBag bag;

        static T Named<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            return asset;
        }

        [SetUp]
        public void SetUp()
        {
            healing = Named<PotionDefinition>("HealingDraught");
            flask = Named<PotionDefinition>("FireFlask");
            var healingRecipe = Named<RecipeDefinition>("HealingDraught");
            healingRecipe.potion = healing;
            var flaskRecipe = Named<RecipeDefinition>("FireFlask");
            flaskRecipe.potion = flask;
            config = ScriptableObject.CreateInstance<PotionConfig>();
            config.recipes = new[] { healingRecipe, flaskRecipe };
            config.battleSlots = 3;
            store = new MemorySaveStore();
            meta = new MetaProgression(ScriptableObject.CreateInstance<MetaConfig>(), store);
            bag = meta.CreatePotionBag(config);
        }

        [Test]
        public void Picking_StopsAtTheSlotsAndAtTheCountOwned()
        {
            bag.Add(healing, 2);
            bag.Add(flask, 5);
            var loadout = meta.CreateLoadout(config);

            Assert.IsTrue(loadout.Add(healing));
            Assert.IsTrue(loadout.Add(healing));
            Assert.IsFalse(loadout.Add(healing));
            Assert.IsTrue(loadout.Add(flask));
            Assert.IsFalse(loadout.Add(flask));

            Assert.AreEqual(3, loadout.Total);
            Assert.AreEqual(2, loadout.Count(healing));
        }

        [Test]
        public void ThePick_IsSavedAndShrinksWhenPotionsAreSpent()
        {
            bag.Add(healing, 2);
            var loadout = meta.CreateLoadout(config);
            loadout.Add(healing);
            loadout.Add(healing);

            bag.Remove(healing);
            var reloaded = new MetaProgression(ScriptableObject.CreateInstance<MetaConfig>(), store).CreateLoadout(config);

            Assert.AreEqual(1, reloaded.Count(healing));
            Assert.IsTrue(loadout.Remove(healing));
            Assert.AreEqual(0, loadout.Total);
        }
    }
}
