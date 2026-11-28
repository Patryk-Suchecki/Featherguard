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

## Textures

Textures are baked from the sculpt.

- **Flat colours per region:** each part (fur, skin, horns, claws, eyes) gets one base colour from its palette; the baked ambient occlusion and cavity add the shading on top, lightly.
- **Value before colour:** the face, hands and weapon are the lightest or most contrasting areas, so the eye goes there first.
- **Accents on a few shapes only:** for example the Bies' amber eyes and mouth, rusty mane tips and red horn stripes.
- **Shape over detail:** detail comes from the sculpted forms, not from fine texture. No photo textures, no specular highlights, no metal sheen.
- **Glow** (eyes, embers, magic) is the lightest, most saturated colour of the character; the toon shader does not add bloom on its own.

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

### Marshes

Dusk over black water; cold pale light and drowned wisps. Concept and theme: [`lands/marshes.md`](design/lands/marshes.md).

| Use | Colour |
|---|---|
| Black water | `#1E2426` |
| Mud | `#3B3A2C` |
| Reeds and moss | `#55613A` |
| Pale grass | `#7D8460` |
| Dusk sky, distance | `#6C6A86` |
| Wisps (the only saturated colour) | `#8FE0C8` |
| Key light | `#B8C2D0` |

### Bald Mountain

A stormy summit at night; cold lightning and a far dragon fire. Concept and theme: [`lands/bald-mountain.md`](design/lands/bald-mountain.md).

| Use | Colour |
|---|---|
| Dark basalt | `#24232A` |
| Grey rock | `#5C5A63` |
| Pale rock edges | `#8C8A90` |
| Storm sky | `#3A3F55` |
| Lightning | `#D8E4FF` |
| Dragon fire (the only warm colour) | `#E8783A` |
| Key light | `#CBD6F0` |

### Old Forest

Ancient trees at night; moonlight and a witch light in the dark. Concept and theme: [`lands/old-forest.md`](design/lands/old-forest.md).

| Use | Colour |
|---|---|
| Dark bark | `#262019` |
| Roots and earth | `#3F3426` |
| Moss | `#4E5E33` |
| Pale mushrooms | `#B9B08F` |
| Moonlight | `#A9B7C9` |
| Witch light (the only saturated colour) | `#7CE070` |
| Key light | `#B4C2D6` |

### Nav

The land of the dead in grey twilight; ash, bone and fog. Concept and theme: [`lands/nav.md`](design/lands/nav.md).

| Use | Colour |
|---|---|
| Dark ash | `#2A2A2E` |
| Ash ground | `#4A4A4D` |
| Bone white | `#CFCBC0` |
| Cold fog | `#8A93A0` |
| River | `#1F2A2E` |
| Ghost light (the only saturated colour) | `#B8E8FF` |
| Key light | `#C9CED6` |

### World map

One continuous valley seen from high above at night, like a painted landscape model: the Stronghold on a low hill in the centre, the Rye Fields front left, the Marshes front right, Bald Mountain back left, the Old Forest back right, and Nav beyond the Smorodina and the Kalinov Bridge at the back. The regions flow into each other through natural edges (fields thinning into wet grass, slopes rising into rock), never as separate tiles. Locked lands lie under thick mist. Each region uses its land's palette, darkened for night; the campfire and the Rye Fields lantern are the only warm lights. Concept: `Art/Concepts/WorldMap.png`.

The map's landmarks are small models placed on the terrain (#150); their sheet is `Art/Concepts/WorldMap/MapLandmarks.png`, with each piece in a three-quarter view (top row) and a front view (bottom row):
- the Stronghold: the oak, its palisade ring and the campfire;
- the Rye Fields: three stooks, a scarecrow, a roadside cross and a lantern post;
- the Marshes: a pool with reeds and a jetty;
- Bald Mountain: the peak and its rocks; the dragon fire on top is an effect;
- the Old Forest: one merged clump of dead trees; the green glow is an effect;
- the Kalinov Bridge: the bridge only, since the river is part of the terrain;
- Nav: a few graves and roofed crosses; the ghost light is an effect.

The round bases on the sheet are not modelled: every piece stands straight on the map terrain.

### Battle preparation

A muddy road through an empty field at night before a fight: the heroes' side on the left lit by a lantern on a post with a blank signpost, the enemies' side further along the road on the right in a cold violet glow from the ground, a wisp between them, dead trees, graves and ground mist. The warm lantern against the cold enemy side is the only colour contrast. Concept: `Art/Concepts/Preparation.png`.

### Sacred Oak Stronghold

Night around old oak, logs and stone; warm firelight against cold mist and a cool sky. The concept is [`Art/Concepts/Stronghold.png`](../Art/Concepts/Stronghold.png).

- **View:** at eye level from the road by the signpost, looking across the campfire at the buildings, with the forest and the mountains behind them; tapping the campfire flies the camera to it.
- **Layout:** the campfire in the centre with six stump seats behind it; the forge back left (a log smithy on a stone foundation with a stone chimney and an open, glowing door); the Wise Woman's Hut back centre (crooked, mossy, herbs and skulls, green glow); the Sacred Oak back right, the tallest shape, with a shrine and candles at its roots; the tavern right (log walls, thatched roof, lit windows); the shop front left (a stall with an awning and crates); the signpost front right where the road leaves the camp. A broken palisade of sharpened logs runs around it.
- **Mood:** ground mist, dead leafless trees at the edges, a graveyard front left with wooden markers, crosses, bones and blue-green wisps, lanterns on posts along the paths.
- **Light:** the campfire, the forge hearth and the tavern windows are the only warm light; the hut's glow and the wisps are the only cold accents.
- **For the models:** follow the layout and shapes, not the painting. Keep the outlines and toon bands of the style, leave out the painted letters on the forge wall, and keep the empty space in front of the fire for the party.

The Stronghold is built from these models. Each concept sheet is in `Art/Concepts/Stronghold/`; the labels and dimension lines on the sheets are only notes and are not modelled.

| Model | Concept | Size | Notes for the model |
|---|---|---|---|
| Forge | [Forge](../Art/Concepts/Stronghold/Forge.png) | 3.2 × 2.6 m, 4.4 m tall | the top row and the three-quarter view are the reference; the second front view (doors wide open) is an alternative. The chimney sits in a different place in each view: follow the three-quarter view. Leave out the painted letters on the wall. The anvil on its block and the stone trough are separate props |
| Wise Woman's Hut | [WiseWomansHut](../Art/Concepts/Stronghold/WiseWomansHut.png) | 2.8 × 2.4 m, 3.5 m tall | the top row is the reference; the second row repeats it with the door the other way. The sheet draws it straight: lean and sag it a little, as in the main concept. Herbs, garlic and skulls under the eaves, pots by the door, the green glow from the door |
| Sacred Oak | [SacredOak](../Art/Concepts/Stronghold/SacredOak.png) | about 9 m tall | the front and three-quarter views are the reference; the side view draws the crown narrower. The crown as a few big leaf clumps; the shrine with candles at the front roots and the ribbons are separate parts |
| Tavern | [Tavern](../Art/Concepts/Stronghold/Tavern.png) | 4.5 × 3.5 m, 4.5 m tall | mossy thatch, carved horse heads on the gables, porch over the door, the sign has no letters; barrels and benches as separate props |
| Shop | [Shop](../Art/Concepts/Stronghold/Shop.png) | 2.5 × 1.5 m, 2.5 m tall | the front view is the reference; the side view draws the awning differently, so model it sagging between the poles as in the front and top views |
| Campfire and stump seat | [Campfire](../Art/Concepts/Stronghold/Campfire.png) | ring 1.4 m across; seat 0.5 m wide, 0.45 m tall | logs and embers only, the flame is an effect; one stump model, used six times |
| Signpost | [Signpost](../Art/Concepts/Stronghold/Signpost.png) | 2.5 m tall | three arrow boards with carved symbols and no letters, rope and nails, a stone pile at the foot |
| Palisade | [Palisade](../Art/Concepts/Stronghold/Palisade.png) | segment 2 m long, 2.2 m tall | three pieces that tile: straight segment, broken segment, end post with a carved band; the same height and log thickness |
| Lantern post | [LanternPost](../Art/Concepts/Stronghold/LanternPost.png) | 2.4 m tall | the side view repeats the front; use the front and three-quarter views. The lantern glass is the only warm part and gets a light in the scene |
| Graveyard kit | [Graveyard](../Art/Concepts/Stronghold/Graveyard.png) | 0.4–1.3 m | roofed wooden cross, plank marker, mossy headstone, grave mound, bones with a skull |
| Forge, ruined | [ForgeRuined](../Art/Concepts/Stronghold/ForgeRuined.png) | as the Forge | made from the Forge model, so the footprint, logs and chimney stay where they are; the sheet moves the chimney and keeps most of the roof, so use it only for the broken door, soot and rubble. The roof caved in, the chimney half fallen, no glow |
| Wise Woman's Hut, ruined | [WiseWomansHutRuined](../Art/Concepts/Stronghold/WiseWomansHutRuined.png) | as the hut | made from the hut model; the sheet keeps its shape well: the mossy roof broken open with rafters showing, a broken wall corner, fallen herbs and skulls, broken pots, no glow |
| Tavern, ruined | [TavernRuined](../Art/Concepts/Stronghold/TavernRuined.png) | as the Tavern | made from the Tavern model; the sheet draws a bigger building with a second gable, so take only the look of the damage: the thatch burnt through to bare rafters, a wall broken in, dark windows, the sign hanging |
| Shop, ruined | [ShopRuined](../Art/Concepts/Stronghold/ShopRuined.png) | as the Shop | made from the Shop model; the sheet's poles lean differently, so take the torn awning, the cracked empty counter and the smashed crates. Ignore the colour swatches on the sheet |
| Dead trees | [DeadTrees](../Art/Concepts/Stronghold/DeadTrees.png) | 4–7 m tall | three variants, taken from the first, third and fifth tree from the left: tall and thin, twisted and leaning, broken snag with moss; the other trees are their other views |
| Camp props | [CampProps](../Art/Concepts/Stronghold/CampProps.png) | 0.4–1 m | crate, stacked crates, barrel, sack, bucket, axe in a chopping block, pitchfork; the three rows are the front, side and back views |

The ground, roads, mist, fire and wisps are not models: the scene and its effects make them.


| Use | Colour |
|---|---|
| Deep shadow | `#2A211A` |
| Old oak wood | `#4A3627` |
| Moss and leaves | `#5E6B3A` |
| Stone | `#8A8780` |
| Night sky, distance | `#5B6A86` |
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

- **Same look as the world:** frames and buttons use the outline colour and flat fills; dark wood, bone and cloth instead of glossy panels.
- **Readable first:** large, high-contrast text on flat dark or light panels, never over busy art.
- **Never colour alone:** every state also has a shape, icon or sound (accessibility rule from the GDD).
- **Icons:** one clear silhouette with an outline, the same light direction as the characters.
- **Rarity frames:** square slots for items and materials, coloured by rarity (Common grey, Uncommon green, Rare blue, Epic purple, Legendary yellow) and told apart by their corner ornaments too: none, round studs, diamonds, diamonds with a top gem, stars with a crown.

## Checklist for a new character

- [ ] Silhouette reads at 64 px black; three carrying shapes named in its character sheet.
- [ ] Height fits its role in the table above.
- [ ] Base colours from its land's palette, accents on a few shapes only.
- [ ] Baked texture with flat colours per region and accents on a few shapes, no PBR look.
- [ ] Looks right with `Toon.mat` in battle next to the heroes.
