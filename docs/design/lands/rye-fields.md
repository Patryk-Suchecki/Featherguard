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

## Law

**Noon Heat** (from the GDD): every 3rd turn, characters without a Shield lose part of their HP. How it changes these fights comes with the Rye Fields spec.

## Roster and roles

To be written with the Rye Fields spec.

### Candidates for the regular enemies

Proposals only: the roster is chosen by both developers and written in the GDD. The land needs 4 regular enemies; the Wolf is one of them. The candidates come from Slavic folk belief about the village and its fields. Each must read as its own black silhouette next to the Wolf (low and long), the Bies (tall and hunched) and Lady Midday (tallest, floating).

| Candidate | Folk belief | Silhouette and size | Accent |
|---|---|---|---|
| Kłobuk | a household spirit that steals grain for its master; it looks like a wet, ruffled hen | small and hunched, about 0.8 m: bedraggled wet feathers, oversized feet, a stolen sheaf of rye clutched under one wing | the bright gold of the stolen grain |
| Skrzat | a little spirit of the barn and the farmyard that works for whoever feeds it and plays tricks on the rest | the smallest in the land, about 0.9 m: an old man's face, a long beard, big hands and feet, a tall drooping cap | a red cap |
| Wąpierz | a villager who will not stay in his grave and comes back to drink the blood of his own; he was buried with a sickle across his neck to keep him down | a gaunt peasant, about 1.6 m, in a grey burial shirt, a flushed, swollen face, the sickle still tied across his neck | blood-red face and lips |

With these the land would read as a low quadruped (Wolf), a small bird (Kłobuk), a little man (Skrzat) and a gaunt man (Wąpierz). The Kłobuk is the first bird and needs its own rig.

First concepts, to choose from: [`Klobuk`](../../../Art/Concepts/Candidates/Klobuk.png), [`Skrzat`](../../../Art/Concepts/Candidates/Skrzat.png), [`Wapierz`](../../../Art/Concepts/Candidates/Wapierz.png) in `Art/Concepts/Candidates`. A chosen candidate gets its sheet and its final concept in `Art/Concepts`.

## Wave building blocks

To be written with the Rye Fields spec.

## Counters

To be written with the Rye Fields spec.

## Open questions

- How Lady Midday's riddles (special QTE) work, and how they relate to the Puzzle fight.
- How many Elites the land has, and whether an Elite can be a variant of a regular enemy.
