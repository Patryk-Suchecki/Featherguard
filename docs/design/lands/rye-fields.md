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
  - stronger noon light: short hard shadows straight under objects and a heat shimmer over the far fields, so the midday heat reads before it strikes;
  - the near-black outline on foreground shapes, like on the characters;
  - the foreground rye at the bottom edge stays low, so it never covers the characters' feet or the timing rings.

### Models

The backdrop is built from these models (#149). Each sheet is in `Art/Concepts/RyeFields/`; the labels and dimension lines on the sheets are notes only.

| Model | Sheet | Size | Notes for the model |
|---|---|---|---|
| Rye clumps and stooks | [RyeStooks](../../../Art/Concepts/RyeFields/RyeStooks.png) | clumps 0.6, 0.9 and 1.2 m; stook 1.3 m | three clump sizes, a stook and a stook with a sickle; the rows are the front and back views |
| Field boundary | [FieldBoundary](../../../Art/Concepts/RyeFields/FieldBoundary.png) | fence 2 m long, 0.8 m tall; stones 0.4–0.8 m | a straight and a broken wattle segment and three boundary stones; the biggest carries a plain carved circle with a dot |
| Roadside cross | [RoadsideCross](../../../Art/Concepts/RyeFields/RoadsideCross.png) | 2.6 m | the front and three-quarter views are the reference: the back view loses the shrine and the side view loses the cross arm. The shrine, its figure, the wreath and the ribbons are separate parts |
| Scarecrow and cart wheel | [FieldProps](../../../Art/Concepts/RyeFields/FieldProps.png) | scarecrow 1.9 m; wheel 1 m across | the wheel is one model, placed standing or lying |
| Barn | [Barn](../../../Art/Concepts/RyeFields/Barn.png) | 6 × 4 m, 5 m tall | no chimney; the crossed beams sit only on the two gables, and the hay opening only on the front gable |
| Pear tree and crows | [PearTree](../../../Art/Concepts/RyeFields/PearTree.png) | tree 5 m; crows 0.4 m | the crown is fuller in the front view than in the back view: follow the front and three-quarter views. The crows are separate small models, perched on the branches by the scene |
| Tree line | [TreeLine](../../../Art/Concepts/RyeFields/TreeLine.png) | about 20 m long, 5–8 m tall | a low-detail background silhouette, softer than the props as the style guide allows |

The clearing ground with the field balk is shaped in the scene, not from a sheet.

## Enemy ideas

Lands have no Laws: these fights differ by their enemies. The former Law becomes an idea for enemy and boss abilities here: the midday heat that hurts heroes without a Shield, for Lady Midday and the enemies of her fields. How they play comes with the land's enemy sheets.

## Roster and roles

To be written with the Rye Fields spec.

### Candidates for the regular enemies

Proposals only: the roster is chosen by both developers and written in the GDD. The land needs 4 regular enemies; the Wolf is one of them. The candidates come from Slavic folk belief about the village and its fields. Each must read as its own black silhouette next to the Wolf (low and long), the Bies (tall and hunched) and Lady Midday (tallest, floating).

| Candidate | Folk belief | Silhouette and size | Accent |
|---|---|---|---|
| Kłobuk | a household spirit that steals grain for its master; it looks like a wet, ruffled hen | small and hunched, about 0.8 m: bedraggled wet feathers, oversized feet, a stolen sheaf of rye clutched under one wing | the bright gold of the stolen grain |
| Skrzat | a little spirit of the barn and the farmyard that works for whoever feeds it and plays tricks on the rest | the smallest in the land, about 0.9 m: an old man's face, a long beard, big hands and feet, a tall drooping cap | a red cap |
| Wąpierz | a villager who will not stay in his grave and comes back to drink the blood of his own; he was buried with a sickle across his neck to keep him down | a gaunt peasant, about 1.6 m, in a grey burial shirt, a flushed, swollen face, the sickle still tied across his neck | blood-red face and lips |
| Bebok | a Silesian bogeyman who carries off children in his sack; he waits at the edge of the village fields | hunched and shaggy, about 1.6 m: a huge patched sack over his shoulder, a mop of matted hair hiding the face, long arms with big hands | yellow eyes glinting under the hair |
| Dusiołek | the little demon from Leśmian's ballad that sits on a sleeper's chest and chokes him; it finds the peasant napping by the road at noon | small and heavy, about 0.7 m: a round, stone-like body, spindly legs, big flat pressing hands, heavy drowsy eyelids | poppy red, the flower of sleep, as in Lady Midday's wreath |

With the Kłobuk, the Skrzat and the Wąpierz the land would read as a low quadruped (Wolf), a small bird (Kłobuk), a little man (Skrzat) and a gaunt man (Wąpierz). The Bebok and the Dusiołek are further options; the Dusiołek, the Kłobuk and the Skrzat are all small, so at most one or two of them fit one land. The Kłobuk is the first bird and needs its own rig.

First concepts, to choose from: [`Klobuk`](../../../Art/Concepts/Candidates/Klobuk.png), [`Skrzat`](../../../Art/Concepts/Candidates/Skrzat.png), [`Wapierz`](../../../Art/Concepts/Candidates/Wapierz.png), [`Bebok`](../../../Art/Concepts/Candidates/Bebok.png), [`Dusiolek`](../../../Art/Concepts/Candidates/Dusiolek.png) in `Art/Concepts/Candidates`. A chosen candidate gets its sheet and its final concept in `Art/Concepts`.

## Wave building blocks

To be written with the Rye Fields spec.

## Counters

To be written with the Rye Fields spec.

## Open questions

- How Lady Midday's riddles (special QTE) work, and how they relate to the Puzzle fight.
- How many Elites the land has, and whether an Elite can be a variant of a regular enemy.
