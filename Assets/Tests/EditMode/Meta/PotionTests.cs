using System.Linq;
using Featherguard.Battle;
using Featherguard.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class PotionTests
    {
        MaterialDefinition flax;
        MaterialDefinition amber;
        PotionDefinition healing;
        PotionDefinition cleansing;
        RecipeDefinition healingRecipe;
        RecipeDefinition cleansingRecipe;
        PotionConfig config;
        MemorySaveStore store;
        MetaProgression meta;
        MaterialStorage storage;
        PotionBag bag;
        RecipeBook book;
        PotionBrewer brewer;

        static T Named<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            return asset;
        }

        static RecipeDefinition Recipe(string name, PotionDefinition potion, bool known, params (MaterialDefinition Material, int Amount)[] ingredients)
        {
            var recipe = Named<RecipeDefinition>(name);
            recipe.potion = potion;
            recipe.knownFromStart = known;
            recipe.ingredients = ingredients.Select(i => new Ingredient { material = i.Material, amount = i.Amount }).ToArray();
            return recipe;
        }

        [SetUp]
        public void SetUp()
        {
            flax = Named<MaterialDefinition>("Flax");
            amber = Named<MaterialDefinition>("Amber");
            healing = Named<PotionDefinition>("HealingDraught");
            cleansing = Named<PotionDefinition>("CleansingBrew");
            healingRecipe = Recipe("HealingDraught", healing, true, (flax, 2));
            cleansingRecipe = Recipe("CleansingBrew", cleansing, false, (flax, 2), (amber, 1));
            config = ScriptableObject.CreateInstance<PotionConfig>();
            config.recipes = new[] { healingRecipe, cleansingRecipe };
            config.maxBrew = 5;
            var materials = ScriptableObject.CreateInstance<MaterialConfig>();
            materials.materials = new[] { flax, amber };
            store = new MemorySaveStore();
            meta = new MetaProgression(ScriptableObject.CreateInstance<MetaConfig>(), store);
            storage = meta.CreateStorage(materials);
            bag = meta.CreatePotionBag(config);
            book = meta.CreateRecipeBook(config);
            brewer = new PotionBrewer(book, storage, bag, config.maxBrew);
        }

        [Test]
        public void Brew_TakesTheMaterialsAndAddsThePotions()
        {
            storage.Add(flax, 5);

            Assert.IsTrue(brewer.Brew(healingRecipe, 2));

            Assert.AreEqual(1, storage.Count(flax));
            Assert.AreEqual(2, bag.Count(healing));
        }

        [Test]
        public void Brew_FailsWithoutEnoughMaterials()
        {
            storage.Add(flax, 3);

            Assert.IsFalse(brewer.Brew(healingRecipe, 2));

            Assert.AreEqual(3, storage.Count(flax));
            Assert.AreEqual(0, bag.Count(healing));
        }

        [Test]
        public void ARecipe_CannotBeBrewedUntilItsSchematicIsLearned()
        {
            storage.Add(flax, 10);
            storage.Add(amber, 10);

            Assert.AreEqual(0, brewer.MostBrewable(cleansingRecipe));
            book.Learn(cleansingRecipe);

            Assert.IsTrue(book.Knows(cleansingRecipe));
            Assert.IsTrue(brewer.Brew(cleansingRecipe, 1));
        }

        [Test]
        public void MostBrewable_IsSetByTheScarcestIngredientAndTheLimit()
        {
            book.Learn(cleansingRecipe);
            storage.Add(flax, 7);
            storage.Add(amber, 9);
            Assert.AreEqual(3, brewer.MostBrewable(cleansingRecipe));

            storage.Add(flax, 30);
            Assert.AreEqual(5, brewer.MostBrewable(cleansingRecipe));
        }

        [Test]
        public void ThePotionBagAndTheSchematics_AreSaved()
        {
            bag.Add(healing, 3);
            book.Learn(cleansingRecipe);

            var reloaded = new MetaProgression(ScriptableObject.CreateInstance<MetaConfig>(), store);

            Assert.AreEqual(3, reloaded.CreatePotionBag(config).Count(healing));
            Assert.IsTrue(reloaded.CreateRecipeBook(config).Knows(cleansingRecipe));
        }

        [Test]
        public void RemovingPotions_NeverGoesBelowZero()
        {
            bag.Add(healing, 1);

            Assert.IsTrue(bag.Remove(healing));
            Assert.IsFalse(bag.Remove(healing));
            Assert.AreEqual(0, bag.Count(healing));
            Assert.AreEqual(0, bag.Owned.Count);
        }
    }
}
