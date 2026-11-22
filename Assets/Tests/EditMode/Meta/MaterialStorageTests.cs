using System.Linq;
using Featherguard.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class MaterialStorageTests
    {
        MaterialDefinition flax;
        MaterialDefinition amber;
        MaterialDefinition[] all;
        MemorySaveStore store;

        static MaterialDefinition Material(string id)
        {
            var material = ScriptableObject.CreateInstance<MaterialDefinition>();
            material.name = id;
            return material;
        }

        [SetUp]
        public void SetUp()
        {
            flax = Material("Flax");
            amber = Material("Amber");
            all = new[] { flax, amber };
            store = new MemorySaveStore();
        }

        MaterialStorage Load() => new(all, store.Load(), store);

        [Test]
        public void Added_MaterialsAreCountedAndListedInCatalogOrder()
        {
            var storage = Load();
            storage.Add(amber, 2);
            storage.Add(flax, 3);
            storage.Add(amber, 1);

            Assert.AreEqual(3, storage.Count(flax));
            Assert.AreEqual(3, storage.Count(amber));
            CollectionAssert.AreEqual(new[] { flax, amber }, storage.Owned.Select(s => s.Material));
        }

        [Test]
        public void Storage_IsSavedAndRestored()
        {
            Load().Add(flax, 4);

            Assert.AreEqual(4, Load().Count(flax));
        }

        [Test]
        public void Clearing_EmptiesTheStorageAndIsSaved()
        {
            var storage = Load();
            storage.Add(flax, 4);
            storage.Clear();

            Assert.IsEmpty(storage.Owned);
            Assert.AreEqual(0, Load().Count(flax));
        }

        [Test]
        public void SavedMaterialsThatNoLongerExist_AreNotListed()
        {
            var data = store.Load();
            data.materials.Add(new ItemCount { id = "Removed", count = 5 });
            store.Save(data);

            Assert.IsEmpty(Load().Owned);
        }

        [Test]
        public void NothingOrLess_IsNotAdded()
        {
            var storage = Load();
            storage.Add(flax, 0);
            storage.Add(flax, -2);
            storage.Add(null, 3);

            Assert.IsEmpty(storage.Owned);
        }
    }
}
