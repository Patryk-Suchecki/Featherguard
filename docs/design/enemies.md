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
| Wild Woman | Old Forest | enemy | ? | – | Idea | – |
| Mamuna | Old Forest | enemy | ? | – | Idea | – |
| Licho | Old Forest | enemy | ? | – | Idea | – |
| Forest Grandfather | Old Forest | enemy | ? | – | Idea | – |
| Leshy | Old Forest | boss | ? | – | Idea | – |
| Sabbath Witch | Bald Mountain | enemy | ? | – | Idea | – |
| Planetnik | Bald Mountain | enemy | ? | – | Idea | – |
| Giant | Bald Mountain | enemy | ? | – | Idea | – |
| Sirin | Bald Mountain | enemy | ? | – | Idea | – |
| Zmey | Bald Mountain | boss | ? | – | Idea | – |
| Wraith | Nav | enemy | ? | – | Idea | – |
| Strzyga | Nav | enemy | ? | – | Idea | – |
| Nightmare | Nav | enemy | ? | – | Idea | – |
| Death | Nav | enemy | ? | – | Idea | – |
| to be decided | Nav | boss | ? | – | Idea | – |

**Count:** 21 enemies (one of them elite) and 5 bosses, one not yet named; 2 enemies are in game. The MVP in [`ROADMAP.md`](../ROADMAP.md#content-scale) needs 12 enemies and 3 bosses from 3 lands; the Rye Fields give 5 enemies and 1 boss.

## Candidates

Figures proposed for later lands, not yet in the GDD. Moving one into the roster changes the GDD first; it then gets a row in the table above. The Rye Fields candidates are in [`lands/rye-fields.md`](lands/rye-fields.md#candidates-for-the-regular-enemies).

| Candidate | Fits | Folk belief |
|---|---|---|
| Kikimora | Marshes | a thin spirit of the house and the bog with a bird-like head |
| Rokita | Marshes | the Polish devil of the bogs |
| Boruta | Old Forest | the Polish devil of the forests; must not read like the goat-like Bies |
| Baba Jaga | Old Forest or Bald Mountain, as an Elite | the witch with a hut on a chicken leg who flies in a mortar |
| Psoglav | Bald Mountain or Nav | a dog-headed man-eater from the southern Slavs |
| Koschei the Deathless | Nav, as the boss | a deathless sorcerer whose death is hidden in a needle, in an egg, in a duck |
