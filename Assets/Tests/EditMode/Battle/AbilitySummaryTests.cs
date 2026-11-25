using Featherguard.Battle;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class AbilitySummaryTests
    {
        static AbilityDefinition Ability(AbilityTarget target, int hits, params AbilityEffect[] effects)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.target = target;
            ability.hits = hits;
            ability.effects = effects;
            return ability;
        }

        static StatusDefinition Status(string name, params StatusBehaviour[] behaviours)
        {
            var status = ScriptableObject.CreateInstance<StatusDefinition>();
            status.displayName = name;
            status.behaviours = behaviours;
            return status;
        }

        [Test]
        public void ASummary_NamesTheTargetTheNumbersAndTheStatuses()
        {
            var bleed = Status("Bleed", new DamageOverTime());
            var ability = Ability(AbilityTarget.Enemy, 1, new DamageEffect { power = 30 },
                new ApplyStatusEffect { application = new StatusApplication { status = bleed, turns = 3, potency = 5, condition = StatusCondition.PerfectTiming } });

            Assert.AreEqual("One enemy: 30 damage; applies Bleed for 3 turns (5 damage a turn) on a perfect timing.", ability.Summary);
        }

        [Test]
        public void ASummary_CountsHitsChancesAndStatusesOnSelf()
        {
            var stun = Status("Stun", new SkipTurn());
            var empower = Status("Empower");
            var ability = Ability(AbilityTarget.AllEnemies, 3, new DamageEffect { power = 10 },
                new ApplyStatusEffect { application = new StatusApplication { status = stun, turns = 1, chance = 0.3f } },
                new ApplyStatusEffect { application = new StatusApplication { status = empower, turns = 2, onSelf = true } });

            Assert.AreEqual("Every enemy: 10 damage; applies Stun for 1 turn, 30% chance; gives itself Empower for 2 turns, 3 hits.", ability.Summary);
        }

        [Test]
        public void ASummary_DescribesHealingChainsCleansesAndPerfectTimingEffects()
        {
            var shield = Status("Shield", new AbsorbDamage());
            var heal = Ability(AbilityTarget.AllAllies, 1, new HealEffect { power = 25, overflowShield = shield, overflowShieldTurns = 2 }, new CleanseEffect());
            var chain = Ability(AbilityTarget.Enemy, 1, new ChainDamageEffect { power = 20, falloff = 0.6f }, new PerfectTimingEffect { effect = new DamageEffect { power = 5 } });

            Assert.AreEqual("Every ally: heals 25; healing past full health becomes a Shield for 2 turns; removes every harmful status.", heal.Summary);
            Assert.AreEqual("One enemy: 20 damage, then jumps to every other enemy, keeping 60% of the hit each time; on a perfect timing, 5 damage.", chain.Summary);
        }
    }
}
