# World Progression

System 2 of the other systems (see [`README.md`](README.md)). The GDD covers it in "World structure and progression"; the wireframes show it on the world map, the Sacred Oak, the battle preparation and the battle result screens ([`screen-flow.md`](screen-flow.md)). This spec is the rule set the world map and its saving follow.

## 1. Goal

The world map is where the player decides what to do next. The story pulls them forward through the lands, and side activities let them grow stronger when the next story mission is too hard. The player should always see the next story step and a few useful detours, judge at a glance how dangerous each one is, and never feel stuck. The story is fairly linear, but it moves across the map instead of finishing one land at a time.

## 2. Concepts

- **Land:** one of the five areas on the map (Rye Fields, Marshes, Bald Mountain, Old Forest, Nav). A land is **locked** or **open**, has a **Law** (a combat rule for its battles), its own enemies and materials, and a **side activity pool**. Data: `LandDefinition`.
- **Activity:** something the player can start from the map: a Single fight, Waves fight, Puzzle fight, Elite fight or Boss fight in a land, with a **threat level** and **rewards**. It wraps an `EncounterDefinition`. Data: `ActivityDefinition`.
- **Story mission:** an activity shown as **Main quest**, at a fixed place in the **story order**: one list across all lands (`WorldDefinition.storyMissions`). The Sacred Oak plays the story scene before or after it.
- **Side activity:** an activity drawn from a land's pool.
- **Activity places:** the 1 to 3 activities an open land shows at a time, the story mission included.
- **Reroll:** drawing the side activities of every open land again.
- **Threat level:** 1–10, how hard an activity is: fixed in a story mission's data; a side activity's follows the story ([`side-activities.md`](side-activities.md)).
- **Party strength:** how strong the current party is, on the same 1–10 scale.
- **Threat rating:** what the player sees before a battle: Easy, Even, Dangerous or Madness, from the threat level compared with the party strength.
- **Daring:** the bonus for winning above your strength.
- **World progress:** what is saved: how many story missions are won, and which side activities each land shows.

## 3. Rules

### Story

1. The story order is one list of story missions. **Only the next story mission is available**; the map shows it as the Main quest in its land.
2. Winning a story mission makes the next one in the order available. Losing changes nothing.
3. Every land has 5 story missions (25 in total), in this order:

    | # | Land | # | Land | # | Land | # | Land | # | Land |
    |---|---|---|---|---|---|---|---|---|---|
    | 1 | Rye Fields | 6 | Bald Mountain | 11 | Old Forest | 16 | Bald Mountain | 21 | Bald Mountain (boss) |
    | 2 | Rye Fields | 7 | Marshes | 12 | Rye Fields (boss) | 17 | Marshes (boss) | 22 | Nav |
    | 3 | Marshes | 8 | Bald Mountain | 13 | Marshes | 18 | Old Forest | 23 | Old Forest (boss) |
    | 4 | Rye Fields | 9 | Rye Fields | 14 | Old Forest | 19 | Nav | 24 | Nav |
    | 5 | Marshes | 10 | Bald Mountain | 15 | Nav | 20 | Old Forest | 25 | Nav (boss) |

4. **A land's 5th story mission is its Boss fight**: Lady Midday at 12, the Water Maiden at 17, Zmey at 21, the Leshy at 23 and the boss of Nav at 25. A boss first appears only in its story mission. **Once beaten, the boss can also appear in its land as a side activity.**

### Lands

5. At the start only the Rye Fields are open.
6. **A land opens when the next story mission is in it for the first time**, and it stays open. With the order above: the Marshes open at mission 3, Bald Mountain at 6, the Old Forest at 11 and Nav at 15.
7. Locked lands are visible on the map but can't be entered.

### Activities on the map

8. **Every open land shows 1 to 3 activities.** The number is drawn on every reroll. If the land holds the next story mission, that mission takes one of the places and the rest are side activities.
9. Side activities are drawn at random from the land's pool:
   - only activities whose `availableAfter` story mission has been won;
   - the land's Boss fight only after its boss story mission has been won;
   - never the same activity twice on the map at once.
10. **Every won activity, story mission or side activity, rerolls the side activities of all open lands.** A loss changes nothing, so the lost activity is still there.
11. Elite fights appear as side activities; their enemies are the land's Elite (for example the Bies in the Rye Fields).
12. Draws use `IRandomSource`, and the shown activities are saved, so closing the game does not reroll them.

### Threat and Daring

13. Every story mission and side activity has a threat level 1–10.
14. **Party strength is the average level of the 3 heroes in the party for now** (all heroes are level 1 until levels exist). How equipment and hero choice add to it comes with the equipment work.
15. The threat rating compares the threat level with the party strength:

    | Threat level − party strength | Rating |
    |---|---|
    | −2 or less | Easy |
    | −1 to +1 | Even |
    | +2 to +3 | Dangerous |
    | +4 or more | Madness |

16. Every activity on the map can be started right away, whatever its rating.
17. **Daring:** winning at Dangerous pays ×1.5 and at Madness ×2: more EXP, gold and materials, and a better chance of a higher rarity for materials, loot and schematics ([`side-activities.md`](side-activities.md)). It never affects unlocks or story progress.

### Rewards and results

18. **Every fight gives one reward**, of one kind: EXP, gold, materials, loot (equipment), schematic or unlock. A side activity draws its reward ([`side-activities.md`](side-activities.md)); a story mission has its own unique rewards set in its data and always advances the story. EXP only comes from fights that reward it.
19. Winning grants the reward (with Daring when it applies), rerolls the side activities and returns to the world map.
20. **Losing grants nothing**: no rewards and no EXP. The activity stays on the map, and **Retry** starts it again from its preparation screen. Potions used in the lost battle are spent.

## 4. Edge cases

- **The last story mission is won:** there is no next mission. The map stays open for side activities, and the Sacred Oak says the story is complete.
- **A land's pool has fewer activities that can be drawn than its drawn number:** it shows the ones it has; a land with no story mission and nothing to draw shows nothing until a later reroll.
- **The player quits during a battle:** nothing is saved from that battle; the activity is still on the map, like a loss without the defeat screen.
- **The story order changes between versions** (content added or reordered): the save keeps how many story missions were won. Lands open again from that count, and shown side activities that no longer exist are drawn again.
- **The party strength changes** (a level up, another party): ratings are recalculated whenever the map or a preparation screen is shown.

## 5. Connections

- **Party** ([`party.md`](party.md)): the party's levels give the party strength; EXP from rewards goes to the heroes who fought.
- **Combat:** an activity starts a battle with its `EncounterDefinition`, the land's Law and the party; the battle reports victory or defeat back.
- **Economy and equipment:** rewards add gold, materials, loot and schematics to the save and the Storage.
- **Stronghold:** **Embark** opens the world map; returning from the map goes back to the Stronghold.
- **Sacred Oak:** shows the story progress, the next story mission with its rating, and *Show on map*. It plays the story scenes.
- **Screens:** world map, battle preparation (single fight and waves fight), battle result (victory and defeat).
- **Current build:** this replaced the bird run; its 9 encounters are now the placeholder encounters of the story missions and side activities.

## 6. Feedback

- **World map:** every land is shown; locked ones with a lock. An open land shows its 1 to 3 activities as icons by type, and the Main quest icon stands out in the land that holds it. Types are told apart by shape, not only colour.
- **Tapping an activity** opens its preparation screen: name, type, threat rating, the Law, the reward (with the Daring multiplier when it applies), the number of waves for a waves fight, and a way to change the party (the Camp) and potions.
- **Victory screen:** the reward and the Daring multiplier when earned; EXP per hero and level-ups after a fight that rewards EXP. **Defeat screen:** the rating it had, "no rewards", tips to get stronger, Retry and back to the map.
- **Returning to the map after a win:** the side activities change with a short shuffle, so the player sees the reroll.
- **Sacred Oak:** *Story missions n / 25*, missions won per land, and the next story mission.
- **A land opening** gets a short moment on the map (the land lights up, a sound) the first time the player returns to it.

## 7. Starting numbers

| Value | Number | Where |
|---|---|---|
| Story missions | 25, 5 per land, in the order of rule 3 | `WorldDefinition.storyMissions` |
| Activities per open land | 1–3, the story mission included | `WorldDefinition.activitiesPerLand` |
| Threat levels | 1–10 | `ActivityDefinition.threat` |
| Rating thresholds | −2 / +2 / +4 | `WorldDefinition.ratingThresholds` |
| Daring multiplier | Dangerous ×1.5, Madness ×2 | `WorldDefinition.daringMultipliers` |

The threat level and rewards of each story mission are content and come with it. Side activities follow [`side-activities.md`](side-activities.md).

## 8. Done when

These become EditMode tests on the world progress model:
- a new world has only the Rye Fields open and story mission 1 as the next one;
- winning the next story mission makes the following one next; losing changes nothing;
- each land opens at the story mission given in rule 6, and stays open;
- every open land shows 1 to 3 activities, the next story mission counted when it is there;
- side activities respect `availableAfter`, never repeat on the map, and include a land's Boss fight only after its boss mission is won;
- any win rerolls the side activities of all open lands; a loss changes nothing;
- the threat rating follows the thresholds;
- every win grants one reward; the Daring multiplier is ×1.5 at Dangerous and ×2 at Madness, and never applies to unlocks;
- a defeat grants nothing;
- world progress and the shown side activities are saved and restored, and ones that no longer exist are drawn again.

## 9. Open questions

- **MVP scope vs. the story order:** the roadmap plans 3 lands and 15 story missions for the first release, but the first 15 missions of this order already reach 5 lands (the Old Forest at 11 and 14, Nav at 15). Either the MVP keeps fewer missions of the first lands, or its 3 lands are chosen differently.
- What drives the main story and how it ends (from the GDD), and the boss of Nav.
- Whether a land's Law grows stronger as the story advances in that land (from the GDD).
- How the threat levels of story missions rise through the story (side activities follow them, see [`side-activities.md`](side-activities.md)).
- How equipment and hero choice count in the party strength (rule 14 uses levels only).
- How a Puzzle fight plays (from the GDD).
- The names of the ratings: the wireframe's preparation screens say Medium and Hard (from the GDD).
