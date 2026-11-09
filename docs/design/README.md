# Design Documentation

[`GDD.md`](../GDD.md) describes the whole game at a high level. This folder holds the detailed specs of each system, written in the order below. When a spec is merged, replace the dash in its row with a link to the file.

## How to write specs

- **Top-down for the feel, bottom-up for the rules.** Start a system with one page on what the player should feel, then specify its rules from the foundations up. Each layer only uses what the layers before it define.
- **Numbers last.** First decide that something exists and how it works; exact values come with content and balance, and live in the data assets under `Assets/Data`, not in the spec.
- **Same names as the code.** Use the names of the existing building blocks (`UnitDefinition`, `AbilityDefinition`, `AbilityEffect`, `AbilityDelivery`, `StatusDefinition`, `BattleRules`, `EncounterDefinition`, `IEnemyBrain`) so a spec maps directly to classes and assets.
- **Built to extend.** A new behaviour should be a new building block (an effect, a condition, a trigger), not a special rule for one character.
- **Completeness check.** Every hero, enemy and boss can be described only with building blocks from the layers above it. If one needs a rule that exists nowhere else, a building block is missing in a lower layer.
- **One spec per PR.** Open an issue `Spec: <system>` as a sub-issue of the epic that needs it, write the spec in a `chore/` branch, and merge after the other developer approves. Changing a rule later means changing its spec in a PR, not only the code.
- **Write specs just before they are built**, not all at once.

Every spec answers, in this order:
1. **Goal:** why the system exists and what the player should feel.
2. **Concepts:** the terms it introduces.
3. **Rules:** how it works, step by step.
4. **Edge cases:** what happens when things overlap, a unit dies mid-action, and so on.
5. **Connections:** which other systems it uses or affects.
6. **Feedback:** what the player sees and hears.
7. **Starting numbers:** and which data asset holds them.
8. **Done when:** checks that become EditMode tests.
9. **Open questions.**

## Combat

Combat is the core of the game and is specified first, in `combat/`.

| # | Layer | Answers | Spec |
|---|---|---|---|
| 0 | Combat vision | What should the player feel? How long is a battle? How many decisions per turn? What is hard and what is satisfying? | – |
| 1 | Battlefield and participants | Which sides exist? Are there positions or slots? Which unit types (hero, enemy, boss, summon)? How do waves work? | – |
| 2 | Stats | Which stats a unit has (HP, damage, crit, Rage gain, ...) and what each one means exactly | – |
| 3 | Time structure | Battle → wave → round → turn → action: the phases of each | – |
| 4 | Events (triggers) | At which moments of the time structure can something hook in (for example turn start, round end, before damage taken, on death)? | – |
| 5 | Actions | What a unit can do on its turn (attack, support, Rage, potion, pass); targeting rules | – |
| 6 | Building blocks | Effects (what can happen), conditions (when), statuses (duration, stacking, removal) | – |
| 7 | Calculations | How damage and healing are calculated, the order of modifiers, rounding | – |
| 8 | Special mechanics | Attack and block timing, Rage, described with layers 4–7 | – |
| 9 | Enemy behaviour | AI, archetypes, boss phases | – |
| 10 | Sources outside combat | How equipment and sets, Laws of the lands, passives, potions and difficulty plug into combat | – |
| 11 | Feedback | What the player sees and hears for every event; what the UI shows (for example enemy intents) | – |
| 12 | Content | Heroes, enemies, bosses and encounters, built from the layers above. Each character has its own sheet in `characters/`, made from `characters/TEMPLATE.md` | – |
| 13 | Balance | Target values (battle length, turns to defeat an enemy) and how we tune them | – |

## Other systems

Specified after combat, in this order. Each follows the same structure on a smaller scale: vision → concepts → rules → connections → feedback → content → numbers.

| # | System | Covers | Spec |
|---|---|---|---|
| 1 | Party | Roster, picking 3 heroes, character levels and growth | – |
| 2 | World progression | Land map, lands, story missions, side activities, unlocks | – |
| 3 | Equipment and forge | Slots, rarities, sets, recipes, materials | – |
| 4 | Economy | Currencies, where they come from, what they are spent on and how fast | – |
| 5 | Stronghold | Buildings, what they offer, how they grow | – |
| 6 | Screen flow and UI | How the player moves between all of the above | – |
