# Game Design Document – Slavic folklore turn-based RPG

Agreed by Aurel and Patryk on 2026-11-07. This is the **target design** the game is moving towards. [`GAME_DESIGN.md`](GAME_DESIGN.md) describes what the current build actually does. Working title to be decided.

## Vision and pillars

A turn-based RPG set in a dark fairy-tale world of Slavic folklore. A party of heroes from the Sacred Oak Stronghold travels through five lands.

Genre core: party-versus-party turn-based combat, a shared party Rage, and collecting and forging equipment (a weapon adds damage, an artifact adds HP).

What sets us apart:
- **Every class is a separate character.** The player collects heroes and builds a party of 3 for each specific fight.
- **Timing (QTE).** Clicking at the right moment in the right place strengthens an attack or reduces damage taken. This is our main differentiator and will be expanded further.
- **Slavic folklore with a twist.** Monsters are taken from Slavic folklore and given our own dark storybook look.

Pillars:
- **Readable combat with depth:** one decision per character per turn; depth comes from synergies, statuses and timing.
- **Hero collection:** a large roster and party building for each fight; heroes are earned by playing, through story missions and side activities.
- **Equipment that changes play:** items change the play style, not only the numbers.
- **Short sessions:** a level takes 10–15 minutes.

Not in the game: gambling. The between-battle gambling layer of the current prototype is dropped; it may come back one day as a separate decision.

## World and tone

The world is a Slavic fairy tale in a dark tone, built on Slavic mythology.

| Place | Role | Boss |
|---|---|---|
| Sacred Oak Stronghold | main screen | – |
| Rye Fields | land 1, open from the start | Lady Midday |
| Marshes | land 2 | Water Maiden |
| Bald Mountain | land 3 | Zmey |
| Old Forest | land 4 | Leshy |
| Nav, the land of the dead (beyond the Smorodina River and the Kalinov Bridge) | land 5 | to be decided |

The Stronghold is the main screen and grows with the player. It is a fortified camp at night, a grim refuge on the edge of death: a palisade, dead trees, a graveyard with wisps, and the campfire at its heart, where the party gathers (the concept is [`Art/Concepts/Stronghold.png`](../Art/Concepts/Stronghold.png); its look and the list of its models with their concept sheets are in [`STYLE_GUIDE.md`](STYLE_GUIDE.md#sacred-oak-stronghold)). Its buildings:
- **Forge:** forging equipment (see Forge).
- **Wise Woman's Hut:** brewing potions, nothing else.
- **Sacred Oak:** the Priest's story scenes and, after the MVP, the bestiary (unlocked by fighting, with boss weaknesses).
- **Tavern:** the entrances to the two extra modes, Endless Run (Veles' Well) and Arena (Kupala Night).
- **Shop:** a merchant with a rotating offer (see Shop).

Extra modes: Veles' Well (Endless Run) and Kupala Night (Arena), both entered from the tavern, and Buyan Island (bonus act).

## Loop and screen flow

**Embark** in the Stronghold opens the world map, where the player picks a land and an activity. Every level follows the same flow:

Stronghold → world map → battle preparation (a separate screen for a single fight and for a waves fight) → battle → result (victory or defeat) → world map.

The **Camp** is a separate screen, not the main screen. It is built around a campfire where every hero has a fixed place, so the camp fills up as the player collects heroes. In the Camp the player picks the party of 3, looks at a hero's description and equipment, and opens the Storage and the potion bag.

Long-term loop: a level on the map → rewards (materials, gold, loot, schematics, EXP) → the forge and the Wise Woman's Hut in the Stronghold → new equipment and potions → harder story missions.

## Characters and party

The player collects heroes into a roster (6 for the first alpha and 12–15 in the full game; the roster is built to keep growing after that) and picks a party of 3 in the Camp; the battle preparation screen leads back to the Camp to change the party before a fight. New heroes are unlocked by playing: story missions and side activities can reward them. Every character has a role, an attack, a support action and a Rage action. Heroes are of mixed genders; a class does not determine gender.

Starting roster (prototype):

| Character | Role | Attack | Support | Rage |
|---|---|---|---|---|
| Bogatyr (Knight) | tank | Sword Strike | Guard: an ally passes 50% of its damage to the Bogatyr, plus a Shield | Bastion: Shield for the party, Taunt |
| Priestess | healer | Ray | Prayer: heal and cleanse | Grace: heal and cleanse the party |
| Volkhv of Svarog (Fire Mage) | mage | Fireball + Burn | Flaming Weapon on an ally | Fire Rain on all enemies |
| Witch of Marzanna (Ice Mage) | mage | Ice Bolt + Freeze | Ice Shield | Blizzard: Freeze all enemies |
| Forest Archer | DPS | Piercing Shot | Target Lock: the next attack applies Mark | Arrow Hail + Mark on all enemies |
| Minstrel (Bard) | support | Dissonance, chance to Stun | Song of Courage: +25% party damage | Hymn: Empower, heal and cleanse |

Further classes (Paladin, Berserker, Druid, Alchemist, Necromancer, Rogue, Monk, Engineer, Oracle) are planned and are waiting for their Slavic versions.

Character progression: heroes earn EXP from fights that reward it, and every hero has its own level (cap 10).

## Combat

Combat is turn-based, party versus party, in side view: first all heroes act in any order, then all enemies. A battle has 1–3 waves, and the party does not heal between waves.

Round flow:
1. Start of the player's turn: damage from effects on the heroes.
2. The player drags a hero onto an enemy (attack) or onto an ally or itself (support). Each hero acts once; the turn can also be ended early.
3. An attack triggers a QTE sequence.
4. Enemy turn: damage from statuses on the enemies, then each enemy attacks; the player can block.
5. End of round: statuses tick down at the end of their own side's turn; check for victory, the next wave or defeat.

### Timing (QTE)

| Moment | How it works | Good | Perfect |
|---|---|---|---|
| Attack | the hero's own gesture (a ring, a slash, a series of rings, a charge) | +25% damage | +50% damage, extra Rage and the ability's perfect effect |
| Support | a harder gesture (tracing a line, guiding a light, spinning) | +20% healing and Shield | +40%, extra Rage and the ability's perfect effect |
| Rage | the hardest gesture (a sequence, or a sign drawn from memory) | +25% damage, +20% healing and Shield | +50% / +40%, extra Rage and the ability's perfect effect |
| Block | a BLOCK! flash just before the hit; clicking before the flash is wasted | −30% damage | −60% damage and extra Rage |

Every hero has its own gestures, and the stronger the action, the harder the gesture; a miss always gives the normal effect. The gestures are listed in the [timing spec](design/combat/timing.md). The enemy wind-up has a random length, so the rhythm cannot be memorised. The auto-timing accessibility option always gives "good", never "perfect". Some effects only work with perfect timing (for example an extra Burn stack or a guaranteed Stun).

### Party Rage

A shared meter filled by hits dealt and taken and by perfect timings. A full meter lets one character use its special action. Rage does not use up that character's move, affects the whole side and is timed with the character's hardest gesture.

### Statuses

| Status | Effect |
|---|---|
| Bleed | damage every turn for 3 turns (applied by the Bies' Rending Claws) |
| Burn | damage every turn, stacks up to 5 |
| Freeze | +30% damage taken; a Frozen enemy also acts last in the enemy turn (on a hero only the damage part applies) |
| Curse | −30% damage dealt (no hero or enemy applies it yet) |
| Shield | absorbs damage up to its value |
| Taunt | enemies must attack this character |
| Stun | skips the turn |
| Mark | +50 percentage points of critical hit chance; a critical hit deals ×1.5 damage |
| Empower | +25% damage dealt |

## Enemies and bosses

Enemies fall into archetypes, and the player learns to read a wave's composition and pick counters. Regular enemies are the monsters as folklore knows them, with their folklore gender. Bosses may be female where it suits the character (such as Lady Midday), but never only for the sake of it.

| Archetype | Behaviour | Good counter |
|---|---|---|
| Slasher | hits one target hard | a tank with Guard or Taunt |
| Shieldbearer | protects allies | Forest Archer (piercing) |
| Shaman | heals or buffs enemies | fast damage, Stun |
| Poisoner | applies statuses | Priestess (cleanse) |
| Scout | flees and returns with reinforcements | Witch of Marzanna |
| Elite | 2 actions per turn | perfect block |

### Bestiary

| Land | Enemies | Boss and mechanic |
|---|---|---|
| Rye Fields | Bandit, Werewolf, Polevik, Wolf; elite: Bies | Lady Midday: calls reinforcements and asks riddles (special QTE) |
| Marshes | Drowned Man, Will-o'-the-Wisp, Bog Hag, Rusalka | Water Maiden: drags a hero underwater for 2 turns |
| Bald Mountain | Sabbath Witch, Planetnik, Giant, Sirin | Zmey: announces a great fire strike, teaches the perfect block |
| Old Forest | Wild Woman, Mamuna, Licho, Forest Grandfather | Leshy: the old lord of the forest; summons roots and shuffles the party's formation |
| Nav | Wraith, Strzyga, Nightmare, Death | to be decided |

## World structure and progression

The game has 5 lands on a single map. At the start only the Rye Fields are open; the other lands unlock as the player completes story missions.

**Story missions:** every land has 5 main story missions (25 in total). They appear on the world map as Main quest activities, like the side activities; the Sacred Oak only holds the story scenes between them. The story does not finish one land before moving to the next: consecutive story missions alternate between lands, so the player keeps moving across the map in a non-linear way, even though the story itself is fairly linear. Only the next story mission is available at a time, and a land opens when the story first reaches it. Each land's 5th story mission is its boss; once beaten, the boss can also return in its land as a side activity. The full order of the 25 missions is in [`design/world-progression.md`](design/world-progression.md).

**Side activities:** an open land shows 1 to 3 activities at a time, the next story mission included when it is there; the others are side activities drawn at random from the land's pool, and they are drawn again after every won activity. Their threat follows the story, so every land stays worth visiting ([`design/side-activities.md`](design/side-activities.md)). They let the player grow stronger when the next story mission is too hard at the current stage.

**Activity types** on the world map: Single fight, Waves fight, Puzzle fight, Elite fight (a fight against an Elite enemy), Boss fight and Main quest (a story mission).

**Rewards:** every fight gives one reward, of one kind: materials, gold, loot (equipment), schematic, EXP or unlock. Side activities draw theirs; story missions have their own unique rewards and also advance the story.

A land's **Law** is a combat rule that applies to battles in that land. Every land also has 3 materials of its own, one of each rarity (see Materials):

| Land | Law | Who shines | Materials (Common · Uncommon · Rare) |
|---|---|---|---|
| Rye Fields | Noon Heat: every 3rd turn, characters without a Shield lose part of their HP | Shields, tempo | Flax · Wolf Pelt · Last Sheaf |
| Marshes | Depths: some enemies submerge; wisps switch targets | area attacks, cleansing | Marsh Herbs · Amber · Wisp in a Jar |
| Bald Mountain | Storm: lightning strikes a random character; dodge it in a QTE | block, cleansing | Brimstone · Mountain Ore · Thunderstone |
| Old Forest | Misleading: the party's formation shuffles; enemies stay hidden until Marked | Mark, control | Yew Wood · Tinder Fungus · Old Oak Heartwood |
| Nav | Chill of Nav: healing is halved; the fallen rise as ghosts unless finished with fire | fast damage, fire | Bone Ash · Gromnica Wax · Nav Essence |

Progress and forcing:
- Party strength = party level (the average level of the 3 picked heroes) + equipment + heroes. Before a battle the player sees a threat rating relative to their party (Easy, Even, Dangerous, Madness), not a power number.
- Every unlocked story mission and activity can be attempted right away. Winning above your strength grants **Daring**: the reward is worth ×1.5 at Dangerous and ×2 at Madness (more of it, or a better chance of a higher rarity).

After the MVP: Land Favour (a merchant and a blessing that weakens the Law).

## Equipment, forge and economy

Power grows mainly through equipment, while new heroes bring new tactical options rather than raw strength.

**Equipment:** every character has 2 slots, a weapon (attack) and an artifact (health). Weapons are made for a specific role; artifacts are universal and fit everyone. Every weapon and artifact has a minimum hero level needed to equip it. Items from the same set give a bonus (for example "Frost Set": attacks on Frozen enemies Stun); every land has 1–2 sets.

| Rarity | Special trait | Source |
|---|---|---|
| Common | none | level loot, forge |
| Rare | 1 weak trait | level loot, forge |
| Epic | 1 strong trait | bosses, forge with rare materials |
| Legendary | a play-style-changing trait + a set piece | bosses |

The shop can also offer equipment.

**Storage** is the screen with all the player's equipment: it filters by type and role, shows which hero wears an item, and lets the player unequip and salvage items. A separate tab holds the materials.

**Salvage:** equipment (not potions) can be salvaged. The item is destroyed and the player gets materials back. Items cannot be sold.

**Materials:** every land has 3 materials of its own, one Common, one Uncommon and one Rare (listed with the lands). Four Epic and three Legendary materials are universal: any land can drop them, rarely, and Legendary ones more rarely than Epic. Materials are used to forge equipment and to brew potions. They come from level rewards, Salvage and the shop.

| Rarity | Universal materials |
|---|---|
| Epic | Firebird Feather, Dead Water, Serpent King's Crown, Kupala Dew |
| Legendary | Fern Flower, Living Water, Alatyr Stone |

**Schematics** are a kind of level reward and can also be bought in the shop. A schematic unlocks a recipe: without it the matching item cannot be forged, or the matching potion brewed. A schematic is the fixed reward of predefined side activities; once it is won, they leave the pool. The map and the preparation only show that a fight rewards a schematic, not which one. Some schematics are in the pool from the start, others join at a story threshold.

**Forge:** the player picks a recipe unlocked by a schematic, puts in materials and gold, and plays a forging minigame. The forged item goes to the Storage. *TODO:* the minigame; the current proposal is a timing ring like the attack's, with three strikes (see Open questions).

**Potions** (healing, cleansing, Rage, protection, thrown potions against enemies, and rare ones that revive; the list and recipes are in [`design/potions.md`](design/potions.md)) are brewed at the Wise Woman's Hut from a schematic, materials and gold; the Healing Draught needs no schematic. They are single-use. The player takes at most 3 potions into a battle, chosen beforehand from the potion bag. In battle the picked potions sit in a tile in the lower left corner, and during the player's turn the player drags a potion onto its target with no timing QTE; it is an extra action that uses up no hero's action.

**Shop:** a merchant with 3 slots, each holding a random item from the shop's pool: materials, schematics, equipment or potions. The offer changes on a timer, for now every 24 hours.

| Currency | Source | Spent on |
|---|---|---|
| Gold | battles | forge, potions, shop |
| Land materials | levels in that land, Salvage, shop | recipes (forging and brewing) |
| Universal materials (Epic, Legendary) | rare drops in levels of any land | recipes (forging and brewing) |

## Art direction and platform

The style is dark cartoon with good-looking characters: a thick hand-drawn outline, heavy cool shadows, a single warm light, and folklore instead of generic fantasy. Rendered in 3D so it reads like a hand-drawn illustrated card game, always our own: never copy another game's characters, shapes or art. Chunky silhouettes that read at phone size, banded toon shading, dark muted backgrounds with saturated characters, a grim but playful storybook tone, and UI and VFX in the same flat, outlined look. The concrete rules, palettes and shader settings are in [`STYLE_GUIDE.md`](STYLE_GUIDE.md).

- **3D:** the whole game is 3D and everything is modelled; there are no 2D sprites.
- **Perspective:** the camp and the world map in a light top-down view (3/4); combat seen from the side at a slight angle (a side-on isometric view); heroes face right, enemies face left.
- **Animation:** skeletal animation of the 3D models; snappy and pose-to-pose, with strong anticipation, fast hits, short holds on key poses and squash and stretch on impacts.
- **Audio:** a clear perfect-timing sound is part of the mechanics; music and voices come later.
- **Interface:** shape and sound, not only colour (accessible for colour-blind players); fast-forward of animations ×2 and ×3.

**Platform:** we design mobile-first in landscape (16:9, touch and mouse share one pointer), and Steam is the second platform on the same build. Engine: Unity 6000.6.2f1 (the exact version is pinned in `CLAUDE.md`), URP, Input System. The first release is narrower: see the scope in [`ROADMAP.md`](ROADMAP.md).

**Business model:** the first release is free, with no ads or purchases (see [`ROADMAP.md`](ROADMAP.md)); free-to-play is the recommendation for later, to be decided after testing.

## Open questions

- What drives the main story and how it ends.
- The boss of Nav.
- Whether a land's Law grows stronger as the player progresses through that land.
- How threat levels (1–10) are assigned to story missions (side activities follow them).
- How a Puzzle fight plays, and how Lady Midday's riddles (special QTE) relate to it.
- Which story missions and side activities unlock which heroes.
- Where equipment of each rarity comes from (level loot, forge, shop).
- Materials use Common, Uncommon, Rare, Epic and Legendary, while equipment uses Common, Rare, Epic and Legendary: should the two scales differ?
- How rare the universal Epic and Legendary materials are, and whether Salvage or the shop can give them.
- How Salvage pays out: how many and which materials come back, and whether an item a hero wears can be salvaged.
- The forging minigame: the proposal is a timing ring like the attack's, with three strikes. Does the result affect the item's quality?
- Costs of forging and brewing, shop prices, and what happens to a shop slot after a purchase.
- Defeat: the wireframe shows no rewards and no EXP, a Retry button and a return to the map. Does that apply, and are the potions used in the battle lost?
- Endless Run (Veles' Well) and Arena (Kupala Night): rules, rewards and scoring.
- Where Buyan Island is entered.
- The battle preparation screens show "Difficulty: Medium / Hard", while the threat rating uses Easy, Even, Dangerous and Madness: which names do we use?
- The wireframes show equipment rarity and material kind only by the colour of the frame, while the interface must read by shape and sound, not only colour.
- Whether the merchant of Land Favour is the Stronghold shop or a separate one.
