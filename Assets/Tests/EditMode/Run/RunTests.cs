using Featherguard.Battle;
using Featherguard.Run;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class RunTests
    {
        static UnitDefinition Hero(int health)
        {
            var hero = ScriptableObject.CreateInstance<UnitDefinition>();
            hero.maxHealth = health;
            hero.displayName = "Hero";
            return hero;
        }

        [Test]
        public void Wallet_EarnAppliesGainButDepositDoesNot()
        {
            var wallet = new Wallet { GainMultiplier = 1.5f };

            Assert.AreEqual(15, wallet.Earn(10));
            wallet.Deposit(10);

            Assert.AreEqual(25, wallet.Gold);
            Assert.AreEqual(15, wallet.Earned);
        }

        [Test]
        public void Wallet_SpendRequiresFundsAndLoseClampsAtZero()
        {
            var wallet = new Wallet();
            wallet.Deposit(20);

            Assert.IsFalse(wallet.TrySpend(30));
            Assert.IsTrue(wallet.TrySpend(15));
            wallet.Lose(50);

            Assert.AreEqual(0, wallet.Gold);
        }

        [Test]
        public void Party_MaxHealthUpgradeAlsoRaisesCurrentHealth()
        {
            var hero = Hero(100);
            var party = new Party(new[] { hero });
            party.HurtAll(0.5f);

            party.AddModifier(hero, new StatModifier(StatType.MaxHealth, multiply: 0.2f));

            Assert.AreEqual(120, party.MaxHealthOf(hero));
            Assert.AreEqual(70, party.HealthOf(hero));
        }

        [Test]
        public void Party_HealAndHurtStayWithinBounds()
        {
            var hero = Hero(100);
            var party = new Party(new[] { hero });

            party.HurtAll(1f);
            Assert.AreEqual(1, party.HealthOf(hero));
            party.HealAll(5f);
            Assert.AreEqual(100, party.HealthOf(hero));
        }

        [Test]
        public void RarityTable_RespectsMinimumAndLegendaryUnlock()
        {
            var table = new RarityTable
            {
                tiers = new[]
                {
                    new RarityTier { rarity = Rarity.Common, weight = 100f },
                    new RarityTier { rarity = Rarity.Epic, weight = 1f },
                    new RarityTier { rarity = Rarity.Legendary, weight = 1000f, requiresLegendaryUnlock = true },
                },
            };

            Assert.AreEqual(Rarity.Epic, table.Roll(Rarity.Rare, 0, false, new FakeRandom(0.99f)));
            Assert.AreEqual(Rarity.Common, table.Roll(Rarity.Common, 0, false, new FakeRandom(0.5f)));
            Assert.AreEqual(Rarity.Legendary, table.Roll(Rarity.Common, 0, true, new FakeRandom(0.99f)));
        }

        [Test]
        public void SlotRules_EvaluatesTriplesPairsAndMisses()
        {
            var coin = new SlotSymbol { id = "Coin", pair = new GoldReward { amount = 20 }, triple = new GoldReward { amount = 60 } };
            var skull = new SlotSymbol { id = "Skull", pair = new PenaltyReward(), triple = new PenaltyReward() };
            var heart = new SlotSymbol { id = "Heart" };
            var config = ScriptableObject.CreateInstance<SlotConfig>();

            var triple = SlotRules.Evaluate(config, new[] { coin, coin, coin });
            var pair = SlotRules.Evaluate(config, new[] { skull, coin, skull });
            var miss = SlotRules.Evaluate(config, new[] { coin, skull, heart });

            Assert.IsTrue(triple.IsJackpot);
            Assert.AreSame(coin.triple, triple.Reward);
            Assert.AreEqual(2, pair.Matches);
            Assert.AreSame(skull.pair, pair.Reward);
            Assert.AreSame(config.noMatch, miss.Reward);
        }

        [Test]
        public void Economy_GambleChanceIsCapped()
        {
            var economy = ScriptableObject.CreateInstance<EconomyConfig>();

            Assert.AreEqual(0.5f, economy.GambleChance(0), 0.001f);
            Assert.AreEqual(economy.gambleMaxChance, economy.GambleChance(100), 0.001f);
            Assert.AreEqual(20, economy.RerollCost(2));
        }
    }
}
