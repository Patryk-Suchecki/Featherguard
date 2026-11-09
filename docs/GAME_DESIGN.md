# Featherguard – Game Design

Source of truth for what is in the game **now**. The agreed target design is [`GDD.md`](GDD.md). The numbers come from the data assets in `Assets/Data` (seeded by `Assets/Editor/Setup/*Catalog.cs`). Whenever a PR changes content or rules, update this file in the same PR.

## Pillars

- Short turn-based battles: a party of three birds against waves of lizards.
- A run of 9 battles ending with a boss. Between battles the player gambles for power.
- Gambling uses in-game currency only: gold during a run, feathers between runs. No real money.
- Built for mobile: landscape, touch, one-handed tapping.

## Battle

- **Turns:** every battle starts with the player's turn, then the enemy turn, and so on.
  - On the player's turn every living, non-stunned hero acts once, in any order: tap a hero, then tap an enemy to attack or an ally to use the support ability.
  - When the **Rage** meter is full, the selected hero can use its special instead.
  - The turn ends when every hero has acted. Enemies then act one by one in the order they entered the battle (summons last); stunned enemies skip.
- **Rage (`BattleRules`):**
  - one meter shared by the party, max 100;
  - it starts every battle at 0, unless a Rage slot reward set a starting value;
  - it fills by 0.45 per point of damage a hero deals (× that hero's RageGain) and by 0.25 per point of damage the enemies deal to the party;
  - a hero special consumes the whole meter. Enemy specials do not use the meter.
- **Damage:** power × attacker's Damage stat × random 90–110%, then crit (15% base chance, ×1.5 base damage), then × the target's DamageTaken stat. Lifesteal heals the attacker by that share of the damage actually dealt.
- **Healing:** power × the healer's Healing stat. No randomness and no crits; it cannot exceed max HP or revive a fallen unit.
- **Statuses:**
  - damage over time and stuns resolve at the start of the owner's turn, so poison can kill a unit before it acts;
  - a status applied by the owner's own side lasts through the owner's next turn; one applied by the opponent ticks down at the end of the owner's turn;
  - reapplying keeps the stronger duration and potency;
  - Poison damage is also multiplied by DamageTaken, so Shield halves it.
- **Taunt** only redirects single-target abilities; abilities that hit all enemies are unaffected.
- **Waves:** an encounter can have several waves. When the last enemy of a wave falls, the next wave enters and a new player turn starts.
- **Enemy AI:**
  - uses its special on its first turn and then every N turns (Chieftain: N = 3);
  - heals the most wounded ally below 60% HP (75% chance) or buffs an ally that lacks the buff (45% chance);
  - otherwise attacks, preferring wounded heroes and heroes it can finish;
  - must attack a taunting hero.
- **Gold in battle:** 3 gold per kill, multiplied by the run's gold bonus.

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

**Bies (test only, not in the run):** the first hand-made 3D character, the Rye Fields elite of the target design. 260 HP. Rending Claws – melee 24, heavy. On its first turn and every 3rd turn after, Howl: Weaken on all heroes. The abilities are placeholders until the target combat rules exist. It only appears in the `BiesTest` encounter; to fight it in the editor, set `GameConfig.testEncounter` to `BiesTest` (editor only, every battle then uses that encounter) and leave the field empty in commits.

### Boss: Chieftain

- 460 HP.
- Chieftain's Wrath: melee 34, heavy, 35% Stun.
- On its first turn and every 3rd turn after, Earthquake: hits all heroes for 22, 50% Weaken.
- At 50% HP it enrages once: +40% damage and summons two Thugs into free slots (a slot is free when no living enemy stands in it). If all slots are taken, fewer or no Thugs appear.

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

- **Start:** every run starts at the camp screen, where feathers are spent on perks, then "Set out!".
- **Hero HP between battles:** heroes keep their HP from battle to battle. After a win, survivors heal 20% of max HP and fallen heroes return with 30%.
- **Gold bonus:** kill gold, encounter gold and gold from the slot machine are multiplied by the run's gold bonus (Lucky Penny, Treasurer, Leaky Purse). Losses, purchases and double-or-nothing are not.
- **Losing:** a lost battle ends the run. Abandoning the run from the pause menu counts as a loss.

### Reward sequence after a win

1. **Encounter gold:** the value from the table, ×2 after an elite battle, × the gold bonus.
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
3. **Double or nothing:** the stake is the encounter gold (before the gold bonus). It is only offered when the wallet holds at least the stake. Win chance 50%, +3% per luck, max 65%: a win adds the stake again, a loss removes it.
4. **Card pick:**
   - three upgrade cards; pick one;
   - reroll costs 10 gold, +5 for each further reroll in the same reward phase;
   - after an elite battle this pick is Rare or better;
   - extra cards won on the slot machine follow, each with its own minimum rarity (Common for a pair, Epic for a triple).
5. **Path choice:** two of Path (normal battle), Elite, Shop, Chest, Campfire. Before the boss the choice is always Shop or Campfire.
   - **Elite:** the next battle has enemies ×1.35 stronger, gives double gold and a Rare+ card.
   - **Shop:** 3 cards, each bought once with gold (Common 25, Rare 45, Epic 80, Legendary 130) or with blood (−10/18/28/40% of current HP for the whole party; blood never drops a hero below 1 HP); healing 30% of max HP for 20 gold, repeatable.
   - **Chest:** one Epic+ card (Legendary only with the Legends perk) plus a random curse, both applied immediately.
   - **Campfire:** the party heals 35%.

### Upgrade cards

Each card targets one hero (Hero), the whole party (Party) or the run (Run). A Hero card names its hero when it is drawn, at random. A max HP increase also heals the hero by the HP gained.

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

- **Feathers per run:** 1 per battle won, plus 1 per 50 gold earned, plus 10 for completing the run. Earned gold counts kills, encounters and slot machine gold; double-or-nothing winnings and starting gold from Purse do not count. Feathers are awarded for losses and abandoned runs too.
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
- **Toon shading:** every unit is drawn with the `Featherguard/Toon` shader: flat light bands, a cool shadow colour and a dark outline. The settings live in `Assets/Art/Materials/Toon.mat`; each unit keeps its own texture and colour.
- **Impact effects:** an ability can draw an extra effect on the target when its hit lands (`AbilityDefinition.impactEffect`). The Bies' Rending Claws draws three ember claw slashes with a dark outline that cut in, hold and fade within about 0.35 s; their shape and colours are tuned on the ability asset.
- **Icons and sounds:** procedural, generated by `IconFactory` and `SfxSynth`.
- **Timing:** feedback timings live in `BattlePresentation.asset`. Hitstop, camera shake and popups are configured in `GameConfig.feedback`.
- **UI:** landscape, 1920×1080 reference resolution, safe area, generous tap tolerance, pause menu with sound toggle and an option to abandon the run.

## Agreed decisions

Record decisions here with the date and who agreed, so both people's AIs follow them.

- 2026-11: Gambling is a reward layer between battles; in-game currency only, never real money.
- 2026-11: Battle logic stays a pure model emitting events; presentation only replays them.
- 2026-11: The whole project is in English, including in-game text, data and docs.
- 2026-11-07 (Aurel and Patryk): The game is heading towards the turn-based Slavic folklore RPG described in [`GDD.md`](GDD.md). Where `GDD.md` and the current build differ, `GDD.md` is the target; this file keeps describing the build as it is until each part is reworked.
- 2026-11-08 (Aurel, answering the review of #7): The birds-and-lizards setting is dropped entirely; the game is set in Slavic mythology. The whole game is 3D and everything is modelled.
- 2026-11-08 (Aurel, answering the review of #7): The target design has no gambling. The gambling reward layer above stays in the current build until it is reworked, and may come back one day only as a separate decision.
