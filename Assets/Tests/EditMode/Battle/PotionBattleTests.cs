using System.Linq;
using Featherguard.Battle;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class PotionBattleTests
    {
        static UnitDefinition Fighter(Team team, int health = 100, int power = 10)
        {
            var attack = ScriptableObject.CreateInstance<AbilityDefinition>();
            attack.target = AbilityTarget.Enemy;
            attack.effects = new AbilityEffect[] { new DamageEffect { power = power } };
            var unit = ScriptableObject.CreateInstance<UnitDefinition>();
            unit.team = team;
            unit.maxHealth = health;
            unit.attack = attack;
            return unit;
        }

        static PotionDefinition Potion(PotionTarget target, params PotionEffect[] effects)
        {
            var potion = ScriptableObject.CreateInstance<PotionDefinition>();
            potion.target = target;
            potion.effects = effects;
            return potion;
        }

        static StatusDefinition Status(bool harmful, params StatusBehaviour[] behaviours)
        {
            var status = ScriptableObject.CreateInstance<StatusDefinition>();
            status.harmful = harmful;
            status.behaviours = behaviours;
            return status;
        }

        static BattleModel Model(int?[] heroHealth, UnitDefinition[] enemies, params (PotionDefinition Potion, int Count)[] potions)
        {
            var rules = ScriptableObject.CreateInstance<BattleRules>();
            rules.baseCritChance = 0f;
            rules.damageVariance = 0f;
            var encounter = ScriptableObject.CreateInstance<EncounterDefinition>();
            encounter.waves = new[] { new EncounterDefinition.Wave { enemies = enemies } };
            var setup = new BattleSetup
            {
                Encounter = encounter,
                Heroes = heroHealth.Select(h => new HeroLoadout { Definition = Fighter(Team.Heroes), Health = h }).ToList(),
                Potions = potions.Select(p => new BattlePotion(p.Potion, p.Count)).ToList(),
            };
            var model = new BattleModel(setup, rules, new FakeRandom(0.5f));
            model.Start();
            return model;
        }

        [Test]
        public void UsingAPotion_UsesNoHerosActionAndKeepsThePlayersTurn()
        {
            var flask = Potion(PotionTarget.Enemy, new DamagePotion { power = 30 });
            var model = Model(new int?[] { null, null }, new[] { Fighter(Team.Enemies) }, (flask, 2));
            int spent = 0;
            model.PotionSpent += _ => spent++;
            int turn = model.Turn;

            var events = model.UsePotion(flask, model.Enemies[0]);

            Assert.AreEqual(70, model.Enemies[0].Health);
            Assert.IsTrue(model.AwaitingInput);
            Assert.AreEqual(turn, model.Turn);
            Assert.IsTrue(model.Heroes.All(h => h.CanAct));
            Assert.AreEqual(1, model.Potions.Count(flask));
            Assert.AreEqual(1, spent);
            Assert.IsTrue(events.OfType<PotionUsed>().Any());
        }

        [Test]
        public void APotion_NeedsOneLeftAndAValidTarget()
        {
            var draught = Potion(PotionTarget.Hero, new HealPercentPotion { fraction = 0.35f });
            var model = Model(new int?[] { 50 }, new[] { Fighter(Team.Enemies) }, (draught, 1));

            Assert.IsFalse(model.CanUsePotion(draught, model.Enemies[0]));
            model.UsePotion(draught, model.Heroes[0]);

            Assert.AreEqual(85, model.Heroes[0].Health);
            Assert.IsFalse(model.CanUsePotion(draught, model.Heroes[0]));
            Assert.AreEqual(0, model.UsePotion(draught, model.Heroes[0]).Count);
        }

        [Test]
        public void ARevive_BringsAFallenHeroBackWithAShareOfMaxHealth()
        {
            var firebird = Potion(PotionTarget.FallenHero, new RevivePotion { fraction = 0.4f });
            var model = Model(new int?[] { null, 0 }, new[] { Fighter(Team.Enemies) }, (firebird, 1));
            var fallen = model.Heroes[1];

            Assert.IsFalse(model.CanUsePotion(firebird, model.Heroes[0]));
            var events = model.UsePotion(firebird, fallen);

            Assert.AreEqual(40, fallen.Health);
            Assert.IsTrue(events.OfType<Revived>().Any(e => e.Unit == fallen));
            Assert.IsTrue(fallen.CanAct);
        }

        [Test]
        public void LivingWater_HealsTheLivingAndRevivesTheFallen()
        {
            var water = Potion(PotionTarget.AllHeroes, new HealPercentPotion { fraction = 0.5f }, new RevivePotion { fraction = 0.5f });
            var model = Model(new int?[] { 20, 0 }, new[] { Fighter(Team.Enemies) }, (water, 1));

            model.UsePotion(water, model.Heroes[0]);

            Assert.AreEqual(70, model.Heroes[0].Health);
            Assert.AreEqual(50, model.Heroes[1].Health);
        }

        [Test]
        public void ARagePotion_AddsItsRageOnceForTheWholeParty()
        {
            var fury = Potion(PotionTarget.AllHeroes, new RagePotion { amount = 40f });
            var model = Model(new int?[] { null, null, null }, new[] { Fighter(Team.Enemies) }, (fury, 1));

            model.UsePotion(fury, model.Heroes[1]);

            Assert.AreEqual(40f, model.Rage.Value, 0.001f);
        }

        [Test]
        public void AThrownPotion_HitsEveryEnemyWhenItTargetsAll()
        {
            var mark = Status(true);
            var oil = Potion(PotionTarget.AllEnemies, new StatusPotion { application = new StatusApplication { status = mark, turns = 2 } });
            var model = Model(new int?[] { null }, new[] { Fighter(Team.Enemies), Fighter(Team.Enemies) }, (oil, 1));

            model.UsePotion(oil, model.Enemies[1]);

            Assert.IsTrue(model.Enemies.All(e => e.Statuses.Has(mark)));
        }

        [Test]
        public void APotionThatKillsTheLastEnemy_WinsTheBattle()
        {
            var flask = Potion(PotionTarget.Enemy, new DamagePotion { power = 30 });
            var model = Model(new int?[] { null }, new[] { Fighter(Team.Enemies, 20) }, (flask, 1));

            model.UsePotion(flask, model.Enemies[0]);

            Assert.IsTrue(model.IsFinished);
            Assert.IsTrue(model.Result.Victory);
        }

        [Test]
        public void Warded_BlocksHarmfulStatusesButNotHelpfulOnes()
        {
            var warded = Status(false, new BlockStatuses { harmful = true });
            var weaken = Status(true);
            var empower = Status(false);
            var brew = Potion(PotionTarget.Hero, new CleansePotion(), new StatusPotion { application = new StatusApplication { status = warded, turns = 1 } });
            var model = Model(new int?[] { null }, new[] { Fighter(Team.Enemies) }, (brew, 1));
            var hero = model.Heroes[0];
            var enemy = model.Enemies[0];
            model.State.TryApplyStatus(enemy, hero, new StatusApplication { status = weaken, turns = 2 });
            Assert.IsTrue(hero.Statuses.Has(weaken));

            model.UsePotion(brew, hero);
            model.State.TryApplyStatus(enemy, hero, new StatusApplication { status = weaken, turns = 2 });
            model.State.TryApplyStatus(hero, hero, new StatusApplication { status = empower, turns = 2 });

            Assert.IsFalse(hero.Statuses.Has(weaken));
            Assert.IsTrue(hero.Statuses.Has(empower));
            Assert.IsTrue(hero.Statuses.Has(warded));
        }
    }
}
