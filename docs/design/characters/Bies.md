# Bies

The first elite enemy and the first character made with the art pipeline. Abilities describe the target design; numbers come with balancing. Technical rules are in [`ART_PIPELINE.md`](../../ART_PIPELINE.md), the look rules in [`STYLE_GUIDE.md`](../../STYLE_GUIDE.md).

## Role

- **Side:** enemy, male.
- **Land:** Rye Fields.
- **Archetype or role:** Elite, 2 actions per turn; the counter is the perfect block. Master of the pack: he hits hard himself and drives his possessed Wolves onto one hero.
- **Encounters:** the Bies with two Wolves.

## Concept and tone

In Slavic folklore a *bies* is an old, unnamed evil spirit of the wild, older than the later Christian devil. He brings frenzy and madness, lurks at crossroads and on the edges of fields, and possesses people and animals. Later beliefs give him horns, fur and hooves, but there is no single canonical look, so our design is our own.

Our Bies is a towering, hunched goat demon that has taken a wolf pack as his own. He looks purely frightening; his character shows in how he moves and sounds: he grins, mocks the heroes when they hit him and laughs when they fall. Grim with a playful edge, in line with the dark storybook tone of the GDD.

## Look

- **Silhouette and body:** a towering, hunched biped with the head forward of the shoulders, long arms hanging to the knees, digitigrade goat legs and a slim waist under a huge shaggy mane. The fur makes up about half of the silhouette.
- **Size:** 2.26 m to the horn tips, about one and a half heroes. The Wolves reach roughly to his waist.
- **Key features:** the three shapes that carry the silhouette are the ram horns, the mane hump and the long clawed arms. Also a goat head with a long snout and big fangs, overhanging brows, leaf-shaped ears, a chin tuft, four-fingered hands with hooked claws, cloven hooves with fur tufts and a short tufted tail. No clothing.
- **Colours** (the ember palette):

| Part | Colour |
|---|---|
| Fur | dark brown `#2B1C17` fading to black `#140D0B` |
| Mane tips | rusty orange `#B5541F` |
| Eyes, mouth | glowing amber `#FFB02E` |
| Horns | bone `#C9B99A` with red stripes `#9C2A1F` |
| Hooves, claws | dark bone `#5A4C3C` |

  The possessed Wolves share his amber eyes and glowing marks, so the player sees at a glance whom they serve.
- **Concept sheet:** `Art/Concepts/Bies.png`.

## Abilities

Turn rhythm: he attacks twice per turn; every 3rd turn **Sic the Pack** replaces his second attack.

| Ability | Kind | What it does | Clip | VFX | Sound |
|---|---|---|---|---|---|
| Rending Claws | Attack | A wide claw swipe at one hero that applies **Bleed**; he is the first unit to apply it. Melee: the game hops him to the target. | `Attack` | Wind-up: amber flash in the eyes, ember trail behind the claws. Hit: three ember claw slashes with sparks. Bleed: dark red drops. | Wind-up: low growl and an arm whoosh. Hit: a short wet tear with a dry crack. Bleed: a quiet drip. |
| Sic the Pack | Special | Puts **Mark** on one hero; both Wolves get **Empower** and attack the Marked hero this turn. The threat is announced, so the player can answer with a Shield, Guard or Taunt. Instant, at his own position. | `Cast` | Howl: heat-haze wave with sparks, the mane glows brighter. Fists on the ground: dust and spark ring. A claw-shaped amber sigil over the Marked hero; the Wolves' eyes and marks ignite. | A long hoarse howl with a goat bleat underneath, a dull ground thud, a sharp "ting" for the sigil, a short answering howl from the Wolves. |

**In the current build** the target rules (Bleed, Mark, two actions, the Wolves) do not exist yet, so the Bies uses placeholders that drive the same clips: Rending Claws deals melee 24 (heavy) with the claw slash effect, and Howl applies Weaken to all heroes. He is fought in the `BiesTest` encounter (see `GAME_DESIGN.md`).

## Animations

Snappy and pose-to-pose; every clip plays in place and the game moves the unit. Frames at 30 fps, as exported.

| Clip | What happens | `OnHitFrame` |
|---|---|---|
| `Idle` (1–76, loops) | Hunched, heavy breathing lifts the shoulders and mane; the claws flex, an ear twitches, the tail flicks; now and then a curious goat head tilt and a grin. | – |
| `AttackWindup` (planned, loops) | The raised-claw hold from the start of `Attack`, breathing and trembling, so the game can stretch the wind-up to a random length and the block can't be learned. Made together with blocking (#18, #19). | – |
| `Attack` (1–31) | He leans back and raises the near arm high above the shoulder, a clear pose to block on; a fast diagonal downward slash with a lunge; the arm swings past the body, a short hold, back to Idle. | 15, when the claws cross the target |
| `Cast` (1–46) | He straightens, chest out, throws his head back and howls, slams both fists into the ground and points a claw at the hero. | 24, the fists hitting the ground |
| `Hit` (1–13) | A short jerk backwards, the mane bristles, a snarl, then a quick angry grin. | – |
| `Death` (1–55) | He staggers, clutches his chest, falls to his knees, then face forward; the last frame holds. | – |
| `Victory` (1–61, loops) | He straightens, throws his head back and laughs mockingly, shoulders shaking. | – |

## Sounds

| Moment | VFX | Sound |
|---|---|---|
| Hit | A puff of fur and a few sparks from the mane, a short flash on the model. | An angry snort or goat bleat, then a stifled chuckle. |
| Death | The mane's glow dies out from the head down, a wisp of smoke, the eyes go out last. | A rattle fading into a dying bleat, a dull fall, a quiet hiss as the embers go out. |
| Victory | The mane flares brighter for a moment. | A hoarse, mocking laugh with a bleat underneath. |

## Asset checklist

- [x] Character sheet approved (this sheet, #47)
- [x] Concept sheet (#53)
- [x] Model (#48)
- [x] Texture (#48, baked from the sculpt)
- [x] Rig (#48)
- [x] `Idle`
- [ ] `AttackWindup` (with blocking, #18, #19)
- [x] `Attack`
- [x] `Cast`
- [x] `Hit`
- [x] `Death`
- [x] `Victory`
- [x] Exported to `Assets/Art/Characters` (#48)
- [ ] Portrait
- [ ] VFX:
  - [x] claw slash (#55)
  - [ ] ember claw trail
  - [ ] Bleed drops
  - [ ] heat-haze howl
  - [ ] ground slam ring
  - [ ] Mark sigil
  - [ ] Wolf possession glow
  - [ ] hit fur puff
  - [ ] death glow fade and smoke
- [ ] Sounds (#56): growl, whoosh, claw tear, Bleed drip, howl, ground thud, Mark ting, Wolf answer howl, hit snort and chuckle, death rattle and fall, ember hiss, victory laugh
- [x] In the game (`UnitDefinition` and the `BiesTest` encounter, #49)

## Open questions

- **Blood Call:** should he become Empowered when a Wolf dies? A possible passive that would make kill order matter more.
- **Wolves after his death:** do they weaken or flee when the Bies dies?
- **Mark target:** how does he choose whom to Mark: lowest HP, the healer, or at random?
- **Wolf turn order:** do the Wolves always act after him, so Sic the Pack pays off in the same turn?
- **Second attack:** can it target a different hero from the first?
- **The body after death:** what happens to the corpse when the next wave arrives?
- **Numbers:** HP, damage, Bleed strength.
