# Featherguard – Game Design

Source of truth for what is in the game **now**. The numbers come from the data assets in `Assets/Data` (seeded by `Assets/Editor/Setup/*Catalog.cs`). Whenever a PR changes content or rules, update this file in the same PR.

## Pillars

- Short turn-based battles: a party of three birds against waves of lizards.
- A run of 9 battles ending with a boss. Between battles the player gambles for power.
- Gambling uses in-game currency only: gold during a run, feathers between runs. No real money.
- Built for mobile: landscape, touch, one-handed tapping.

## Battle

- **Turns:** the player's turn, then the enemy turn. On their turn the player can act with every hero once, in any order:
  - tap a hero, then tap an enemy to attack or an ally to use the support ability;
  - when the shared **Rage** meter is full, use that hero's special.
- **Rage (`BattleRules`):**
  - max 100;
  - heroes gain 0.45 rage per point of damage dealt, multiplied by the hero's RageGain;
  - enemies feed 0.25 rage per point of damage they deal;
  - a special consumes the whole meter.
- **Damage:** power × Damage stat × random 90–110%. Crit chance 15%, crit damage ×1.5. The target's DamageTaken stat multiplies the result. Lifesteal heals the attacker by that share of damage dealt.
- **Statuses:**
  - applied by the owner's side during the owner's turn, they last through the next own turn;
  - applied by the opponent, they tick down at the end of the owner's turn;
  - reapplying keeps the stronger duration and potency.
- **Waves:** an encounter can have several waves. Clearing a wave on the player's turn starts the next wave and a new player turn.
- **Enemy AI:**
  - uses its special every N turns;
  - heals wounded allies below 60% HP (75% chance) or buffs allies (45% chance);
  - otherwise attacks, preferring wounded heroes and heroes it can finish;
  - must attack a taunting hero.
- **Rewards in battle:** 3 gold per kill.

### Heroes

| Hero | HP | Attack | Support | Special (full rage) |
|---|---|---|---|---|
| Robin (Knight) | 150 | Slash – melee, 30 dmg | Cover – Shield on an ally + Taunt on self | Shield Bash – melee, 85 dmg + Stun, heavy |
| Jay (Archer) | 105 | Double Shot – 2 arrows × 14 dmg, 30% Poison (5/turn, 2 turns) | Battle Cry – Empower on an ally | Arrow Rain – all enemies, 2 × 24 dmg + Poison (6/turn, 2 turns) |
| Owl | 115 | Spark – magic bolt, 20 dmg, 35% Weaken | Mend – heal 40 | Forest Renewal – heal all allies 60 |

### Enemies

| Enemy | HP | Abilities |
|---|---|---|
| Thug (Grunt) | 80 | Club – melee 15 |
| Shaman | 70 | Hex – bolt 12, 50% Weaken; Dark Mending – heal 25 |
| Brute | 170 | Crush – melee 26, heavy, 25% Stun |
| Spitter | 65 | Venom – bolt 9, 70% Poison (6/turn, 2 turns) |
| Shieldbearer (Guard) | 110 | Spear Thrust – melee 14; Shield Wall – Shield on an ally + Taunt on self |

Each encounter also scales enemy HP and damage, as listed in the Run section.

### Boss: Chieftain

- 460 HP.
- Chieftain's Wrath: melee 34, heavy, 35% Stun.
- On its first turn and every 3rd turn after, Earthquake: hits all heroes for 22, 50% Weaken.
- At 50% HP it enrages: +40% damage and summons two Thugs into free slots.

### Statuses

| Status | Effect |
|---|---|
| Shield | −50% damage taken |
| Empower | +50% damage, consumed by the next damaging ability |
| Poison | Takes the potency as damage at the start of the owner's turn |
| Stun | Skips the turn |
| Weaken | −30% damage |
| Taunt | Opponents' single-target abilities must target this unit |

The default duration is 1 turn unless stated otherwise.

## Run

Nine encounters (`Run.asset`):

| # | Name | Waves | Enemy HP × / dmg × | Gold |
|---|---|---|---|---|
| 1 | Forest Edge | Thug, Thug | 1.0 / 1.0 | 10 |
| 2 | Woodland Path | Thug, Shaman, Thug | 1.0 / 1.0 | 12 |
| 3 | Old Oak | Thug, Thug → Shaman, Thug | 1.0 / 1.0 | 14 |
| 4 | Windfall | Thug, Brute, Spitter | 1.05 / 1.0 | 16 |
| 5 | Misty Ravine | Spitter, Shieldbearer, Shaman → Thug, Brute | 1.1 / 1.05 | 18 |
| 6 | Marsh | Spitter, Shaman, Spitter → Thug, Shieldbearer, Thug | 1.15 / 1.1 | 20 |
| 7 | Stone Ford | Brute, Shieldbearer, Brute → Shaman, Spitter | 1.2 / 1.1 | 22 |
| 8 | Lizard Camp | Shieldbearer, Shaman, Brute → Spitter, Brute, Thug | 1.3 / 1.15 | 25 |
| 9 | Chieftain (boss) | Thug, Shieldbearer, Thug → Spitter, Chieftain, Shaman | 1.3 / 1.15 | 40 |

- **Hero HP between battles:** heroes keep their HP from battle to battle. After a win, survivors heal 20% of max HP and fallen heroes return with 30%.
- **Losing:** a lost battle ends the run.

### Reward sequence after a win

1. **Encounter gold:** ×2 after an elite battle.
2. **Slot machine:**
   - three reels; each reel copies the previous one with a 30% chance, +4% per luck, max 60%;
   - a pair or triple of a symbol grants its reward, otherwise "Nothing... +5 gold";
   - symbols:

   | Symbol | Weight | Pair | Triple |
   |---|---|---|---|
   | Gold | 30 | +20 gold | +60 gold |
   | Heart | 20 | heal 25% | full heal |
   | Card | 18 (+1/luck) | extra card | extra Epic+ card |
   | Rage | 14 | +40 rage at next battle start | full rage |
   | Bad luck (skull) | 13 (−2/luck, min 3) | −10 gold | −15% HP, −20 gold |
   | Gem (needs the Gems perk) | 6 (+1/luck) | +50 gold | MEGA JACKPOT +150 gold |
3. **Double or nothing:** risk the encounter gold at a 50% win chance, +3% per luck, max 65%.
4. **Card pick:**
   - three upgrade cards; pick one;
   - reroll costs 10 gold, +5 for each further reroll in the same reward phase;
   - after an elite battle the cards are Rare or better;
   - extra cards won on the slot machine follow.
5. **Path choice:** two of Path (normal battle), Elite, Shop, Chest, Campfire. Before the boss the choice is always Shop or Campfire.
   - **Elite:** the next battle has enemies ×1.35 stronger, gives double gold and a Rare+ card.
   - **Shop:** 3 cards, each bought with gold (Common 25, Rare 45, Epic 80, Legendary 130) or blood (−10/18/28/40% current HP for the whole party); healing 30% for 20 gold.
   - **Chest:** one Epic+ card plus a random curse.
   - **Campfire:** the party heals 35%.

### Upgrade cards

Each card targets one hero (Hero), the whole party (Party) or the run (Run).

**Rarity roll:** base weights Common 60, Rare 28, Epic 10, Legendary 2. Each luck point moves them by −6 / +3 / +2 / +1, and Common never drops below 10. Legendary cards only appear with the Legends perk.

| Rarity | Cards |
|---|---|
| Common | Sharp Feathers +15% dmg; Thick Plumage +15% max HP; Keen Eye +6% crit; Warm Nest +20% healing; Down Jacket −10% damage taken; Lucky Penny +15% gold (run); Horseshoe +1 luck (run) |
| Rare | Fury Blade +25% dmg; Cruel Talons +30% crit damage; Vampire Beak 15% lifesteal; Hot Blood +30% rage gain; Steel Feathers +25% max HP; Four-Leaf Clover +2 luck (run) |
| Epic | Frenzy +20% dmg, +12% crit; Flock +12% dmg (party); Bastion −20% damage taken (party); Treasurer +40% gold (run) |
| Legendary | Phoenix +40% max HP, +30% healing; Jackpot +4 luck (run); Forest Wrath +25% dmg, +10% crit (party) |
| Curses | Frailty −12% max HP (party); Dull Beaks −10% dmg (party); Exposed +15% damage taken (party); Leaky Purse −25% gold (run) |

**Luck** improves slot matches, symbol weights, card rarity and gamble odds.

## Meta progression (Camp)

- **Feathers per run:** 1 per battle won, plus 1 per 50 gold earned, plus 10 for completing the run.
- **Perk costs:** a perk costs its base cost × (current level + 1).
- **Save file:** `featherguard_save.json` in the persistent data path.

| Perk | Effect per level | Max level | Base cost |
|---|---|---|---|
| Tough Feathers | +6% party max HP | 5 | 3 |
| Sharp Beaks | +5% party damage | 5 | 3 |
| Horseshoe | +1 starting luck | 3 | 5 |
| Purse | +15 starting gold | 4 | 2 |
| Gems | Adds the Gem slot symbol | 1 | 8 |
| Legends | Adds Legendary cards | 1 | 10 |

## Presentation

- **Visuals:** low-poly characters, generated procedurally in Blender (`Art/Blender`), with animations Idle, Attack, Cast, Hit, Death and Victory. Portraits are rendered from the same models.
- **Icons and sounds:** procedural, generated by `IconFactory` and `SfxSynth`.
- **Timing:** feedback timings live in `BattlePresentation.asset`. Hitstop, camera shake and popups are configured in `GameConfig.feedback`.
- **UI:** landscape, 1920×1080 reference resolution, safe area, generous tap tolerance, pause menu with sound toggle and an option to abandon the run.

## Agreed decisions

Record decisions here with the date and who agreed, so both people's AIs follow them.

- 2026-11: Gambling is a reward layer between battles; in-game currency only, never real money.
- 2026-11: Battle logic stays a pure model emitting events; presentation only replays them.
- 2026-11: The whole project is in English, including in-game text, data and docs.
