# Bies

Character sheet for the first elite enemy. Abilities are a design proposal; numbers come later with balancing. Technical scale, naming and export rules belong to the art pipeline spec (issue #35), not to this sheet.

## Role and archetype

- **Land:** Rye Fields.
- **Archetype:** Elite, 2 actions per turn. The counter is the perfect block.
- **First encounter:** the Bies with two Wolves.
- **Role in the fight:** master of the pack. He hits hard himself and drives his possessed Wolves onto one hero.
- **Gender:** male.
- **Facing in battle:** left, like every enemy.

## Concept and tone

In Slavic folklore a *bies* is an old, unnamed evil spirit of the wild, older than the later Christian devil. He brings frenzy and madness; the verb "to bies" still means to rage. He lurks at crossroads and on the edges of fields and possesses people and animals. Later beliefs give him horns, fur and hooves, but there is no single canonical look, so our design is our own.

Our Bies is a towering, hunched goat demon that has taken a wolf pack as his own. He looks purely frightening. His character shows in how he moves and sounds: he grins, mocks the heroes when they hit him and laughs when they fall. This is grim with a playful edge, in line with the dark storybook tone of the GDD.

## Look

### Silhouette and size

- A towering, hunched biped. The head sits forward of the shoulders.
- About **1.5 times the height of a hero** to the top of the horns. The Wolves reach roughly to his waist.
- Long arms hang to the knees.
- A huge shaggy mane on the neck and shoulders and shaggy arms and thighs. The fur makes up about half of the silhouette, and the waist is slim by contrast.
- Digitigrade goat legs bent backwards.
- As a black silhouette at phone size, three shapes carry it: the ram horns, the mane hump and the long clawed arms.
- Low-poly build: fur is a set of big, hard-edged clumps, not strands. Fine detail (hair, stripes, small teeth) goes into the painted texture.

### Key features

- **Head:** goat head with a long snout, an open jaw with big fangs, overhanging brows over deep eye sockets, leaf-shaped ears and a chin tuft.
- **Horns:** big ram horns curling back and around towards the cheeks.
- **Hands:** four fingers with long, curved, hooked claws.
- **Legs:** cloven hooves with a fur tuft above them.
- **Tail:** a short tufted tail.
- **Clothing:** none, only fur.

### Colours

The ember palette:

| Part | Colour |
|---|---|
| Fur | dark brown fading to black |
| Mane tips | rusty orange |
| Eyes, mouth | glowing amber |
| Horns | bone with red stripes |
| Hooves, claws | dark bone |

The possessed Wolves share his amber eyes and glowing marks, so the player sees at a glance whom they serve.

Proposed Rye Fields palette (not agreed for the land yet), all kept muted so the characters stand out:

- muted olive-ochre rye;
- grey-brown earth;
- a hazy grey-green, bluish noon sky;
- warm but faded light.

## Abilities

Turn rhythm: he always attacks twice. Every 3rd turn **Sic the Pack** replaces his second attack.

### Rending Claws (attack)

- **Effect:** a wide claw swipe at one hero that applies **Bleed**. He is the first unit in the game to apply Bleed.
- **Delivery:** melee. The game hops him to the target before the clip plays.
- **Clip:** `Attack`.
- **VFX:**
  - Wind-up: an amber flash in the eyes and an ember trail behind the claws.
  - Hit: three bright orange claw-shaped slash lines on the target with sparks.
  - Bleed: dark red drops falling down.
- **Sound:**
  - Wind-up: a low growl and a whoosh of the arm.
  - Hit: a short, sharp wet tear with a dry crack.
  - Bleed: a quiet drip.

### Sic the Pack (special)

- **Effect:**
  - He puts **Mark** on one hero.
  - Both Wolves get **Empower** and attack the Marked hero this turn.
- **Player response:** the threat is announced, so the player can answer with a Shield, Guard or Taunt.
- **Delivery:** instant, at his own position.
- **Clip:** `Cast`.
- **VFX:**
  - The howl: a heat-haze wave with sparks around the head, and the mane glows brighter.
  - Fists hitting the ground: a dust and spark ring.
  - A sharp, claw-shaped amber target sigil lights up over the Marked hero.
  - The Wolves' eyes and possession marks ignite.
- **Sound:**
  - A long, hoarse howl with a goat bleat underneath.
  - A dull ground thud.
  - A sharp ringing "ting" when the sigil appears.
  - A short howl from both Wolves in answer.

## Animations

The style is snappy and pose-to-pose. Every clip plays in place; battle movement is done by the game. Damage and effects show on the `OnHitFrame` animation event.

| Clip | Description | Length | `OnHitFrame` |
|---|---|---|---|
| `Idle` (loop) | Hunched, heavy breathing lifts the shoulders and mane. Clawed fingers slowly flex, an ear twitches, the tail flicks. Now and then he tilts his head like a curious goat and the jaw opens into a grin. | ~2–3 s | – |
| `Attack` | Anticipation: he leans back and raises the right arm high above the head, a clear pose the player waits on to block. Strike: a fast diagonal downward slash through the target with a lunge. Follow-through: the arm swings past the body, a short hold, back to Idle. | ~1 s | When the claws cross the target's centre, about 45–50% in, right after the fastest frame of the slash. |
| `Cast` | He straightens to full height, chest out, throws his head back and howls. Then he slams both fists into the ground and points a claw at the hero ("take him!"). | ~1.5 s | On the fists hitting the ground: Mark appears, the Wolves get Empower. |
| `Hit` | A short jerk of the head and torso backwards, the mane bristles, a snarl, then a quick angry grin and back to pose. | ~0.4 s | – |
| `Death` | He staggers, clutches his chest, falls to his knees, then face forward. The ember glow in the mane fades from the head down and the eyes go out last. The final frame holds; the body stays on the ground. | ~1.5–2 s | – |
| `Victory` | He straightens, throws his head back and laughs mockingly, shoulders shaking. | ~2 s, may loop | – |

Reaction effects:

| Moment | VFX | Sound |
|---|---|---|
| Hit | A puff of fur and a few sparks from the mane, a short flash on the model. | An angry snort or goat bleat, then a stifled chuckle. |
| Death | The mane's glow dies out top to bottom, a wisp of smoke rises from the fur, the eyes go out last. | A rattle fading into a dying bleat, a dull fall, then a quiet hiss as the embers go out. |
| Victory | The mane flares brighter for a moment. | A hoarse, mocking laugh with a bleat underneath. |

## Asset checklist

- [ ] Concept sheet: front, side, back and colour variant in the ember palette. An uncoloured reference turnaround exists outside the repo.
- [ ] Model: low poly, about 4,000–6,000 triangles as a working target until #35 sets the budget (#13). A first low-poly draft (about 5,500 triangles, unwrapped) exists outside the repo and needs a manual pass.
- [ ] Painted texture in the ember palette (#13).
- [ ] Rig (#14).
- [ ] Clip `Idle` (#14).
- [ ] Clip `Attack` with `OnHitFrame` (#14).
- [ ] Clip `Cast` with `OnHitFrame` (#14).
- [ ] Clip `Hit` (#14).
- [ ] Clip `Death` (#14).
- [ ] Clip `Victory` (#14).
- [ ] Portrait.
- [ ] VFX (#43):
  - [ ] ember claw trail;
  - [ ] claw slash;
  - [ ] Bleed drops;
  - [ ] heat-haze howl;
  - [ ] ground slam ring;
  - [ ] Mark sigil;
  - [ ] Wolf possession glow;
  - [ ] hit fur puff;
  - [ ] death glow fade and smoke.
- [ ] SFX:
  - [ ] growl;
  - [ ] whoosh;
  - [ ] claw tear;
  - [ ] Bleed drip;
  - [ ] howl;
  - [ ] ground thud;
  - [ ] Mark ting;
  - [ ] Wolf answer howl;
  - [ ] hit snort and chuckle;
  - [ ] death rattle and fall;
  - [ ] ember hiss;
  - [ ] victory laugh.
- [ ] Into battle (#15).

## Open questions

- **Blood Call:** should he become Empowered when a Wolf dies? This is a possible passive that would make kill order matter more.
- **Wolves after his death:** do they weaken or flee when the Bies dies?
- **Mark target:** how does he choose whom to Mark: lowest HP, the healer, or at random?
- **Wolf turn order:** do the Wolves always act after him in the enemy turn, so Sic the Pack pays off in the same turn?
- **Second attack:** can it target a different hero from the first?
- **The body after death:** what happens to the corpse when the next wave arrives?
- **Rye Fields palette:** to be agreed for the whole land.
- **Technical limits:** final scale, triangle budget and naming come from the art pipeline spec (#35).
- **Numbers:** HP, damage, Bleed strength.
