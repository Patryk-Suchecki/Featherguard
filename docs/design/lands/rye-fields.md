# Rye Fields

How the fights in the Rye Fields play, the first land and the one in the vertical slice. A land file never decides the roster: which enemies appear here is set by the Bestiary in [`GDD.md`](../../GDD.md#bestiary), and their stages are in [`enemies.md`](../enemies.md). It never covers story missions or side activities either; those belong to [`world-progression.md`](../world-progression.md).

Only the theme is written so far; the other sections come with the Rye Fields spec.

## Theme

![Rye Fields battle backdrop concept](../../../Art/Concepts/RyeFields.png)

Concept: `Art/Concepts/RyeFields.png`.

- **Motif:** a trampled clearing among the rye at high noon, in a dark Slavic folk tale. Village life is near but nobody is in sight; the heat itself feels like it is watching, because this is the land of Lady Midday.
- **Layout:** the empty clearing of bare earth is the stage; heroes stand on the left, enemies on the right. Around it: tall muted rye in big clumps, a low wattle fence and a field balk with boundary stones, rye stooks, a broken cart wheel. Further back a scarecrow, a thatched wooden barn and a lone crooked pear tree with crows; a hazy line of trees on the horizon.
- **Folk details** that tie the land to its story: the wooden roadside cross with a small roofed shrine (left) and the sickle stuck in a stook, both hints at Lady Midday, who punishes those working at noon.
- **Palette and light:** the Rye Fields palette in [`STYLE_GUIDE.md`](../../STYLE_GUIDE.md#rye-fields): faded olive-ochre rye, dusty earth, a hazy grey-green noon sky and a warm, faded key light from above and slightly from the heroes' side. The backdrop stays darker and less saturated than the characters.
- **What the 3D backdrop should do differently from the concept:**
  - the camera is the battle camera (side-on at a slight angle), so the clearing is shallower than in the concept and the characters fill more of the frame;
  - stronger noon light: short hard shadows straight under objects and a heat shimmer over the far fields, so Noon Heat reads before it strikes;
  - the near-black outline on foreground shapes, like on the characters;
  - the foreground rye at the bottom edge stays low, so it never covers the characters' feet or the timing rings.

### Models

The backdrop is built from these models (#149). Each sheet is in `Art/Concepts/RyeFields/`; the labels and dimension lines on the sheets are notes only.

| Model | Sheet | Size | Notes for the model |
|---|---|---|---|
| Rye clumps and stooks | [RyeStooks](../../../Art/Concepts/RyeFields/RyeStooks.png) | clumps 0.6, 0.9 and 1.2 m; stook 1.3 m | three clump sizes, a stook and a stook with a sickle; the rows are the front and back views |
| Field boundary | [FieldBoundary](../../../Art/Concepts/RyeFields/FieldBoundary.png) | fence 2 m long, 0.8 m tall; stones 0.4–0.8 m | a straight and a broken wattle segment and three boundary stones. Replace the carved sun wheel on the big stone with a plain carved circle or a simple folk mark: the hooked shape on the sheet is too easy to misread |
| Roadside cross | [RoadsideCross](../../../Art/Concepts/RyeFields/RoadsideCross.png) | 2.6 m | the front and three-quarter views are the reference: the back view loses the shrine and the side view loses the cross arm. The shrine, its figure, the wreath and the ribbons are separate parts |
| Scarecrow and cart wheel | [FieldProps](../../../Art/Concepts/RyeFields/FieldProps.png) | scarecrow 1.9 m; wheel 1 m across | the wheel is one model, placed standing or lying |
| Barn | [Barn](../../../Art/Concepts/RyeFields/Barn.png) | 6 × 4 m, 5 m tall | the front and three-quarter views are the reference; leave out the chimney and the crossed beams the side view draws on the roof |
| Pear tree and crows | [PearTree](../../../Art/Concepts/RyeFields/PearTree.png) | tree 5 m; crows 0.4 m | the crown is fuller in the front view than in the back view: follow the front and three-quarter views. The crows are separate small models, perched on the branches by the scene |
| Tree line | [TreeLine](../../../Art/Concepts/RyeFields/TreeLine.png) | about 20 m long, 5–8 m tall | a low-detail background silhouette, softer than the props as the style guide allows |

The clearing ground with the field balk is shaped in the scene, not from a sheet.

## Law

**Noon Heat** (from the GDD): every 3rd turn, characters without a Shield lose part of their HP. How it changes these fights comes with the Rye Fields spec.

## Roster and roles

To be written with the Rye Fields spec.

## Wave building blocks

To be written with the Rye Fields spec.

## Counters

To be written with the Rye Fields spec.

## Open questions

- How Lady Midday's riddles (special QTE) work, and how they relate to the Puzzle fight.
- How many Elites the land has, and whether an Elite can be a variant of a regular enemy.
