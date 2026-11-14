# Wolf

A regular Rye Fields enemy and the Bies' pack, and the first quadruped made with the art pipeline. Abilities describe the target design; numbers come with balancing. Technical rules are in [`ART_PIPELINE.md`](../../ART_PIPELINE.md), the look rules in [`STYLE_GUIDE.md`](../../STYLE_GUIDE.md); the Bies is described in [`Bies.md`](Bies.md).

## Role

- **Side:** enemy, female (a she-wolf; the pack is mixed, the model is one).
- **Land:** Rye Fields.
- **Archetype or role:** Slasher: she hits one hero hard and goes for the weakest. Alone she is a nuisance; in a pair with the Bies she turns his Mark into a kill.
- **Encounters:** two Wolves with the Bies; packs of two or three Wolves, or a Wolf with the Bandit Woman, in ordinary Rye Fields battles.
- **With the Bies:** the Wolves always act after him in the enemy turn, so Sic the Pack pays off in the same turn. When he dies they fight on unchanged.
- **Plain and possessed:** the player meets plain field Wolves first; the possessed ones, with the Bies' embers, fight at his side. Both use the same model and rules; the plain Wolf has an albedo without the ember marks and with dull yellow eyes.

## Concept and tone

In Slavic folklore the wolf is the beast of the borderland between the village and the wild, a servant of the forest and field spirits and a common shape of the werewolf. Our Wolves are lean field wolves from the edges of the rye that the Bies has taken as his own: they move like hungry animals, but his embers burn in their eyes and along their spines. They should read as dangerous dogs of a demon, not as noble wild animals.

## Look

- **Silhouette and body:** a lean, long-legged wolf in a low stalking stance, head held below the shoulders, back sloping down to the hips. Long snout, a ragged ruff around the neck and shoulders, a bushy tail held low. Digitigrade legs with large paws.
- **Size:** the head at about 0.9 m (the style guide's quadruped height), about 1.6 m from snout to tail tip, so she reaches the Bies' waist.
- **Key features:** the three shapes that carry the silhouette are the tall pointed ears, the ragged ruff and the low bushy tail. Also a long snout with visible fangs, sunken flanks with ribs showing under the fur, and large paws with dark claws. No collar or gear.
- **Colours** (the Rye Fields palette with the Bies' embers):

| Part | Colour |
|---|---|
| Back and tail | dark earth `#33271F` |
| Flanks and legs | dusty grey-brown `#5E4A3B` |
| Belly, inner legs, muzzle | faded rye `#8A6C4C` |
| Eyes | glowing amber `#FFB02E` |
| Possession marks along the spine and shoulders | ember cracks `#B5541F` |
| Claws, nose, lips | near black `#140D0B` |
| Inside of the mouth | dark red `#4A1F1A` |

  Her overall value stays darker than the heroes'; the eyes and marks are the only saturated colours, shared with the Bies so the player sees whom she serves. The plain Wolf swaps them for dull yellow eyes `#B59A3A` and no marks; the marks stay a separate texture layer in the `.blend`, so both albedos come from one bake.
- **Concept sheet:** `Art/Concepts/Wolf.png`: side, front and back on one ground line in a neutral standing pose, plus the stalking pose.

## Abilities

| Ability | Kind | What it does | Clip | VFX | Sound |
|---|---|---|---|---|---|
| Bite | Attack | A lunging bite at one hero; she picks wounded heroes first (Slasher). With the Bies, Sic the Pack makes both Wolves bite the Marked hero. Melee: the game moves her to the target. | `Attack` | Wind-up: the ember marks flare. Hit: a short bite flash with a few sparks. | Wind-up: a low snarl. Hit: a snapping bite with a wet crunch. |
| Pack Howl | Special | Every 3rd turn when no Bies is in the fight: the Wolves get **Empower** for their next attack. With the Bies she answers his Sic the Pack with the same clip instead. Instant, at her own position. | `Cast` | The marks flare along the spine, a small heat-haze ring around the head. | A rising howl; with the Bies, a short answering howl. |

**Until the target rules exist** (Sic the Pack, enemy archetypes, #82), the Wolf uses placeholders in the current build: Bite deals melee damage, and Pack Howl applies Empower to all Wolves.

**Starting numbers** (first guesses for balancing; the Bies has 260 HP and a 24 damage claw):

| Value | Start |
|---|---|
| HP | 70 |
| Bite | 14 damage, melee |
| Pack Howl | Empower (+25% damage) on all Wolves for their next attack, every 3rd turn without the Bies |

## Animations

Snappy and pose-to-pose; every clip plays in place and the game moves the unit. Frames at 30 fps. Enemies face left, so the battle camera sees her right side.

| Clip | What happens | `OnHitFrame` |
|---|---|---|
| `Idle` (loops, 48 frames, 1.6 s) | Low stalking stance; heavy breathing moves the ribs and ruff, the head sways and sniffs, the ears twitch, the tail flicks low. | – |
| `AttackWindup` (loops, 12 frames, 0.4 s) | A crouch held on bent hind legs, lips curled, a growl that shakes the ruff, so the game can stretch the wind-up before the block. | – |
| `Attack` (26 frames, 0.87 s) | Crouch, a fast lunge with the head forward and jaws open, the bite, a quick shake of the head, back to the stance. | frame 10, the bite's contact |
| `Cast` (40 frames, 1.33 s) | She sits back on her haunches, raises the head and howls, then drops back to the stance. | frame 20, the peak of the howl |
| `Hit` (12 frames, 0.4 s) | A short yelp and recoil, the head jerks away, the ears flatten, then the snarl returns. | – |
| `Death` (40 frames, 1.33 s) | Her legs give way and she falls on her side; a short twitch, the marks fade, the last frame holds. | – |
| `Victory` (loops, 40 frames, 1.33 s) | She raises the head and gives a short howl, the tail swishes. | – |

## Sounds

| Moment | VFX | Sound |
|---|---|---|
| Hit | A puff of fur, a short flash on the model. | A sharp yelp turning into a snarl. |
| Death | The ember marks go out from the tail to the head, the eyes last. | A whimper and a dull fall. |
| Victory | The marks flare once. | A short howl. |

## Asset checklist

- [ ] Character sheet approved (this sheet, #86)
- [ ] Concept sheet (#40)
- [x] Model (#40)
- [x] Texture (#40, baked from the sculpt, 1024 × 1024): possessed albedo
- [ ] Plain albedo (no marks, dull eyes)
- [x] Rig (#41, the first quadruped rig; the art pipeline gains a quadruped section)
- [x] `Idle`
- [x] `AttackWindup`
- [x] `Attack`
- [x] `Cast`
- [x] `Hit`
- [x] `Death`
- [x] `Victory`
- [x] Exported to `Assets/Art/Characters`
- [ ] Portrait
- [ ] VFX: marks flare, bite flash, howl ring, hit fur puff, death fade
- [ ] Sounds: snarl, bite, howl, answering howl, yelp, whimper and fall
- [ ] In the game (`UnitDefinition` for both Wolves and the encounter with the Bies, #42)

## Open questions

- **Albedo swap:** the battle presentation needs a way to give one model a second albedo (for the plain Wolf); this comes with #42.
- **Pack size:** are three Wolves at once too much against a party of three before the player has a Shield?
