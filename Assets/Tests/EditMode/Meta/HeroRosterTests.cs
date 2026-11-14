using System.Linq;
using Featherguard.Battle;
using Featherguard.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class HeroRosterTests
    {
        static UnitDefinition Hero(string name)
        {
            var hero = ScriptableObject.CreateInstance<UnitDefinition>();
            hero.name = name;
            hero.displayName = name;
            return hero;
        }

        static (MetaProgression meta, UnitDefinition[] heroes, MemorySaveStore store) Create(int heroCount)
        {
            var store = new MemorySaveStore();
            var meta = new MetaProgression(ScriptableObject.CreateInstance<MetaConfig>(), store);
            var heroes = Enumerable.Range(1, heroCount).Select(i => Hero($"Hero{i}")).ToArray();
            return (meta, heroes, store);
        }

        [Test]
        public void NewRoster_StartsWithTheFirstThreeHeroesInTheParty()
        {
            var (meta, heroes, _) = Create(4);

            var roster = meta.CreateRoster(heroes, 6);

            CollectionAssert.AreEqual(heroes.Take(3), roster.Party);
            Assert.IsTrue(roster.IsPartyComplete);
        }

        [Test]
        public void Toggle_RemovesAPartyMemberAndAddsAnotherHero()
        {
            var (meta, heroes, _) = Create(4);
            var roster = meta.CreateRoster(heroes, 6);

            Assert.AreEqual(PartyChange.Removed, roster.Toggle(heroes[1]));
            Assert.IsFalse(roster.IsPartyComplete);
            Assert.AreEqual(PartyChange.Added, roster.Toggle(heroes[3]));

            CollectionAssert.AreEqual(new[] { heroes[0], heroes[2], heroes[3] }, roster.Party);
        }

        [Test]
        public void Toggle_RefusesAFourthHero()
        {
            var (meta, heroes, _) = Create(4);
            var roster = meta.CreateRoster(heroes, 6);

            Assert.AreEqual(PartyChange.PartyFull, roster.Toggle(heroes[3]));
            Assert.IsFalse(roster.InParty(heroes[3]));
        }

        [Test]
        public void Toggle_RefusesAHeroWhoHasNotJoined()
        {
            var (meta, heroes, _) = Create(3);
            var roster = meta.CreateRoster(heroes, 6);

            Assert.AreEqual(PartyChange.NotJoined, roster.Toggle(Hero("Stranger")));
        }

        [Test]
        public void Party_IsSavedAndRestored()
        {
            var (meta, heroes, store) = Create(4);
            var roster = meta.CreateRoster(heroes, 6);
            roster.Toggle(heroes[0]);
            roster.Toggle(heroes[3]);

            var reloaded = new MetaProgression(meta.Config, store).CreateRoster(heroes, 6);

            CollectionAssert.AreEqual(new[] { heroes[1], heroes[2], heroes[3] }, reloaded.Party);
        }

        [Test]
        public void SavedHeroesMissingFromTheRoster_AreDropped()
        {
            var (meta, heroes, store) = Create(3);
            meta.Data.party = new() { "Gone", "Hero2" };
            store.Save(meta.Data);

            var roster = new MetaProgression(meta.Config, store).CreateRoster(heroes, 6);

            CollectionAssert.AreEqual(new[] { heroes[1] }, roster.Party);
        }

        [Test]
        public void Seats_HoldHeroesInRosterOrderAndStayEmptyAfterThem()
        {
            var (meta, heroes, _) = Create(3);
            var roster = meta.CreateRoster(heroes, 6);

            Assert.AreEqual(heroes[2], roster.AtSeat(2));
            Assert.IsNull(roster.AtSeat(3));
            Assert.AreEqual(6, roster.Seats);
        }
    }
}
