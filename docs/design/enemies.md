# Enemy Index

One table of every enemy and boss with its design and production stage. Which enemies appear in which land is decided in the Bestiary table of [`GDD.md`](../GDD.md#bestiary); the production steps are in [`ART_PIPELINE.md`](../ART_PIPELINE.md#steps). How an enemy fights in its land is in that land's file in [`lands/`](lands/TEMPLATE.md).

## Rules

- The GDD decides the roster. Adding, removing, renaming or moving an enemy changes the GDD first, in a PR both developers approve; this table then follows it.
- Every PR that moves an enemy or boss to another stage updates its row in the same PR.
- Names are exactly as in the GDD. An archetype is "?" until the GDD or the enemy's sheet sets it.

## Stages

| Stage | Means | `ART_PIPELINE.md` step |
|---|---|---|
| Idea | named in the GDD, nothing else yet | – |
| Sheet | the character sheet in `characters/` is merged | 1 |
| Concept | the concept sheet in `Art/Concepts` is merged | 2 |
| Model and texture | the model and its albedo are in the `.blend` | 3–4 |
| Rig and animations | rigged, animated and exported to `Assets/Art/Characters` | 5–6 |
| In game | in `UnitCatalog` and an encounter, the sheet's checklist ticked | 7–8 |

## Enemies and bosses

| Enemy | Land | Type | Archetype | Sheet | Stage | Issue |
|---|---|---|---|---|---|---|
| Bandit | Rye Fields | enemy | ? | – | Idea | – |
| Werewolf | Rye Fields | enemy | ? | – | Idea | – |
| Polevik | Rye Fields | enemy | ? | – | Idea | – |
| Wolf | Rye Fields | enemy | Slasher | [Wolf](characters/Wolf.md) | In game | #42 |
| Bies | Rye Fields | elite | Elite | [Bies](characters/Bies.md) | In game | #15 |
| Lady Midday | Rye Fields | boss | ? | – | Idea | #25 |
| Drowned Man | Marshes | enemy | ? | – | Idea | – |
| Will-o'-the-Wisp | Marshes | enemy | ? | – | Idea | – |
| Bog Hag | Marshes | enemy | ? | – | Idea | – |
| Rusalka | Marshes | enemy | ? | – | Idea | – |
| Water Maiden | Marshes | boss | ? | – | Idea | – |
| Sabbath Witch | Bald Mountain | enemy | ? | – | Idea | – |
| Planetnik | Bald Mountain | enemy | ? | – | Idea | – |
| Giant | Bald Mountain | enemy | ? | – | Idea | – |
| Sirin | Bald Mountain | enemy | ? | – | Idea | – |
| Zmey | Bald Mountain | boss | ? | – | Idea | – |
| Wild Woman | Old Forest | enemy | ? | – | Idea | – |
| Mamuna | Old Forest | enemy | ? | – | Idea | – |
| Licho | Old Forest | enemy | ? | – | Idea | – |
| Forest Grandfather | Old Forest | enemy | ? | – | Idea | – |
| Leshy | Old Forest | boss | ? | – | Idea | – |
| Wraith | Nav | enemy | ? | – | Idea | – |
| Strzyga | Nav | enemy | ? | – | Idea | – |
| Nightmare | Nav | enemy | ? | – | Idea | – |
| Death | Nav | enemy | ? | – | Idea | – |
| to be decided | Nav | boss | ? | – | Idea | – |

**Count:** 20 regular enemies, 1 elite and 5 bosses, one not yet named; elites are counted apart from regular enemies. 2 are in game (the Wolf and the Bies). The MVP in [`ROADMAP.md`](../ROADMAP.md#content-scale) needs 12 regular enemies and 3 bosses from 3 lands; the Rye Fields give 4 regular enemies, 1 elite and 1 boss.
