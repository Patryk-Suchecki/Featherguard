using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Core;
using UnityEngine;

namespace Featherguard.Run
{
    public enum RunUnlock { None, Gems, Legendaries }

    public sealed class RunSession
    {
        readonly HashSet<RunUnlock> unlocks = new();
        readonly List<UpgradeDefinition> taken = new();

        public static RunSession Current { get; private set; }

        public RunDefinition Definition { get; }
        public EconomyConfig Economy => Definition.economy;
        public Party Party { get; }
        public Wallet Wallet { get; } = new();
        public UpgradeDeck Deck { get; }
        public IRandomSource Random { get; }
        public int EncounterIndex { get; private set; }
        public int BattlesWon { get; private set; }
        public int Kills { get; private set; }
        public int Luck { get; set; }
        public float StartRage { get; set; }
        public bool NextElite { get; set; }
        public bool CurrentElite { get; private set; }
        public IReadOnlyList<UpgradeDefinition> Taken => taken;

        public EncounterDefinition Encounter => Definition.encounters[EncounterIndex];
        public int EncounterCount => Definition.encounters.Length;
        public bool IsFinal => EncounterIndex >= EncounterCount - 1;
        public bool NextIsFinal => EncounterIndex + 1 >= EncounterCount - 1;
        public int CurrentGoldReward => Encounter.goldReward * (CurrentElite ? Economy.eliteGoldMultiplier : 1);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLoad() => Current = null;

        public RunSession(RunDefinition definition, IReadOnlyList<UnitDefinition> heroes, IRandomSource random)
        {
            Definition = definition;
            Random = random;
            Party = new Party(heroes);
            Deck = new UpgradeDeck(definition.upgrades, definition.curses, definition.rarity, random);
        }

        public static RunSession Start(RunDefinition definition, IReadOnlyList<UnitDefinition> heroes, IRandomSource random) =>
            Current = new RunSession(definition, heroes, random);

        public static void End() => Current = null;

        public bool Has(RunUnlock unlock) => unlocks.Contains(unlock);
        public void Unlock(RunUnlock unlock) => unlocks.Add(unlock);

        public BattleSetup CreateBattleSetup()
        {
            float elite = CurrentElite ? Economy.eliteStrength : 1f;
            var setup = new BattleSetup
            {
                Encounter = Encounter,
                Heroes = Party.Loadouts(),
                StartRage = StartRage,
                EnemyHealthScale = Encounter.enemyHealth * elite,
                EnemyDamageScale = Encounter.enemyDamage * elite,
                EncounterNumber = EncounterIndex + 1,
                EncounterCount = EncounterCount,
                Elite = CurrentElite,
            };
            StartRage = 0f;
            return setup;
        }

        public int RecordKill()
        {
            Kills++;
            return Wallet.Earn(Economy.goldPerKill);
        }

        public void RecordVictory(BattleResult result)
        {
            BattlesWon++;
            Wallet.Earn(CurrentGoldReward);
            Party.RecordBattle(result.Heroes, Economy.restHeal, Economy.reviveHealth);
        }

        public void Advance()
        {
            EncounterIndex++;
            CurrentElite = NextElite;
            NextElite = false;
        }

        public List<UpgradeOffer> DrawOffers(int count, Rarity minimum = Rarity.Common) =>
            Deck.Draw(count, minimum, Luck, Has(RunUnlock.Legendaries), Party.Heroes);

        public UpgradeOffer DrawCurse() => Deck.DrawCurse(Party.Heroes);

        public void Apply(UpgradeOffer offer)
        {
            if (!offer.IsValid) return;
            var upgrade = offer.Upgrade;
            taken.Add(upgrade);
            var targets = upgrade.scope switch
            {
                UpgradeScope.Hero => new[] { offer.Hero },
                UpgradeScope.Party => Party.Heroes,
                _ => System.Array.Empty<UnitDefinition>(),
            };
            foreach (var hero in targets)
                foreach (var modifier in upgrade.modifiers)
                    Party.AddModifier(hero, modifier);

            switch (upgrade.runBonus)
            {
                case RunBonus.Luck: Luck += Mathf.RoundToInt(upgrade.runAmount); break;
                case RunBonus.GoldGain: Wallet.GainMultiplier += upgrade.runAmount; break;
            }
        }
    }
}
