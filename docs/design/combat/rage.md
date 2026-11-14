# Party Rage

Layer 8 of the combat specs (see [`../README.md`](../README.md)), next to [timing](timing.md). The summary is the GDD's "Party Rage" section; this spec is the exact rule set the battle model follows.

## 1. Goal

Rage is the party's shared big moment. Fighting well (dealing damage, timing attacks and blocks perfectly) and surviving hits builds it up together, and when it is full the player picks which hero turns it into a turn-changing action for the whole side. It should feel earned, be visible at a glance, and create one clear decision ("who uses it, and now or later?") rather than bookkeeping.

## 2. Concepts

- **Rage meter** (`RageMeter`): one meter for the party, from 0 to its maximum. Enemies have no meter.
- **Ready:** the meter is full.
- **Rage action:** a hero's special ability (`UnitDefinition.special`), usable only when the meter is ready. It always affects a whole side: all allies or all enemies.
- **Rage gain:** the hero stat `RageGain`, a multiplier on the Rage that hero's own damage earns (equipment and perks raise it).

## 3. Rules

### Filling

The meter fills from these sources only, each counted after Shield soaks its part:

| Source | Rage |
|---|---|
| Damage a hero deals (hits and damage over time from a hero's status) | 0.45 per point of health it removes × the dealing hero's `RageGain` (damage over time ignores `RageGain`) |
| Damage the party takes (enemy hits and damage over time from an enemy's status) | 0.25 per point of health a hero loses |
| A perfect attack or a perfect block | +10, once per action ([timing](timing.md)) |

- Healing, Shields, statuses that deal no damage, and damage soaked by a Shield give no Rage.
- Damage beyond the target's remaining health counts only up to that health.
- The meter never goes above its maximum; Rage earned while it is full is lost.

### Using

1. When the meter is ready, any living hero who is not stunned can use its Rage action during the player's turn, **whether or not that hero has already acted this turn**.
2. The Rage action **does not use up the hero's move**: a hero that has not acted yet can still attack or support afterwards.
3. Using it empties the whole meter.
4. At most **one Rage action per player turn**, even if the meter fills again in the same turn; the refilled meter waits for the next turn.
5. A Rage action has no target choice (it affects a whole side), no attack ring and no attack timing bonus.

### Keeping

- The meter starts every battle at 0 and lasts through all waves of that battle.
- Other systems may start a battle with Rage (for example a Rage potion or a perk); the starting value never goes above the maximum.

## 4. Edge cases

- **The meter fills during the enemy turn** (from damage taken or a perfect block): it is ready from the start of the next player turn.
- **The meter fills during the player's turn after a Rage action was already used:** it stays full until the next player turn.
- **The meter is ready but every hero has acted:** the player can still use a Rage action before ending the turn, because using it needs no move.
- **The only hero who could use it is stunned:** the meter stays full until a hero can use it.
- **A Rage action kills the last enemy of a wave:** the next wave enters and a new player turn starts, as after any action that clears a wave.
- **A hero dies while the meter is full:** nothing is lost; any other living hero can use it.
- **Enemy specials** do not use a meter: they keep their own rhythm (`EnemyAiProfile.specialEvery`) and are announced by the enemy behaviour rules.
- **A perfect block of an enemy action that deals no damage to health** (fully soaked by a Shield) still gives +10: it rewards the timing, not the damage.

## 5. Connections

- **Damage and statuses:** Rage counts health removed after Shield, as the [status spec](statuses.md) describes.
- **Timing:** +10 for perfect results; the Rage action gets no ring ([timing](timing.md)).
- **Content:** every hero's Rage action targets a whole side (`AbilityTarget.AllEnemies` or `AllAllies`); the prototype heroes whose specials target one unit change with the hero rework (#26).
- **Equipment, perks and potions:** raise `RageGain`, start a battle with Rage, or refill it; their specs decide how much.
- **Presentation:** `RageChanged` (with `BecameReady`) drives the meter and the ready cue.

## 6. Feedback

- **Meter:** always visible in the battle HUD; it fills smoothly and shows the value as a share.
- **Ready:** a clear change of shape and a sound, not only colour; the heroes who can use it are marked.
- **Perfect timing:** the meter pulses when a perfect result adds to it.
- **Using it:** the hero's name and the action's name as a callout, then the meter empties visibly.
- **Already used this turn:** a full meter after a Rage action shows that it waits for the next turn.

## 7. Starting numbers

| Value | Start | Lives in |
|---|---|---|
| Meter maximum | 100 | `BattleRules.maxRage` |
| Rage per point of damage a hero deals | 0.45 × `RageGain` | `BattleRules.heroRagePerDamage` |
| Rage per point of damage the party takes | 0.25 | `BattleRules.enemyRagePerDamage` |
| Rage for a perfect attack or block | +10 | `BattleRules.perfectTimingRage` |
| Rage actions per player turn | 1 | rule, no value |

## 8. Done when

These become EditMode tests on the model:
- a hero's hit adds 0.45 per point of health removed × its `RageGain`; damage soaked by a Shield adds nothing;
- damage the party takes adds 0.25 per point of health lost;
- damage over time adds Rage like a hit from its source side;
- the meter never goes above its maximum;
- a Rage action needs a full meter, empties it, and does not use up the hero's move;
- a hero that has already acted can use the Rage action;
- a stunned hero cannot use it;
- only one Rage action per player turn, even after the meter refills;
- the meter keeps its value between waves and starts each battle at the starting value.

## 9. Open questions

- Should some Rage actions charge only part of the meter (cheaper, weaker actions), or is it always all or nothing?
- Should the meter slowly drain when the party plays passively, to push tempo?
- How much Rage does a Rage potion give, and does it count as the party's one Rage action if it fills the meter mid-turn?
