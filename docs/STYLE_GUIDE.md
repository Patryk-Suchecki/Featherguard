# Style Guide

How the game looks, in numbers and rules both of us follow. The art direction in [`GDD.md`](GDD.md) says what we aim for; this guide makes it concrete. Technical rules (scale, budgets, export) are in [`ART_PIPELINE.md`](ART_PIPELINE.md) and are not repeated here.

The style must stay our own: references are our own sketches, renders and the concept sheets in `Art/Concepts`, never another game's art.

References:
- `Art/Concepts/Bies.png`: the first character, the reference for proportions, fur, texture and poses.
- `Art/Concepts/Palettes.png`: the palettes below as swatches, one row per table.

## Silhouette and proportions

- **Read as a black shape first.** Every character must be recognisable as a solid black silhouette 64 px tall. Pick three shapes that carry it (for the Bies: ram horns, mane hump, long clawed arms) and exaggerate them.
- **Big, medium, small.** One big mass (body or mane), a few medium ones (head, limbs, weapon) and small details only in the texture.
- **Chunky and exaggerated:**
  - heroes about 3.5–4 heads tall;
  - hands, feet and weapons 1.3–1.5 times their natural size;
  - limbs thinner than the masses they join, so the shapes read at phone size.
- **Size by role**, next to a hero of 1.4 m:

| Role | Height | Read as |
|---|---|---|
| Hero | 1.4 m | the reference |
| Regular enemy | about 1.5 m, quadrupeds with the head at about 0.9 m | equal or a little bigger |
| Elite | about 2.0–2.3 m (the Bies is 2.26 m to the horn tips) | one and a half heroes |
| Boss | about 2.4 m or more | clearly the biggest in the fight |

- **Posture tells the role:** heroes stand upright and open; enemies hunch, lean forward or crouch.

## Fur, hair and cloth

- Big, hard-edged clumps that follow one flow direction, not strands.
- Jagged edges only where they build the silhouette (mane tips, tufts); everywhere else the clumps lie down.
- Cloth is thick with few, large folds.

## Shading and outline

All units use the `Featherguard/Toon` shader; its settings live in `Assets/Art/Materials/Toon.mat`. The agreed values:

| Setting | Value | Why |
|---|---|---|
| Bands | 3 | light, half-tone and shadow; 2 looks flat, 4 looks soft |
| Light Threshold | 0 | half of each form in light |
| Shadow Color | `#6B759E` | cool shadows against the warm key light |
| Outline Color | `#120D0F` | near-black with a warm tint, never pure black |
| Outline Width | 0.35 % of screen height | thick enough to read on a phone, thin enough not to fill small details |

- **One warm key light** from above and in front, slightly from the heroes' side; no rim lights or coloured fill lights on characters.
- **Shadows are a colour, not black:** the shadow band keeps the texture's hue and turns it cooler.
- Characters and props use the toon shader. Backgrounds may be softer, but never sharper or more detailed than the characters.

## Texture painting

- **Three tones per material:** shadow, base and light. Paint the light from above and the front; darken towards the back, the underside and the ends of the limbs.
- **Value before colour:** the face, hands and weapon are the lightest or most contrasting areas, so the eye goes there first.
- **Accents on a few shapes only:** for example the Bies' amber eyes and mouth, rusty mane tips and red horn stripes.
- **Visible brushwork:** colour shifts and broad strokes instead of fine detail. No photo textures, no painted specular highlights, no metal sheen.
- **Glow** (eyes, embers, magic) is painted as the lightest, most saturated colour of the character; the toon shader does not add bloom on its own.

## Colour

- **Hierarchy:** backgrounds are dark to mid and desaturated; characters are more saturated; effects and UI highlights are the most saturated things on screen.
- **Heroes and enemies:** heroes carry warm, light accents. Enemies sit on dark, cold or neutral bases; their accents may glow (the Bies' embers), but their overall value stays darker than the heroes'.
- **Each land has its own limited palette.** Characters from that land take their base colours from it; the land's light colour is the key light colour.

### Rye Fields

Noon over muted rye; warm but faded light.

| Use | Colour |
|---|---|
| Deep shadow, soil in shade | `#2F2A22` |
| Earth, paths, field edges | `#6E6152` |
| Rye, muted olive-ochre | `#A39A5B` |
| Sunlit rye | `#C9B77A` |
| Hazy noon sky, distance | `#9FAEA8` |
| Key light | `#F2DFB8` |

### Sacred Oak Stronghold

Dusk around old oak and stone; warm firelight against a cool sky.

| Use | Colour |
|---|---|
| Deep shadow | `#2A211A` |
| Old oak wood | `#4A3627` |
| Moss and leaves | `#5E6B3A` |
| Stone | `#8A8780` |
| Dusk sky, distance | `#5B6A86` |
| Firelight, key light | `#F0B060` |

### Shared

| Use | Colour |
|---|---|
| Outline | `#120D0F` |
| Shadow tint | `#6B759E` |
| Warm neutral light (menus, portraits) | `#F5E4C8` |

The battle scene still uses the bright prototype lighting and sky; matching it to the land palettes is part of the Rye Fields backdrop work.

## Effects (VFX)

- **Bold flat shapes, not realistic particles:** slashes, shards, swirls and stylised flames with a dark outline or a dark core and a light edge. No smoke volumes, no soft glowing fog.
- **Few frames, big changes:** an effect reads in 2–4 strong shapes over its life.
- **Timing:** a short anticipation glow, the impact exactly on `OnHitFrame`, then a fast fade of 0.2–0.4 s.
- **Size:** an impact effect is at most about the size of the target's upper body; a Rage action may fill the target's side of the screen.
- **Colour by element**, always the most saturated colour on screen while it plays:

| Element | Colour |
|---|---|
| Fire, embers | `#FFB02E` with `#B5541F` |
| Ice | `#A8E6F0` |
| Dark, curse | `#8E5CC2` |
| Healing, blessing | `#B8D45A` |
| Blood, Bleed | `#8C1C1C` |

## Interface

- **Same look as the world:** frames and buttons use the outline colour, flat fills and painted texture; dark wood, bone and cloth instead of glossy panels.
- **Readable first:** large, high-contrast text on flat dark or light panels, never over busy art.
- **Never colour alone:** every state also has a shape, icon or sound (accessibility rule from the GDD).
- **Icons:** one clear silhouette with an outline, the same light direction as the characters.

## Checklist for a new character

- [ ] Silhouette reads at 64 px black; three carrying shapes named in its character sheet.
- [ ] Height fits its role in the table above.
- [ ] Base colours from its land's palette, accents on a few shapes only.
- [ ] Three-tone painted texture, light from above and the front, no PBR look.
- [ ] Looks right with `Toon.mat` in battle next to the heroes.
