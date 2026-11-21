# Attack and Block Timing

Layer 8 of the combat specs (see [`../README.md`](../README.md)). The summary is in the GDD's "Timing (QTE)" section; this spec is the exact rule set the battle model and the battle screen follow.

## 1. Goal

Timing is what sets the combat apart: the player is not only choosing actions but also performing them. A well-timed attack hits harder, a well-timed block takes the sting out of an enemy hit, and a perfect one feels great and feeds the party's Rage. It must stay readable and fair: one clear cue per action, no rhythm that can be learned by heart, and no punishment beyond losing the bonus.

## 2. Concepts

- **Timing result:** `Miss`, `Good` or `Perfect`, judged from when the player taps relative to the target moment.
- **Attack ring:** shown when a hero uses a damaging ability. A white ring shrinks onto a gold circle; the target moment is when they meet.
- **Block cue:** shown before a damaging enemy action. The enemy holds its wind-up (`AttackWindup` clip) for a random time, then a **BLOCK!** flash appears; the target moment is the hit that follows it.
- **Window:** how far from the target moment a tap may land and still count as `Good` or `Perfect`.
- **Pending enemy action:** a damaging enemy action that the model has announced but not resolved yet, waiting for the block result.

## 3. Rules

### Attack

1. The player picks a hero, an ability and a target.
2. If the ability deals damage and is not the hero's Rage action, or the ability is marked as always timed, the ability's gesture plays (the ring when it has none).
3. The gesture is judged: within the perfect window → `Perfect`; within the good window → `Good`; otherwise, or no input in time → `Miss`. Gestures made of parts (several rings, several steps) are `Perfect` only when every part is, and `Good` when at least 60% of the parts hit.
4. The model resolves the ability with that result:
   - `Good`: +25% damage on every hit of the ability, +20% healing and Shield points;
   - `Perfect`: +50% damage on every hit, +40% healing and Shield points, the ability's perfect-only effects, and +10 Rage once for the action;
   - `Miss`: the normal effect, no penalty.
5. Other abilities without damage (support, healing) and other Rage actions have no gesture and always resolve as `Miss`.

### Gestures

Each hero action can have its own gesture, and the stronger the action, the harder the gesture: attacks take about 1 s with one element, support about 1.5 s with two elements or a longer movement, Rage 2–3 s as a sequence or a sign drawn from memory, with narrower windows.

| Gesture | What the player does |
|---|---|
| Ring | taps as a shrinking ring meets the gold circle |
| Series | taps several numbered rings in turn, each as it closes |
| Slash | swipes through a gate in the arrow's direction as the ring closes |
| Charge | holds, then releases with the bar in the gold zone; holding too long is a miss |
| Trace | draws along a shown line from its start |
| Guide | holds a moving light and follows it along a path |
| Spin | circles around the centre a given number of times |
| Sign | sees a sign briefly, then draws it from memory |
| Sequence | performs several gestures in a row |

Gestures are presentation: they live on the ability's visuals, are judged by plain classes that can be tested, and only hand the model a `Miss`, `Good` or `Perfect` result.

### Block

1. In the enemy turn, before each damaging enemy action, the model stops and announces the pending action (actor, ability, target).
2. The battle screen plays the actor's `AttackWindup` for a random time, then shows BLOCK! shortly before the hit.
3. The player taps once. A tap before the BLOCK! flash is wasted: the block counts as `Miss` even if the player taps again.
4. The model resolves the pending action with the block result:
   - `Good`: −30% damage on every hit, for every hero the action hits;
   - `Perfect`: −60% damage on every hit, for every hero it hits, and +10 Rage once for the action;
   - `Miss`: normal damage.
5. Enemy actions without damage resolve at once, with no cue.

## 4. Edge cases

- **Multi-hit abilities** (for example two arrows): one ring or one block for the whole ability; the result applies to every hit.
- **Abilities that hit several heroes:** one block protects every hero the action hits.
- **Elites with two actions:** each damaging action gets its own wind-up and block.
- **A due special that deals damage** (for example the Chieftain's Earthquake) is blocked like any other damaging action.
- **Damage over time** (Poison, Burn, Bleed) is not an action and cannot be blocked; it ticks as before.
- **The target dies before a later hit:** later hits on that target are skipped, as without timing.
- **Critical hits:** the timing bonus or reduction multiplies the final damage, crits included.
- **Shield and other modifiers:** they apply as before; the timing multiplier is one more factor on the same roll.
- **Pause:** the ring and the wind-up stop with the game; judging uses game time, not real time.
- **Auto-timing** (accessibility, M2): every timed action resolves as `Good`, never `Perfect`.

## 5. Connections

- **Damage:** the timing multiplier is applied to the damage roll of each hit (`BattleState.DealDamage`).
- **Rage:** a perfect attack or block adds Rage through the same meter as damage; it can make Rage ready mid-turn.
- **Enemy brain:** unchanged; it still decides what the enemy does, and the model then decides whether that action waits for a block.
- **Animation:** heroes play `Attack` after the ring; enemies loop `AttackWindup` while the block cue runs, then play `Attack` with damage on `OnHitFrame`.
- **Sound:** the perfect-timing and block sounds (#22) play on `Perfect` and `Good` results.
- **Statuses with "perfect only" effects** (an extra Burn stack, a guaranteed Stun) read the timing result of the action; the [status spec](statuses.md) defines them as `Perfect timing` applications.

## 6. Feedback

- **Ring:** white ring, gold target circle, flat shapes with the dark outline of the style guide.
- **Result text** above the target: "GOOD" or "PERFECT!", with the perfect one bigger, brighter and with its own sound; nothing for `Miss`.
- **BLOCK! flash:** a bold shape over the targeted hero, readable without colour; the shield shape stays briefly on a successful block.
- **Rage:** the meter pulses when a perfect result adds to it.
- The cue must never depend on colour alone, and each element is readable on a phone in landscape.

## 7. Starting numbers

| Value | Start | Lives in |
|---|---|---|
| Perfect window | ±60 ms around the target moment | `BattleRules` |
| Good window | ±150 ms | `BattleRules` |
| Attack ring duration | 0.8 s from appearing to the target moment | `BattlePresentationConfig` |
| Enemy wind-up hold | random 0.4–1.2 s (through `IRandomSource`) | `BattlePresentationConfig` |
| BLOCK! lead | 0.25 s before the hit | `BattlePresentationConfig` |
| Good attack bonus | +25% damage | `BattleRules` |
| Perfect attack bonus | +50% damage | `BattleRules` |
| Good block reduction | −30% damage | `BattleRules` |
| Perfect block reduction | −60% damage | `BattleRules` |
| Rage for a perfect attack or block | +10 (the meter holds 100) | `BattleRules` |

## 8. Done when

These become EditMode tests on the model:
- an attack with `Miss`, `Good` and `Perfect` deals 100%, 125% and 150% of its normal damage, on every hit;
- a perfect attack adds 10 Rage once per action, not per hit;
- support, healing and the Rage action ignore the timing result;
- in block mode the model stops before each damaging enemy action and resumes with the block result;
- a block with `Miss`, `Good` and `Perfect` lets through 100%, 70% and 40% of the damage, for every hero hit;
- a perfect block adds 10 Rage once per action;
- enemy actions without damage resolve without stopping;
- an Elite's two damaging actions each stop for a block;
- without block mode the enemy turn resolves at once, as before.

## 9. Open questions

- Should a `Miss` attack ever be worse than normal damage, for example on bosses?
- Does the block window shrink against Elites and bosses, or only their wind-up get harder to read?
- How are attack and block timing taught in the tutorial (M2)?
