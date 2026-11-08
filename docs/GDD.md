# Game Design Document – Slavic folklore turn-based RPG

Agreed by Aurel and Patryk on 2026-11-07. This is the **target design** the game is moving towards. [`GAME_DESIGN.md`](GAME_DESIGN.md) describes what the current build actually does. Working title to be decided.

## Vision and pillars

A turn-based RPG set in a dark fairy-tale world of Slavic folklore. A party of heroes from the Sacred Oak Stronghold travels through five lands.

Genre core: party-versus-party turn-based combat, a shared party Rage, and collecting and forging equipment (a weapon adds damage, an artifact adds HP).

What sets us apart:
- **Every class is a separate character.** The player collects heroes and builds a party of 3 for each specific fight.
- **Timing (QTE).** Clicking at the right moment in the right place strengthens an attack or reduces damage taken. This is our main differentiator and will be expanded further.
- **Slavic folklore with a twist.** Some monsters get female versions (for example a Lady Leshy instead of the Leshy).

Pillars:
- **Readable combat with depth:** one decision per character per turn; depth comes from synergies, statuses and timing.
- **Hero collection:** a large roster and party building for each fight; most characters are earned outside the main story.
- **Equipment that changes play:** items change the play style, not only the numbers.
- **Short sessions:** a level takes 10–15 minutes.

Not in the game: gambling. The between-battle gambling layer of the current prototype is dropped; it may come back one day as a separate decision.

## World and tone

The world is a Slavic fairy tale in a dark tone. The story is driven by the legend of Koschei the Deathless: his death is hidden in a needle, the needle in an egg, the egg in a duck, the duck in a hare and the hare in a chest. Each land's boss guards one part, and in the finale Koschei is immortal until the party destroys his death.

| Place | Role | Boss | Guards |
|---|---|---|---|
| Sacred Oak Stronghold | camp and main screen | – | – |
| Rye Fields | Act I | Lady Midday | the chest |
| Marshes | Act II | Water Maiden | the hare |
| Old Forest | Act III | Lady Leshy | the duck |
| Bald Mountain | Act IV | Dragoness | the egg |
| Nav, the land of the dead (beyond the Smorodina River and the Kalinov Bridge) | Act V | Koschei / Koscheia | the needle |

The Stronghold grows with the player. It has a forge, the Wise Woman's hut (potions and bestiary), a tavern (rumours and recruitment), the Priest at the Sacred Oak (story chapters) and the gate the party sets out through.

Extra modes: Kupala Night (arena), Veles' Well (endless mode, the equivalent of the tower from the first version of the GDD), Buyan Island (bonus act).

## Loop and screen flow

The Stronghold is the main screen with a single **Set out** button. The land and level are chosen on the land map, and after a battle the player returns to the map.

Long-term loop: a level on the map → rewards and materials → the forge in the Stronghold → new equipment and characters → a harder circle.

## Characters and party

The player collects heroes into a roster (6 for the first alpha and 12–15 in the full game; the roster is built to keep growing after that) and picks 3 before each battle. Every character has a role, an attack, a support action and a Rage action. Heroes are of mixed genders; a class does not determine gender.

Starting roster (prototype):

| Character | Role | Attack | Support | Rage |
|---|---|---|---|---|
| Bogatyr (Knight) | tank | Sword Strike | Guard: an ally passes 50% of its damage to the Bogatyr, plus a Shield | Bastion: Shield for the party, Taunt |
| Priestess | healer | Ray | Prayer: heal and cleanse | Grace: heal and cleanse the party |
| Volkhv of Svarog (Fire Mage) | mage | Fireball + Burn | Flaming Weapon on an ally | Fire Rain on all enemies |
| Witch of Marzanna (Ice Mage) | mage | Ice Bolt + Freeze | Ice Shield | Blizzard: Freeze all enemies |
| Forest Archer | DPS | Piercing Shot | Target Lock: the next attack applies Mark | Arrow Hail + Mark on all enemies |
| Minstrel (Bard) | support | Dissonance, chance to Stun | Song of Courage: +25% party damage | Hymn: Empower, heal and cleanse |

The remaining classes from the first version of the GDD (Paladin, Berserker, Druid, Alchemist, Necromancer, Rogue, Monk, Engineer, Oracle) are waiting for their Slavic versions.

Character progression: every character has its own level (cap 10).

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
| Attack | a white ring shrinks onto a gold circle; click or Space | +25% damage | +50% damage and extra Rage |
| Block | a BLOCK! flash just before the hit; clicking before the flash is wasted | −30% damage | −60% damage and extra Rage |

The enemy wind-up has a random length, so the rhythm cannot be memorised. The auto-timing accessibility option always gives "good", never "perfect". Some effects only work with perfect timing (for example an extra Burn stack or a guaranteed Stun).

### Party Rage

A shared meter filled by hits dealt and taken and by perfect timings. A full meter lets one character use its special action. Rage does not use up that character's move, affects the whole side and has no timing ring.

### Statuses

| Status | Effect |
|---|---|
| Bleed | damage every turn for 3 turns |
| Burn | damage every turn, stacks up to 5 |
| Freeze | +30% damage taken, the enemy acts last |
| Curse | −30% damage dealt |
| Shield | absorbs damage up to its value |
| Taunt | enemies must attack this character |
| Stun | skips the turn |
| Mark | +50% critical hit chance (×1.5) |
| Empower | +25% damage dealt |

## Enemies and bosses

Enemies fall into archetypes, and the player learns to read a wave's composition and pick counters. Some monsters that are male in folklore get female versions; those that are already female (Lady Midday, the Wild Woman, the Strzyga) stay as they are.

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
| Rye Fields | Bandit Woman, She-Werewolf, Field Maiden, She-Devil | Lady Midday: calls reinforcements and asks riddles (special QTE) |
| Marshes | Drowned Maiden, Will-o'-the-Wisp, Bog Hag, Drowned Girl | Water Maiden: drags a hero underwater for 2 turns |
| Old Forest | Wild Woman, Mamuna, Spider Woman, Werewolf Mother | Lady Leshy: a girl spirit of the forest; summons roots and shuffles the party's formation |
| Bald Mountain | Sabbath Witch, Storm Maiden, Giantess, Harpy | Dragoness: announces a great fire strike, teaches the perfect block |
| Nav | Wraith, Strzyga, Nightmare, Death | Koschei / Koscheia the Deathless: revives until the needle is destroyed |

## World structure and progression

> **To be rewritten.** This section is agreed as a draft and will be reworked.

Progression is pseudo-linear: every land must be completed, but the story interleaves them in different orders, and harder levels can be forced early. Each land has 4 circles with overlapping threat levels (1–10), so at any strength the player has 2–3 sensible places to choose from.

| Land | Circle I | Circle II | Circle III | Circle IV (boss) |
|---|---|---|---|---|
| Rye Fields | 1 | 2 | 3 | 4 |
| Marshes | 2 | 3 | 5 | 6 |
| Old Forest | 3 | 4 | 6 | 7 |
| Bald Mountain | 5 | 6 | 8 | 9 |
| Nav | 7 | 8 | 9 | 10 |

Every circle has a story mission (hand-made, with stars), a challenge with an imposed rule and a unique reward, and expeditions generated from the enemy pool (repeatable, the main source of materials). Circle IV also has the boss.

Story chapters open circles in several lands; a chapter's missions can be done in any order.

| Chapter | Threat | Missions | Reward |
|---|---|---|---|
| 1. Burnt Harvest | 1–2 | Fields I, Fields II | opens the Marshes |
| 2. Mist over the Bog | 2–4 | Marshes I, Forest I, Fields III → Lady Midday | the chest |
| 3. Whisper of the Forest | 3–6 | Marshes II, Forest II, Marshes III → Water Maiden | the hare |
| 4. Wrath of the Woods | 5–7 | Bald Mountain I, Forest III, Bald Mountain II → Lady Leshy | the duck |
| 5. Storm over the Peak | 7–9 | Nav I, Bald Mountain III, Nav II → Dragoness | the egg |
| Finale: Beyond the Smorodina | 9–10 | Nav III → Koschei | the needle |

A land's **Law** is a combat rule that is only hinted at in circle I and works in full in circle IV:

| Land | Law | Who shines | Materials |
|---|---|---|---|
| Rye Fields | Noon Heat: every 3rd turn, characters without a Shield lose part of their HP | Shields, tempo | iron, flax |
| Marshes | Depths: some enemies submerge; wisps switch targets | area attacks, cleansing | amber, herbs |
| Old Forest | Misleading: the party's formation shuffles; enemies stay hidden until Marked | Mark, control | wood, resin |
| Bald Mountain | Storm: lightning strikes a random character; dodge it in a QTE | block, cleansing | ore, thunderstone |
| Nav | Chill of Nav: healing is halved; the fallen rise as ghosts unless finished with fire | fast damage, fire | Nav essence |

Progress and forcing:
- Party strength = party level + equipment + heroes. Before a battle the player sees a threat rating relative to their party (Easy, Even, Dangerous, Madness), not a power number.
- Every open circle can be attempted right away. Winning above your strength grants **Daring**: +50% rewards.

**Rumours from the Road** (a board in the tavern) encourage detours from the story:
- 3 rumours are posted at a time; new ones appear after every story mission, with no time pressure.
- A rumour points to a level in a neglected land, has a modifier (for example no healer) and a reward visible up front.
- Most of the roster (for example 9 of 15 characters) joins only through rumours.
- After the MVP: the Wise Woman's Book (a bestiary unlocked by fighting, with boss weaknesses) and Land Favour (a merchant and a blessing that weakens the Law).

## Equipment, forge and economy

Power grows mainly through equipment, while new heroes bring new tactical options rather than raw strength.

**Equipment:** every character has 2 slots, a weapon (attack) and an artifact (health). Weapons are made for a specific role; artifacts are universal and fit everyone. Items from the same set give a bonus (for example "Frost Set": attacks on Frozen enemies Stun); every land has 1–2 sets.

| Rarity | Special trait | Source |
|---|---|---|
| Common | none | expeditions, forge |
| Rare | 1 weak trait | expeditions, forge |
| Epic | 1 strong trait | bosses, forge with rare materials |
| Legendary | a play-style-changing trait + a set piece | bosses, rumours |

**Forge:** the player picks a recipe, puts in materials and plays a forging minigame. *TODO*

**Potions** are brewed by the Wise Woman (healing, cleansing, Rage) and are used in battle instead of a character's action. *TODO*

| Currency | Source | Spent on |
|---|---|---|
| Gold | battles, selling items | forge, potions, merchants |
| Land materials | expeditions in a specific land | recipes |

## Art direction and platform

The style is dark cartoon with good-looking characters: a thick hand-drawn outline, heavy cool shadows, a single warm light, a painterly texture, and folklore instead of generic fantasy.
- **Perspective:** the camp and the land map in a light top-down view (3/4); combat in side view; heroes face right, enemies face left.
- **Animation:** skeletal (Unity 3D Animation); characters are split into layers instead of drawing frames.
- **Audio:** a clear perfect-timing sound is part of the mechanics; music and voices come later.
- **Interface:** shape and sound, not only colour (accessible for colour-blind players); fast-forward of animations ×2 and ×3.

**Platform:** we design mobile-first in landscape (16:9, touch and mouse share one pointer), and Steam is the second platform on the same build. Engine: Unity 6000.6.2f1 (the exact version is pinned in `CLAUDE.md`), URP, Input System.

**Business model** (recommendation, to be decided after testing): free-to-play.
