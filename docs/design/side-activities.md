# Side Activities

Part of system 2, world progression (see [`world-progression.md`](world-progression.md)), which says where side activities appear (rules 8–12) and how threat and Daring work (rules 13–17). This spec covers how a side activity is built, how its threat follows the story, how the map draws it and what it rewards.

## 1. Goal

Side activities are the player's answer to "the next story mission is too hard", and the only source of a land's materials. After every win the player should see a real choice on the map: a safe fight, an even one and sometimes a risky one, each showing the one reward it gives. A land the story has left behind should stay worth visiting, and the player should never be left with only fights they cannot win.

## 2. Concepts

- **Side activity template:** an entry in a land's pool: type, encounter, `availableAfter`, and optionally a fixed reward. It has no fixed threat. Data: `ActivityDefinition` in `LandDefinition.sideActivities`.
- **Offer:** a template drawn onto the map, with a rolled **threat level** and a rolled **reward**. Offers are what the save keeps.
- **World level:** the threat level of the next story mission (10 once the story is complete). Side activities follow it.
- **Land floor:** the threat level of the land's first story mission. A land's offers are never easier than that.
- **Threat offset:** how far an offer's threat is from the world level, rolled per offer.
- **Reward kind:** EXP, gold, materials, loot (equipment), schematic or unlock. **Every fight gives exactly one kind.**
- **Daring multiplier:** how much more a win pays at a harder threat rating.

## 3. Rules

### Threat

1. An offer's threat = world level + threat offset + type bonus, at least the land floor, clamped to 1–10.
2. The offset is rolled per offer from a weighted table (starting numbers below), so most offers are near the world level and a few are easier or harder.
3. Type bonus: Single and Waves +0, Elite +1, a returning Boss +2.
4. An offer's enemies get stronger with every threat level above their encounter's base threat (more health and damage per level, set in `WorldDefinition`). An encounter's base threat is the threat it was built for.
5. Story missions keep the fixed threat from their data; only side activities scale.

### The draw

6. The number of places per land (1–3, world progression rule 8) is drawn by weight, most often 2.
7. Each place draws a template from the land's pool by type weight. Types with nothing drawable are skipped. Puzzle fights are not drawn until their spec exists.
8. A land never shows two offers of the same type at once.
9. The encounter that was just won is not drawn again in its land on the next reroll.
10. **Map-wide guarantees**, checked after every draw against the current party strength:
    - at least one side offer is rated Easy or Even; if none is, the offer with the lowest threat is lowered until it is (not below 1);
    - at most one offer is rated Madness; any other Madness offer is lowered to Dangerous.
11. Every win rerolls all offers (world progression rule 10). The draw uses `IRandomSource`, and the offers are saved (rule 12).

### Rewards

12. **A side offer gives exactly one reward.** Its kind is drawn by weight when the offer is drawn: EXP, gold, materials, loot or schematic. A kind that cannot be given yet (for example schematics before they exist) is not drawn.
13. **Unlocks** are set by hand: a template with a fixed unlock (a hero, for example) always gives it. Once won, the template leaves the pool for good.
14. **Amount and rarity grow with the threat level and the type:**
    - EXP and gold: a base amount that grows with the threat level, times the type multiplier (Waves and Elite pay more than a Single fight, a returning Boss the most);
    - materials: the Common, Uncommon and Rare materials of the land the offer is in; more of them, and a better chance of a rarer kind, at a higher threat and a harder type. The universal Epic and Legendary materials ([GDD](../GDD.md#equipment-forge-and-economy)) can also drop, rarely, in any land;
    - loot and schematics: one item; the threat level and type raise the chance of a higher rarity. Epic and Legendary loot only come from Elites and Bosses.
15. **Daring multiplier** on top of rule 14: Easy and Even ×1, Dangerous ×1.5, Madness ×2. It multiplies the amount of EXP, gold and materials, and multiplies the chance of a higher rarity for materials, loot and schematics. It never affects unlocks.
16. EXP goes to the 3 heroes who fought ([`party.md`](party.md)). **A fight that does not reward EXP gives none**, so the player chooses when to level and which heroes take part.
17. **Story missions** have their own unique rewards, set by hand in their data, and always advance the story. Their threat is fixed, but the Daring multiplier applies to their rewards the same way.

### Pacing target

18. Balance aims at this: a player who wins about one EXP side activity between story missions keeps the next story mission at Even. One who skips side activities finds it Dangerous within a few missions; one who plays more finds it Easy. Hero levels (cap 10) and the story's threat (1–10) rise together.

## 4. Edge cases

- **The party changes after the draw:** ratings are recalculated (world progression), but offers are not redrawn, so the guarantees of rule 10 can stop holding until the next win. The Daring multiplier uses the rating at the start of the battle.
- **A land's pool has only one drawable type:** the land shows one side offer.
- **An unlock template is drawn but lost:** it stays on the map like any lost activity and can come back after the next reroll.
- **The story is complete:** the world level stays at 10, so offers sit at threat 8–10, and returning Bosses keep coming back.
- **An offer saved by an older version** without a threat or reward: it is drawn again.
- **The guarantees cannot be met** (every open land's floor is too high for the party): the easiest offer is shown anyway, and the defeat screen's tips point the player to their party.

## 5. Connections

- **World progression:** this spec replaces the fixed threat of side activities and makes rewards a single kind (rules 17–19 there point here).
- **Main quest:** the story missions' threat curve sets the world level.
- **Party:** party strength gives the rating; EXP rewards go to the fighters.
- **Combat:** the battle setup scales enemy health and damage by the offer's threat (`ActivityBattle`).
- **Economy and equipment:** gold, materials, loot and schematics go to the save and the Storage. A land's own materials are only found in that land, so every land stays worth visiting; the universal Epic and Legendary ones can come from any land.

## 6. Feedback

- **Map marker:** the type's shape; the rating as a coloured ring and a word under the name (Easy, Even, Dangerous, Madness); a flame at Dangerous and Madness with "×1.5" or "×2"; and the reward's icon (EXP star, coin, the land's material, chest for loot, scroll for schematic, a portrait for a hero unlock). Rating and reward read by shape and word, not only colour.
- **Preparation screen:** the threat rating, the reward with its amount or rarity chances, and the Daring multiplier when it applies.
- **Victory screen:** the one reward, with the multiplier when earned; EXP per hero and level-ups only after an EXP fight.
- **After a win:** the old offers fade and the new ones drop in one land at a time, so the reroll reads.
- The guarantees are not shown: the player just always finds something they can win.

## 7. Starting numbers

| Value | Number | Where |
|---|---|---|
| Places per land | 1: 25%, 2: 50%, 3: 25% | `WorldDefinition.placeWeights` |
| Threat offset | −2: 15%, −1: 20%, 0: 30%, +1: 20%, +2: 10%, +3: 5% | `WorldDefinition.threatOffsets` |
| Type weights | Single 35, Waves 35, Elite 20, returning Boss 10 | `WorldDefinition.typeWeights` |
| Type bonus to threat | Elite +1, returning Boss +2 | `WorldDefinition` |
| Enemy growth per threat level | +15% health, +10% damage | `WorldDefinition.threatHealthStep`, `threatDamageStep` |
| Reward kind weights | EXP 30, gold 25, materials 25, loot 15, schematic 5 | `WorldDefinition.rewardWeights` |
| Gold | 20 + 10 × threat | `WorldDefinition` |
| EXP | comes with the party levels spec | – |
| Type multiplier | Single ×1, Waves ×1.5, Elite ×2, returning Boss ×3 | `WorldDefinition` |
| Daring multiplier | Easy and Even ×1, Dangerous ×1.5, Madness ×2 | `WorldDefinition.daringMultipliers` |

Material amounts and the rarity chances per threat level come with the economy spec.

## 8. Done when

EditMode tests on the world progress model:
- an offer's threat follows rule 1 and stays within the land floor and 1–10;
- offsets, place counts, types and reward kinds follow their weights with a fixed `FakeRandom`;
- a land never shows the same type twice, and the encounter just won is not drawn again there;
- after a draw at least one offer is Easy or Even and at most one is Madness;
- story missions keep their fixed threat;
- enemy health and damage grow per threat level above the encounter's base;
- every win grants exactly one reward kind; EXP only after an EXP fight;
- an unlock template gives its unlock and leaves the pool once won;
- gold follows threat × type, and the Daring multiplier is ×1, ×1.5 and ×2 by rating, never on unlocks;
- offers with their threat and reward are saved and restored; old ones without them are drawn again.

## 9. Open questions

- How Puzzle fights play and when they join the draw.
- The EXP amounts and the level curve (party levels).
- Material amounts and rarity chances per threat level, and loot rarity chances (economy and equipment specs).
- Which side activities carry hero unlocks, and in which lands.
