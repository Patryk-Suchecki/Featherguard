# Potions and Schematics

System 7 of the other systems (see [`README.md`](README.md)). The GDD covers potions and schematics in "Equipment, forge and economy"; the wireframes show the Wise Woman's Hut (screen X), the potions of the battle preparation and the battle HUD ([`screen-flow.md`](screen-flow.md)). This spec covers which potions exist, their recipes, how they are used in battle, and how schematics unlock the recipes.

## 1. Goal

Potions are the player's single-use trump cards: a heal at the right moment, an answer to a land's Law, a revived hero. They can swing a fight, so they cost materials the player has to fight for, and the player chooses before each battle which three to bring. Learning a new recipe should feel like a reward from the map, and each land should teach potions that help against its own and later Laws.

## 2. Concepts

- **Potion** (`PotionDefinition`): a single-use item with an effect, a target and a recipe. Potions have no rarity; only materials do.
- **Recipe:** the materials and gold one potion costs to brew.
- **Schematic** (`SchematicDefinition`): unlocks one potion's recipe for good. A recipe without its schematic cannot be brewed.
- **Schematic fight:** a side activity template whose fixed reward is one schematic.
- **Potion bag:** every potion the player owns, with its count; saved with the rest of the progress.
- **Battle potions:** the up to 3 potions picked in the battle preparation for one fight.
- **Thrown potion:** a potion whose target is an enemy or all enemies; the others are drunk by a hero or poured over the party.

## 3. Rules

### Potions

1. The potions and their starting recipes (numbers are starting values; gold costs come with the economy spec):

    | Potion | Effect | Target | Recipe |
    |---|---|---|---|
    | Healing Draught | heals 35% of max health | one hero | Marsh Herbs ×2, Flax ×1 |
    | Cleansing Brew | removes every harmful status; no harmful status can be applied to the hero until their next turn | one hero | Flax ×2, Amber ×1 |
    | Wolf's Fury | +40 Rage | the party | Wolf Pelt ×1, Flax ×2 |
    | Sheaf Ward | Shield of 15% of max health for 1 turn (the Rye Fields' Noon Heat spares Shielded heroes) | every hero | Last Sheaf ×1, Flax ×2 |
    | Wisp Oil | Mark for 2 turns; hidden enemies are revealed (the Old Forest's Misleading) | every enemy | Wisp in a Jar ×1, Amber ×1 |
    | Bog Mist | Weaken for 2 turns | every enemy | Marsh Herbs ×2, Amber ×1 |
    | Fire Flask | 30 damage and 2 stacks of Burn; fire, so the fallen in Nav do not rise | one enemy | Brimstone ×2, Flax ×1 |
    | Storm Draught | immune to Stun and to the Bald Mountain's lightning for 3 turns | one hero | Thunderstone ×1, Marsh Herbs ×1 |
    | Heartwood Brew | +30% damage for 3 turns | one hero | Old Oak Heartwood ×1, Tinder Fungus ×1 |
    | Gromnica Light | for 3 turns the party's healing is not halved and fallen enemies do not rise (Nav's Chill of Nav) | the party | Gromnica Wax ×1, Bone Ash ×2 |
    | Firebird Draught | a fallen hero returns with 40% of max health | one fallen hero | Firebird Feather ×1, Brimstone ×2 |
    | Dead Water Tincture | heals to full and removes Bleed | one hero | Dead Water ×1, Marsh Herbs ×2 |
    | Kupala Dew Elixir | heals 30% of max health and removes every harmful status | every hero | Kupala Dew ×1, Marsh Herbs ×2 |
    | Living Water | every fallen hero returns with 50% of max health; living heroes heal 50% | the party | Living Water ×1, Dead Water ×1 |
    | Fern Flower Elixir | fills the Rage meter; +30% damage for 2 turns | the party | Fern Flower ×1, Kupala Dew ×1 |

2. Mountain Ore, Yew Wood, Serpent King's Crown and Alatyr Stone are in no recipe; they are left for the forge.
3. Every recipe also costs gold.
4. Effects are built from the existing blocks: healing, statuses ([`combat/statuses.md`](combat/statuses.md)), Rage ([`combat/rage.md`](combat/rage.md)) and damage. Reviving a fallen hero, healing that ignores Chill of Nav and stopping the fallen from rising are new effects the potions add.

### Brewing (Wise Woman's Hut)

5. The Hut lists every potion. A potion whose schematic the player lacks shows a lock and the schematic icon; the **Healing Draught is known from the start** and needs no schematic.
6. Picking a potion shows its effect, the count owned, and every ingredient with the amount owned and needed.
7. The player picks how many to brew (− and +). **Brew** is available while the materials and gold cover that count; it takes them and adds the potions to the potion bag.

### Schematics

8. Every potion except the Healing Draught has one schematic, and one schematic fight in the pool of the land of its main ingredient.
9. A schematic fight is drawn onto the map like any side activity. Its marker and its preparation show only that the reward is a schematic, never which potion it teaches; the victory screen reveals it.
10. Winning it unlocks the recipe for good, and the fight leaves the pool. (Later more fights can award the same schematic; winning any of them removes all of them: #188.)
11. A schematic fight enters the pool at a story threshold (its `availableAfter`):

    | From the start (Rye Fields) | After mission 3 (Marshes) | After mission 6 (Bald Mountain) | After mission 11 (Old Forest) | After mission 15 (Nav) |
    |---|---|---|---|---|
    | Cleansing Brew, Wolf's Fury, Sheaf Ward | Wisp Oil, Bog Mist | Fire Flask, Storm Draught | Heartwood Brew | Gromnica Light |

    The schematics for the potions brewed from Epic and Legendary materials follow the bosses: Firebird Draught after mission 12 (Lady Midday), Dead Water Tincture after 17 (the Water Maiden), Kupala Dew Elixir after 21 (Zmey), Living Water after 23 (the Leshy) and Fern Flower Elixir after 25 (the end of the story).
12. A schematic the player already owns never appears again.

### Using potions in battle

13. In the battle preparation, **Potions** picks up to 3 potions from the bag for the fight.
14. In battle, the picked potions sit in a tile in the lower left corner of the screen, each with its count.
15. A potion is used like a hero's action: the player drags it from the tile onto its target, with the same arrow as when dragging a hero. Valid targets light up while dragging; letting go elsewhere cancels.
16. Potions can only be used during the player's turn. Using one is an extra action: it uses up no hero's action and does not end the turn, so the player can use several in one turn. There is no timing QTE.
17. A used potion is gone, also when the battle is lost ([`world-progression.md`](world-progression.md) rule 20). Picked potions that were not used go back to the bag.

## 4. Edge cases

- **No valid target** (Firebird Draught with no fallen hero, a revive after the whole party fell): the potion cannot be dropped and stays in the tile.
- **Heal over max health:** healing stops at max health.
- **Immunity and a harmful status in the same turn:** Cleansing Brew removes what is on the hero first, then blocks new ones until the hero's next turn.
- **The battle ends while a potion's effect lasts:** the effect ends with the battle.
- **A schematic fight is lost:** it stays on the map like any lost activity.
- **The player owns a potion whose recipe they never learned** (from the shop later): it can be used, but not brewed.

## 5. Connections

- **Materials and Storage:** recipes take materials from the Storage ([`side-activities.md`](side-activities.md), the GDD's Materials).
- **Side activities:** schematic fights are side activity templates with a fixed schematic reward, like unlocks there (rule 13); their kind is never drawn by weight.
- **Battle preparation:** the Potions button picks the battle potions.
- **Battle:** potions use the drag-to-act input and arrow; their effects use the statuses, Rage and the new effects of rule 4. Laws: Sheaf Ward (Noon Heat), Wisp Oil (Misleading), Storm Draught (Storm), Fire Flask and Gromnica Light (Chill of Nav).
- **Economy:** gold costs of recipes, and later schematics and potions in the shop.

## 6. Feedback

- **Hut:** the grid of potions with owned counts; locked recipes show a lock and the schematic icon; the selected potion's panel shows the effect, ingredients as owned / needed (short ones in a warning colour) and the brew count.
- **Map and preparation:** a schematic fight's marker uses the schematic reward colour, and its preparation says "Reward: Schematic" without naming the potion.
- **Battle:** the potion tile in the lower left corner with counts; while dragging, the arrow and the valid targets light up; using a potion plays its sound and effect over the targets, and the count drops.
- **Victory:** a won schematic shows "New recipe: <potion>".

## 7. Starting numbers

| Value | Number | Where |
|---|---|---|
| Potions per battle | 3 | `PotionConfig.perBattle` |
| Effects | as in rule 1 | each `PotionDefinition` |
| Recipes | as in rule 1, plus a gold cost | each `PotionDefinition.recipe` |
| Schematic thresholds | as in rule 11 | each schematic fight's `availableAfter` |

## 8. Done when

EditMode tests:
- only the Healing Draught can be brewed without a schematic;
- brewing takes the recipe's materials and gold per potion and adds that many to the bag; it is refused when anything is short;
- winning a schematic fight unlocks its recipe and removes the fight from the pool; an owned schematic is never drawn again;
- schematic fights respect their story threshold;
- at most 3 potions are picked for a battle, and unused ones return to the bag;
- a potion can only be used in the player's turn and only on a valid target, uses up no hero's action, and is spent when used, also in a lost battle;
- a schematic fight does not tell which schematic it gives until it is won;
- each potion's effect matches rule 1 (heal, statuses, Rage, revive, damage).

## 9. Open questions

- Gold costs, and the balance of effects and recipes (playtests).
- Whether the shop sells schematics and potions, as the GDD says.
- More fights per schematic (#188).
