# Party

System 1 of the other systems (see [`README.md`](README.md)). The GDD covers it in "Characters and party" and the Camp in "Loop and screen flow". This spec covers what is built now: the roster, the seats at the campfire and picking the party of 3. Character levels and growth come later and are listed as open questions.

## 1. Goal

The player builds a party for each fight from the heroes they have collected. The Camp should feel like a gathering around the fire, not a menu: every hero has their own seat, the camp fills up as heroes join, and choosing who goes out is a quick tap on the hero.

## 2. Concepts

- **Roster:** the heroes who have joined, in the order they joined. `GameConfig.heroes` holds it for now.
- **Seat:** one of the fixed places around the campfire (`GameConfig.campSeats`, 6 for the alpha). Hero *n* of the roster always sits on seat *n*. A seat without a hero is an **empty seat**, waiting for a hero who has not joined yet.
- **Party:** the heroes who go into the next run. Its size is `HeroRoster.PartySize`, which is 3.
- **In party / resting:** a joined hero is either in the party or resting at the fire.

## 3. Rules

1. The roster fills the seats in order. Heroes beyond the number of seats are not shown.
2. The party is saved with the game (`SaveData.party`, by asset name) and restored at start.
3. With no saved party, the party is the first 3 heroes of the roster.
4. Tapping a hero in the Camp:
   - a hero in the party leaves it;
   - a resting hero joins the party if it has fewer than 3 heroes; otherwise nothing changes and the player is told the party is full.
5. Every change is saved at once.
6. **Embark** needs a full party; otherwise the player is sent to the campfire.
7. A run starts with the party, in party order.

## 4. Edge cases

- **A saved hero is no longer in the roster** (renamed or removed): that hero is dropped. The rest of the saved party stays, even if that leaves it short.
- **The saved party is empty after dropping**: the first 3 heroes are used, as in rule 3.
- **Fewer than 3 heroes have joined**: the party can't be completed, so Embark stays blocked. This can't happen today, because the roster starts with 3.
- **Tapping an empty seat** changes nothing; the player is told the seat waits for a hero.
- **The party changes during a run**: it can't, because the Camp is in the Stronghold scene and a run lives in the battle scene.

## 5. Connections

- **Run:** `RunSession.Start` receives the party.
- **Save:** the party lives in the same `SaveData` as feathers and perks (`MetaProgression.CreateRoster`), so one save file holds everything. Both scenes share one `MetaProgression` (`SavedGame`).
- **Stronghold:** only party members stand at the fire in the Stronghold view; all joined heroes sit in the Camp view.
- **Later:** levels, equipment, Storage and potions plug into the hero details and the Camp buttons.

## 6. Feedback

- In the Stronghold, tapping the campfire (or a hero at it) flies the camera to the fire; **Back** flies back.
- Each seat shows the hero's name and *In party* or *Resting* as text, with the party in warm gold; an empty seat shows "?" and *Not joined yet*. The words carry the state, not only the colour.
- The header shows *Party n / 3*.
- Holding a hero opens the details: name, party state, abilities (attack, support, Rage), and level and equipment as *coming soon*. Tapping outside closes them.
- Sounds: joining plays the buff sound, leaving a click; entering and leaving the Camp a whoosh.

## 7. Starting numbers

| Value | Number | Where |
|---|---|---|
| Party size | 3 | `HeroRoster.PartySize` |
| Seats at the campfire | 6 | `GameConfig.campSeats` |
| Hold time for details | 0.45 s | `StrongholdBootstrap` |
| Camera flight | 0.7 s | `StrongholdBootstrap` |

## 8. Done when

These are covered by `HeroRosterTests`:
- a new roster starts with the first 3 heroes in the party;
- toggling removes a member and adds a resting hero;
- a fourth hero is refused while the party is full;
- a hero who has not joined is refused;
- the party is saved and restored;
- saved heroes missing from the roster are dropped;
- seats hold heroes in roster order and stay empty after them.

## 9. Open questions

- How heroes join the roster (story missions and side activities can reward them, per the GDD), and in which order they take their seats.
- Character levels and EXP: how the party level shown before battles is made, and whether resting heroes earn anything.
- Whether a party can be smaller than 3 for some activities, or larger later.
- Whether the party order matters in battle (formation), and how the player changes it.
- What the Camp shows when the roster outgrows the seats (12–15 heroes in the full game).
