# Featherguard – Game Design

Source of truth for what is in the game **now**. The agreed target design is [`GDD.md`](GDD.md). The numbers come from the data assets in `Assets/Data` (seeded by `Assets/Editor/Setup/*Catalog.cs`). Whenever a PR changes content or rules, update this file in the same PR.

## Pillars

- Short turn-based battles: a party of three heroes against waves of enemies (the bird and lizard art of the prototype stands in until their own is made).
- A loop of Stronghold → world map → battle preparation → battle, following the story across the lands (see Game flow).
- Built for mobile: landscape, touch, one-handed tapping.

## Battle

- **Turns:** every battle starts with the player's turn, then the enemy turn, and so on.
  - On the player's turn every living, non-stunned hero acts once, in any order. Units stand on a ring coloured by their class: Warrior yellow, Defender blue, Mage purple, Support green, enemies red (`BattlePresentationConfig`). Enemies always have theirs; a hero shows its ring only while it can still act this turn and loses it as soon as its action is chosen. Drag a hero onto an enemy to attack it, or onto an ally to use the support ability on that ally; tapping a hero uses its support on itself (or on the party, for a party support). While dragging, a dotted arrow follows the finger; only the valid target under the finger is highlighted, with a white ring, and the arrow turns red over an enemy and green over an ally. Dropping anywhere else does nothing.
  - The Rage button arms Rage once the meter is full; the next hero dragged onto a target, or tapped when its Rage hits every enemy or ally, uses its Rage action.
  - There are no hero cards: each hero has a health and status bar above its head, like the enemies.
  - When the **Rage** meter is full, the selected hero can use its special instead.
  - The turn ends when every hero has acted. Enemies then act one by one in the order they entered the battle (summons last); stunned enemies skip.
- **Rage (`BattleRules`):**
  - one meter shared by the party, max 100;
  - it starts every battle at 0;
  - it fills by 0.45 per point of damage a hero deals (× that hero's RageGain) and by 0.25 per point of damage the enemies deal to the party;
  - a hero special consumes the whole meter. Enemy specials do not use the meter.
- **Damage:** power × attacker's Damage stat × timing × random 90–110%, then crit (15% base chance plus the target's CritChanceTaken, ×1.5 base damage), then × the target's DamageTaken stat, rounded once. A Shield soaks what it can; the rest is taken from health. Lifesteal heals the attacker by that share of the damage that reached health. Rage only counts damage that reached health.
- **Healing:** power × the healer's Healing stat. No randomness and no crits; it cannot exceed max HP or revive a fallen unit.
- **Statuses:**
  - a status is a list of behaviours (`StatusBehaviour`): stat modifiers, damage over time, skip turn, force targeting, absorb damage, act last, consumed on attack, consumed by a critical hit;
  - damage over time and stuns resolve at the start of the owner's turn, so poison can kill a unit before it acts; damage over time is potency × stacks, cannot crit or be blocked, is multiplied by DamageTaken, is soaked by Shield and gives Rage to the side that applied it;
  - a status applied by the owner's own side lasts through the owner's next turn; one applied by the opponent ticks down at the end of the owner's turn;
  - reapplying keeps the stronger duration and potency; a stacking status (Burn) also adds a stack up to its maximum;
  - Shield points are the application's potency × the caster's Healing stat; reapplying keeps the larger value, and the Shield leaves at 0 points;
  - an application can require a perfect attack timing (`StatusCondition.PerfectTiming`);
  - a cleanse (`CleanseEffect`) removes harmful statuses; death clears all statuses; heroes keep theirs between waves;
  - enemies with an act-last status (Freeze) act after the other enemies.
- **Attack timing:** an attack can carry a timing result. Good gives +25% damage and Perfect +50% damage on every hit plus +10 Rage once for the action (`BattleRules`); Miss is normal damage. Support, healing and the Rage action ignore it unless the ability is `alwaysTimed`; then Good adds +20% and Perfect +40% to its healing and Shield points, and its damage gets the attack bonus. Effects wrapped in `PerfectTimingEffect` (or status applications with `PerfectTiming`) only happen on a perfect result. A gesture asks for the timing (see Presentation) until #19 brings the final battle screen.
- **Retaliation:** a status with `Retaliate` (Thorns, Sparks) returns its potency as damage to every opponent hit that lands on its owner, and can put a status on the attacker (Sparks: 30% Stun). An attacker killed this way stops its remaining hits.
- **Chain damage:** `ChainDamageEffect` hits the target, then every other enemy in slot order, each jump keeping 60% of the previous hit.
- **Overflow healing:** a `HealEffect` with `overflowShield` turns healing beyond full health into Shield points for 2 turns.
- **Block timing:** with `BattleSetup.AwaitBlocks` the enemy turn stops before each damaging enemy action (an `EnemyWindup` event) and `Block` resolves it: Good removes 30% and Perfect 60% of the damage for every hero hit, and Perfect adds +10 Rage once. Enemy actions without damage resolve at once. The game turns it on together with the temporary timing prompts.
- **Taunt** only redirects single-target abilities; abilities that hit all enemies are unaffected.
- **Waves:** an encounter can have several waves. When the last enemy of a wave falls, the next wave enters and a new player turn starts.
- **Enemy AI:**
  - acts once per enemy turn, Elites twice (`actionsPerTurn`); a special that is due replaces the last action of the turn;
  - uses its special on its first turn and then every N turns (Chieftain: N = 3);
  - heals the most wounded ally below 60% HP (75% chance) or buffs an ally that lacks the buff (45% chance);
  - otherwise attacks, preferring wounded heroes and heroes it can finish;
  - must attack a taunting hero.

### Heroes

| Hero | HP | Attack | Support | Special (full rage) |
|---|---|---|---|---|
| Warrior | 140 | Sword Cut – melee, 30 dmg; perfect: Bleed (5/turn, 3 turns) | War Shout – Empower on the party for 2 turns; perfect: 3 turns | Hero's Blow – melee, 85 dmg, heavy; perfect: Stun |
| Thorn Warden | 165 | Thorn Lash – melee, 16 dmg + Taunt on self; perfect: Weaken | Thorn Guard – Thorns (8) on an ally for 2 turns; perfect: Shield (20) | Bramble Wall – Thorns (20) on the party + Taunt on self; perfect: party Shield (25) |
| Healer | 110 | Amber Spark – magic bolt, 14 dmg; perfect: Mark | Oak Blessing – heal 40, overflow becomes Shield; perfect: cleanse | Grace of the Oak – heal 45 and cleanse the party; perfect: party Shield (25) |
| Thundercaller of Perun | 100 | Chain Lightning – 22 dmg, jumps to every other enemy at 60% each jump; perfect: 50% Stun | Spark Ward – Sparks (8) on an ally for 2 turns; perfect: Empower | Perun's Storm – all enemies, 50 dmg; perfect: Stun all |

Every hero has a class (`UnitDefinition.heroClass`): the Warrior is a Warrior, the Thorn Warden a Defender, the Healer a Support and the Thundercaller a Mage.

The four heroes come from the new roster and are the only heroes at the campfire. Until their own art is made they use the bird prototype's Knight, Owl and Archer models, portraits and animators as placeholders; the prototype heroes themselves (Robin, Jay and the Owl) are gone. Each hero action has its own gesture:

| Hero | Attack | Support | Rage |
|---|---|---|---|
| Warrior | Slash to the right | Series of 2 rings | 3 Slashes: right, up-left, down |
| Thorn Warden | Ring | Trace a thorn line | Trace a shield, then a Ring |
| Healer | Charge: hold, release in the gold | Guide the light along a wave | Draw a circle from memory |
| Thundercaller | Series of 3 rings | Spin twice | Draw a lightning sign from memory |

### Enemies

| Enemy | HP | Abilities |
|---|---|---|
| Thug (Grunt) | 80 | Club – melee 15 |
| Shaman | 70 | Hex – bolt 12, 50% Weaken; Dark Mending – heal 25 |
| Brute | 170 | Crush – melee 26, heavy, 25% Stun |
| Spitter | 65 | Venom – bolt 9, 70% Poison (6/turn, 2 turns) |
| Shieldbearer (Guard) | 110 | Spear Thrust – melee 14; Shield Wall – Shield (30) on an ally + Taunt on self |

Each encounter also scales enemy HP and damage, as listed in the Encounters section.

**Bies (test encounters and the map's Elite fights):** the first hand-made 3D character, the Rye Fields elite of the target design. 260 HP. An Elite: two actions per enemy turn. Rending Claws – melee 24, heavy, Bleed (6 per turn, 3 turns). On its first turn and every 3rd turn after, Howl (Weaken on all heroes) replaces its second action. Howl is a placeholder until Sic the Pack and the Wolves exist. It only appears in the `BiesTest` encounter; to fight it in the editor, select `Assets/Data/Encounters/BiesTest` and run **Featherguard > Test Encounter > Use Selected**; every battle in the editor then uses that encounter until **Featherguard > Test Encounter > Clear**. The choice is stored only on your machine, never in the repo, and builds ignore it.

**Wolf (test encounters and the map's Elite fights):** the Bies' pack and the first hand-made quadruped. 70 HP. Bite – melee 14. On its first turn and every 3rd turn after, Pack Howl (Empower on all its allies, the Bies included) replaces the Bite. Placeholders until Sic the Pack and the enemy archetypes exist. The `BiesPack` encounter puts the Bies in the middle slot with a Wolf on each side (wave order Wolf, Bies, Wolf); pick it the same way as `BiesTest`.

### Boss: Chieftain

- 460 HP.
- Chieftain's Wrath: melee 34, heavy, 35% Stun.
- On its first turn and every 3rd turn after, Earthquake: hits all heroes for 22, 50% Weaken.
- At 50% HP it enrages once: +40% damage and summons two Thugs into free slots (a slot is free when no living enemy stands in it). If all slots are taken, fewer or no Thugs appear.

### Statuses

| Status | Effect |
|---|---|
| Shield | Soaks damage up to its points |
| Empower | +25% damage |
| Poison | Takes the potency as damage at the start of the owner's turn (harmful) |
| Stun | Skips the turn (harmful) |
| Weaken | −30% damage (harmful) |
| Taunt | Opponents' single-target abilities must target this unit |
| Bleed | Takes the potency as damage at the start of the owner's turn (harmful) |
| Burn | The same, potency × stacks, up to 5 stacks (harmful) |
| Freeze | +30% damage taken; an enemy acts last (harmful) |
| Curse | −30% damage (harmful) |
| Mark | +50 points of crit chance against the owner, gone after the first crit it takes (harmful) |
| Thorns | Every hit the owner takes returns the potency as damage to the attacker |
| Sparks | Like Thorns, plus a 30% chance to Stun the attacker |

The default duration is 1 turn unless stated otherwise. Bleed is used by the Bies; Burn, Freeze, Curse and Mark exist for the reworked heroes and enemies and use placeholder icons until their own are drawn. The rules follow `docs/design/combat/statuses.md`.

## Encounters

Nine encounters (`Assets/Data/Encounters`, seeded by `EncounterCatalog`); the activities on the world map use them as placeholders:

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

## Game flow

- **Start:** the game opens on the Stronghold, its own scene (`Stronghold.unity`, the main screen). It is a 3D camp at night in a light top-down view, seen through mist, with dead trees and graves. Lanterns, glowing windows, the forge fire, a green glow at the Wise Woman's Hut, candles under the Sacred Oak and wisps over the graves light it, and the party stands around the campfire. The player taps a place: the Wise Woman's Hut opens brewing (see Potions); the Forge, Sacred Oak, Tavern and Shop say "coming soon" for now, as does the Settings button. Tapping the signpost (**Embark**) opens the world map; Embark needs a full party.
- **Debug menu:** in development builds and the editor, a faint "dbg" button in the top-left corner of every screen opens a debug menu: **Win battle** (only during a battle) ends the battle at once as a victory, **Add materials** opens a list of every material with its icon and x1, x5 and x10 buttons that add that many to the Storage, **Wipe storage** (a second tap confirms) empties it, **Add potions** opens a list of every potion with x1, x5 and x10 buttons that add that many to the potion bag, **Learn a schematic** lists every potion with a schematic and learns the one tapped (learned ones are greyed out), **Learn all schematics** (a second tap confirms) unlocks every recipe, **Reset schematics** (a second tap confirms) forgets every learned schematic, **Empty potion bag** (a second tap confirms) empties it, **Reroll side activities** draws the map's side activities again, and **Reset all progress** (a second tap confirms) wipes the save and returns to the Stronghold. Release builds have no debug menu.
- **Camp:** tapping the campfire (or a hero at it) flies the camera to the fire. Six fixed seats (`GameConfig.campSeats`) hold the heroes of `GameConfig.heroes` in order; the other seats are empty, "not joined yet". Tapping a hero moves them into or out of the party of 3; a fourth is refused. Holding a hero shows their abilities, with level and equipment "coming soon". **Potions** opens the potion bag with each potion's icon, count and name. **Storage** opens the materials the player owns, with their icons, counts and names, in the order of `MaterialConfig`. The party is saved (first 3 heroes by default) and goes into every activity battle. **Back** returns to the Stronghold. Rules: [`design/party.md`](design/party.md). The buildings are code-built placeholders until their models exist.
- **World map:** its own scene (`WorldMap.unity`), a tabletop map at night seen from above. The Stronghold sits in the middle, with roads to five lands: Rye Fields, Marshes, Bald Mountain, Old Forest and, beyond the river, Nav. A land opens when the story reaches it; until then mist covers it and its name says Locked, while an open land shows its story progress. Each open land shows its 1 to 3 activities as buttons under its name: the border colour tells the reward (blue EXP, yellow gold, green materials, red loot, cyan schematic, purple unlock, brown the Main quest) and the icon the kind of fight (circle single, two circles waves, clover puzzle, elite, crown boss, flag Main quest), with the threat rating against the party under it; a legend in the corner lists the colours. The next story mission takes one of those places as the **Main quest**. Reward kinds are placeholders set per activity in `WorldCatalog`. Schematic fights come from `PotionCatalog`. Side activities are drawn again after every won mission. Tapping an activity opens its battle preparation. **Back** returns to the Stronghold. Lands and activities live in `Assets/Data/World` with placeholder encounters. Rules: [`design/world-progression.md`](design/world-progression.md).
- **Battle preparation:** its own scene (`Preparation.unity`): the party stands on a night road facing the first wave of the activity. A panel shows the activity, its kind (with the number of waves for a waves fight), the land and its Law, the threat rating with Daring, the gold reward and the party. **Change party** opens the Camp straight away, and **Back** in the Camp returns to the same preparation. Potions are "coming soon". **Fight** needs a full party and starts the battle with the activity's encounter and the party. **Back** returns to the map. The chosen activity is saved, so the flow survives a restart.
- **Activity battles:** a win advances the story when it was the Main quest, draws the side activities again and shows the victory screen with the reward: the gold amount (with Daring) for a gold activity, the materials won for a materials activity, the learned recipe for a schematic fight, otherwise its kind. Materials are kept in the Storage and recipes in the save; gold and the other kinds are not kept yet. A loss grants nothing; **Retry** returns to the preparation and **World map** to the map. Leaving from the pause menu returns to the map. The Law is not applied in combat yet.
- **Materials:** 22 materials (`Assets/Data/Materials`, seeded by `MaterialCatalog`), each with its icon in its rarity frame: three per land (Common, Uncommon, Rare) and four Epic and three Legendary ones that drop in any land. Winning an activity that rewards materials gives the land's materials: 2 for a single or puzzle fight, 3 for waves or an Elite, 4 for a boss, more with Daring; each is Common, Uncommon or Rare by weight (70 / 25 / 5, Uncommon and Rare rising with the threat level), and the win also has a 2% chance of a universal Epic and a 0.3% chance of a Legendary one. The numbers live in `MaterialConfig`.
- **Potions:** 15 potions (`Assets/Data/Potions`) and their recipes (`Assets/Data/Recipes`), seeded by `PotionCatalog` from [`design/potions.md`](design/potions.md); potions have no rarity. Tapping the Wise Woman's Hut opens brewing: a grid of every potion, where a recipe not learned yet is dark and Locked. Picking a potion shows its effect, how many the player owns, and each ingredient with the amount owned and needed for the chosen count; − and + choose 1 to 10 (`PotionConfig.maxBrew`), and **Brew** takes the materials and adds the potions to the bag. Recipes cost no gold yet. The Healing Draught is known from the start. Potions cannot be used in battle yet.
- **Schematic fights:** every other potion has one schematic fight, a single fight among its land's side activities from a story threshold (`ActivityDefinition.schematic`, `availableAfter`): Cleansing Brew, Wolf's Fury and Sheaf Ward in the Rye Fields from the start, Wisp Oil and Bog Mist in the Marshes after mission 3, Fire Flask and Storm Draught on Bald Mountain after 6, Heartwood Brew in the Old Forest after 11, Gromnica Light in Nav after 15, and the potions of Epic and Legendary materials after their lands' bosses (Firebird Draught 12, Dead Water Tincture 17, Kupala Dew Elixir 21, Living Water 23, Fern Flower Elixir 25). The map and the preparation only say the reward is a schematic; the victory screen names the recipe learned. A fight whose schematic is learned never comes back. Puzzle fights reward EXP instead of a schematic.
- **Battle without an activity:** pressing Play in the Battle scene with no chosen activity opens the world map, unless a test encounter is set (**Featherguard > Test Encounter**), which the battle then fights. A test encounter also replaces the encounter of any activity battle in the editor.
- **Save:** `featherguard_save.json` in the persistent data path keeps the party, the story progress, the chosen activity, the map's side activities, the materials in the Storage, the potion bag and the learned schematics.

## Presentation

- **Visuals:** low-poly characters, generated procedurally in Blender (`Art/Blender`), with animations Idle, Attack, Cast, Hit, Death and Victory. Portraits are rendered from the same models.
- **Toon shading:** every unit is drawn with the `Featherguard/Toon` shader: flat light bands, a cool shadow colour and a dark outline. The settings live in `Assets/Art/Materials/Toon.mat`; each unit keeps its own texture and colour.
- **Impact effects:** an ability can draw an extra effect on the target when its hit lands (`AbilityVisuals.impactEffect`). The Bies' Rending Claws draws three ember claw slashes with a dark outline that cut in, hold and fade within about 0.35 s; their shape and colours are tuned on its AbilityVisuals asset.
- **Timing prompts (temporary, `BattlePresentationConfig.timingPrompts`):** every timed hero action plays the gesture on its `AbilityVisuals.gesture`. Without one, it shows a white ring shrinking onto a gold circle on the target for 0.8 s; a tap anywhere or Space is judged against the moment they meet: within 0.06 s Perfect, within 0.15 s Good, otherwise Miss (windows in `BattleRules`, scaled by the gesture's `windowScale`). The other gestures play in a panel in the middle of the screen:
  - **Ring:** the ring above.
  - **Series:** numbered rings at different places, each tapped as it closes; Space works too.
  - **Slash:** swipe through the gold gate in the arrow's direction as the ring closes.
  - **Charge:** hold (or hold Space), release with the bar in the gold zone; holding too long is a Miss.
  - **Trace:** draw along the shown line from the gold dot; judged by how much of the line is covered and how far the stroke strays.
  - **Guide:** hold the light and follow it along the path; judged by the share of time the finger stays on it.
  - **Spin:** circle around the centre the given number of times; Perfect within 65% of the time limit.
  - **Sign:** a sign is shown, then drawn from memory; judged by a stroke matcher against the sign.
  - **Sequence:** several gestures in a row; Perfect only if every step is Perfect, Good if at least 60% of the steps hit.
  - Series and Sequence are judged the same way over their parts. Gesture logic is plain C# (`GestureRun` subclasses) drawn through a `GestureSketch` by the HUD.

  Before each damaging enemy action the game waits a random 0.4–1.2 s, then flashes BLOCK! over the hero 0.25 s before the hit; a tap before the flash is wasted. GOOD / PERFECT! (or BLOCK / PERFECT BLOCK!) pops up over the unit, with a placeholder sound from `SfxSynth`: a click for a good action, a metal clank for any block, and a bright chime on top of either for a perfect result. The enemy only moves and strikes after the block is judged; matching the cue to the animation comes with #19. With the switch off, every attack and block is a Miss and enemy turns don't wait.
- **Icons and sounds:** procedural, generated by `IconFactory` and `SfxSynth`.
- **Timing:** feedback timings live in `BattlePresentationConfig.asset`. Hitstop, camera shake and popups are configured in `GameConfig.feedback`.
- **UI:** landscape, 1920×1080 reference resolution, safe area, generous tap tolerance, pause menu with sound toggle and an option to leave the battle for the world map.

