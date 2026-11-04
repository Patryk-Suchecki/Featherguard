using System.Linq;
using Featherguard.Battle;
using Featherguard.Run;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class RewardTests
    {
        static UpgradeDefinition Upgrade(string name, bool curse = false)
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.name = name;
            upgrade.displayName = name;
            upgrade.curse = curse;
            upgrade.scope = UpgradeScope.Party;
            upgrade.modifiers = new[] { new StatModifier(StatType.Damage, multiply: curse ? -0.1f : 0.1f) };
            return upgrade;
        }

        static RunSession Session(float random, UpgradeDefinition[] upgrades, UpgradeDefinition[] curses = null)
        {
            var run = ScriptableObject.CreateInstance<RunDefinition>();
            run.economy = ScriptableObject.CreateInstance<EconomyConfig>();
            run.upgrades = upgrades;
            run.curses = curses ?? new UpgradeDefinition[0];
            run.rarity = new RarityTable { tiers = new[] { new RarityTier { rarity = Rarity.Common, weight = 1f }, new RarityTier { rarity = Rarity.Epic, weight = 1f } } };
            var encounter = ScriptableObject.CreateInstance<EncounterDefinition>();
            encounter.goldReward = 10;
            run.encounters = new[] { encounter };
            var hero = ScriptableObject.CreateInstance<UnitDefinition>();
            hero.maxHealth = 100;
            return new RunSession(run, new[] { hero }, new FakeRandom(random));
        }

        [Test]
        public void Shop_GoldPurchaseSpendsAppliesAndSellsOnce()
        {
            var session = Session(0.5f, new[] { Upgrade("A"), Upgrade("B"), Upgrade("C") });
            session.Wallet.Deposit(100);
            var shop = new Shop(session);
            int price = shop.GoldPrice(0);

            Assert.IsTrue(shop.BuyWithGold(0));
            Assert.IsTrue(shop.IsSold(0));
            Assert.IsFalse(shop.BuyWithGold(0));
            Assert.IsFalse(shop.BuyWithBlood(0));
            Assert.AreEqual(100 - price, session.Wallet.Gold);
            Assert.AreEqual(1, session.Taken.Count);
        }

        [Test]
        public void Shop_BloodPurchaseCostsHealthAndHealCostsGold()
        {
            var session = Session(0.5f, new[] { Upgrade("A") });
            var shop = new Shop(session);
            var hero = session.Party.Heroes[0];

            Assert.IsFalse(shop.BuyHeal());
            Assert.IsTrue(shop.BuyWithBlood(0));
            Assert.AreEqual(Mathf.RoundToInt(100 * (1f - shop.BloodPrice(0))), session.Party.HealthOf(hero));

            session.Wallet.Deposit(shop.HealCost);
            Assert.IsTrue(shop.BuyHeal());
            Assert.AreEqual(100, session.Party.HealthOf(hero));
            Assert.AreEqual(0, session.Wallet.Gold);
        }

        [Test]
        public void Gamble_WinDoublesStakeAndLossTakesIt()
        {
            var winner = Session(0.1f, new UpgradeDefinition[0]);
            winner.Wallet.Deposit(10);
            var loser = Session(0.9f, new UpgradeDefinition[0]);
            loser.Wallet.Deposit(10);

            Assert.IsTrue(new Gamble(winner).Resolve());
            Assert.IsFalse(new Gamble(loser).Resolve());
            Assert.AreEqual(20, winner.Wallet.Gold);
            Assert.AreEqual(0, loser.Wallet.Gold);
            Assert.IsFalse(new Gamble(loser).Available);
        }

        [Test]
        public void CardDraft_RerollCostsGoldAndGetsPricier()
        {
            var session = Session(0.5f, new[] { Upgrade("A"), Upgrade("B"), Upgrade("C"), Upgrade("D") });
            session.Wallet.Deposit(30);
            var round = new RewardRound(session);
            var draft = new CardDraft(round, Rarity.Common);
            int first = draft.RerollCost;

            Assert.IsTrue(draft.Reroll());
            Assert.AreEqual(30 - first, session.Wallet.Gold);
            Assert.Greater(draft.RerollCost, first);
            Assert.IsTrue(draft.Pick(0));
            Assert.IsFalse(draft.Pick(99));
            Assert.AreEqual(1, session.Taken.Count);
        }

        [Test]
        public void CardDraft_IsEmptyWhenPoolHasNothing()
        {
            var draft = new CardDraft(new RewardRound(Session(0.5f, new UpgradeDefinition[0])), Rarity.Common);

            Assert.IsTrue(draft.IsEmpty);
        }

        [Test]
        public void Chest_GrantsRewardAndCurse()
        {
            var epic = Upgrade("Epic");
            epic.rarity = Rarity.Epic;
            var session = Session(0.5f, new[] { epic }, new[] { Upgrade("Curse", true) });

            var gained = PathRewards.OpenChest(session);

            Assert.AreEqual(2, gained.Count);
            Assert.IsTrue(gained[1].Upgrade.curse);
            Assert.AreEqual(2, session.Taken.Count);
        }

        [Test]
        public void SlotRules_PlayGrantsTheEvaluatedReward()
        {
            var session = Session(0.5f, new UpgradeDefinition[0]);
            var config = ScriptableObject.CreateInstance<SlotConfig>();
            config.matchChance = 1f;
            config.maxMatchChance = 1f;
            config.symbols = new[] { new SlotSymbol { id = "Coin", weight = 1f, pair = new GoldReward { amount = 20 }, triple = new GoldReward { amount = 60 } } };

            var outcome = SlotRules.Play(config, session, new RewardRound(session));

            Assert.IsTrue(outcome.IsJackpot);
            Assert.AreEqual(60, session.Wallet.Gold);
        }

        [Test]
        public void Path_BeforeBossUsesFixedNodes()
        {
            var session = Session(0.5f, new UpgradeDefinition[0]);
            session.Definition.beforeBossNodes = new[] { new PathNode { type = PathNodeType.Campfire } };
            session.Definition.pathNodes = new[] { new PathNode { type = PathNodeType.Shop }, new PathNode { type = PathNodeType.Chest } };

            Assert.AreEqual(PathNodeType.Campfire, PathRewards.Choices(session).Single().type);
        }
    }
}
