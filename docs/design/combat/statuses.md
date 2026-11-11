# Statuses

Layers 6 and 7 of the combat specs (see [`../README.md`](../README.md)), the status part: what a status is made of, how statuses last, stack and leave, and where they enter the damage calculation. The summary is the GDD's "Statuses" table; this spec is the exact rule set the battle model follows.

## 1. Goal

Statuses are the second language of combat after damage: they let a hero protect an ally, let an enemy threaten one target, and give each archetype something to answer. The player should read every status at a glance, predict what it will do this turn, and feel the answer land (a Shield soaking a hit, a Freeze making the Bies act last). They must never pile up into numbers nobody can follow: each status does one clear thing, stacks in one known way, and leaves at a known time.

## 2. Concepts

- **Status** (`StatusDefinition`): a named rule set a unit can carry, for example Bleed or Shield. It holds only rules and numbers; its icon and colour live in `StatusVisuals`.
- **Behaviour:** one building block of a status, a strategy on the status (`StatusBehaviour`). A status is a list of behaviours, so a new kind of status is a new behaviour class, never a new branch in battle code. The behaviours this spec needs:

| Behaviour | What it does |
|---|---|
| Stat modifier | Changes a stat while the status is on (today's `modifiers`) |
| Damage over time | Deals damage at the start of the owner's turn (today's `damagePerTurn`) |
| Skip turn | The owner skips its turn (today's `skipsTurn`) |
| Force targeting | Opponents' single-target abilities must target the owner (today's `forcesTargeting`) |
| Absorb damage | Soaks incoming damage up to its value before health |
| Act last | The owner acts after its side's other units |
| Consumed on attack | The status leaves after the owner uses a damaging ability (today's `consumedOnAttack`) |
| Consumed by a critical hit | The status leaves after the owner takes a critical hit |

- **Application** (`StatusApplication`): what an ability puts on a unit: the status, its turns, its potency, its chance, whether it goes on the actor itself, and a **condition**.
- **Condition:** when an application happens: `Always` (today) or `Perfect timing` (only when the action's attack timing is `Perfect`).
- **Potency:** the application's number: damage per turn for damage over time, base points for Absorb damage.
- **Source side:** the side whose unit applied the status; damage over time gives Rage to that side.
- **Stacks:** how many times a stacking status is on a unit.
- **Stacking rule** (per status): `Refresh` (one instance; a new application refreshes it) or `Stack` up to a maximum.
- **Harmful:** a status a cleanse removes (Bleed, Burn, Freeze, Curse, Stun, Mark). Helpful statuses are not cleansed.
- **Fresh:** a status applied during its owner's own turn; it does not lose a turn at the end of that turn (today's rule).

## 3. Rules

### Applying

1. An ability's `ApplyStatusEffect` applies its application to each target it hits (or to the actor with `onSelf`), after the ability's damage on that target.
2. The condition is checked first: a `Perfect timing` application is skipped unless the action's attack timing is `Perfect`.
3. The chance is rolled next through `IRandomSource`; a failed roll applies nothing.
4. A dead target gets nothing.
5. If the unit has no instance of that status, it gains one with the application's turns and potency, 1 stack and the actor's side as its source side.
6. If it already has one:
   - `Refresh`: turns and potency become the larger of the old and new values;
   - `Stack`: stacks go up by 1 up to the maximum, turns become the larger of the old and new values, potency the larger of the old and new values.
7. The model emits `StatusApplied`, as today.

### Turn order

The GDD round flow: heroes' turn, then enemies' turn.

1. **Start of a side's turn:** every damage-over-time status on that side's living units deals its damage, unit by unit in slot order; then units that skip the turn are announced (`Stunned`).
2. **During the turn:** units act. On the enemy side, units with Act last act after all units without it; within each group the slot order stays.
3. **End of a side's turn:** every status on that side's living units loses 1 turn, unless it is fresh (it only stops being fresh). Statuses with 0 turns leave.

### Damage over time

- Damage per turn = potency × stacks.
- It is not an action: it cannot be blocked, gets no timing bonus and cannot crit.
- It gives Rage like a hit dealt by the source side, with the same Rage per damage (the Rage spec sets the amounts).
- It is multiplied by the owner's damage taken (so Freeze makes Bleed hurt more) and is soaked by Absorb damage like any other damage.
- A unit that dies from it dies as from any hit (`Died`, its statuses cleared).

### Absorb damage (Shield)

- An instance holds points equal to its potency × the caster's Healing stat, the same stat that scales heals, so Shields grow with the caster (for example with levels and equipment later).
- Incoming damage, after all multipliers and rounding, first lowers the points; only the rest reaches health.
- At 0 points the status leaves at once; otherwise it leaves when its turns run out.
- Reapplying refreshes: points become the larger of the old and new values, never the sum.

### Removal

- Turns run out (end of the owner's turn, see Turn order).
- Consumed on attack: after the owner's damaging ability resolves.
- Consumed by a critical hit: after the hit that crit the owner resolves (Mark).
- Absorb damage at 0 points.
- Cleanse (a new `AbilityEffect`, used by the Priestess): removes every harmful status from the target.
- Death clears all statuses, as today.

### Damage calculation (layer 7)

For one hit of a damaging ability:

1. Base = the effect's power.
2. × the attacker's damage stat: 1 + the sum of its Damage modifiers (Empower +25%, Curse −30%, so both together give −5%).
3. × the timing multiplier (`ActionTiming`: attack bonus, block reduction).
4. × a random variance within `BattleRules.damageVariance`.
5. Critical hit: rolled against the attacker's crit chance (base + modifiers, so Mark adds +50 points); a crit multiplies by its crit damage (1.5).
6. × the target's damage taken: 1 + the sum of its DamageTaken modifiers (Freeze +30%).
7. Round once to a whole number.
8. Absorb damage soaks what it can; the rest is subtracted from health.

Modifiers of the same stat add up; there is no cap except that a multiplier never goes below 0.

## 4. Edge cases

- **Taunt and Stun together:** a stunned unit still forces targeting; Taunt does not need the owner to act.
- **Several taunting units:** the attacker may pick any of them, as today.
- **Stun during the owner's own turn** (from an ally or itself): it is fresh, so it does not run out at the end of that turn and the owner still skips its next turn.
- **Freeze on a hero:** only the damage taken part matters; heroes act in the order the player chooses.
- **Several Act last enemies:** they keep their slot order among themselves, after everyone else.
- **Elite with two actions and Act last:** both actions move to the end; each still gets its own block.
- **A status applied to a unit that dies from the same hit:** nothing is applied (the target is dead).
- **Damage that kills through a Shield:** the Shield soaks first, so only the rest can kill.
- **Waves:** statuses on heroes stay from one wave to the next, like health; the leaving wave's enemies take theirs with them.
- **Bleed and Burn on the same unit:** both tick, each with its own potency.
- **Empower and Curse on the same unit:** they add up (+25% − 30% = −5%).
- **Bosses:** they follow the same rules as any unit, so a boss can be stunned and skips its turn.
- **Mark and a multi-hit ability:** the first critical hit removes Mark after that hit resolves; later hits of the same ability roll without it.
- **Damage over time that kills:** the source side still gains its Rage for the damage dealt.
- **Perfect-only applications on enemy actions:** enemies have no attack timing, so their `Perfect timing` applications never happen; enemies use `Always`.

## 5. Connections

- **Timing:** the action's attack timing feeds the `Perfect timing` condition; the timing multiplier is step 3 of the damage calculation ([timing](timing.md)).
- **Rage:** damage over time gives Rage to its source side like a hit; damage soaked by a Shield gives no Rage to either side. The Rage spec sets the amounts.
- **Enemy behaviour:** Act last changes the order of the enemy turn (`EnemyTurn`); brains read statuses to pick targets and actions (the enemy behaviour spec).
- **Laws of the lands:** Noon Heat spares units with an Absorb damage status ("without a Shield").
- **Presentation:** `StatusApplied`, `StatusTicked` and the new events below drive popups, icons and VFX.
- **Content:** each status is a `StatusDefinition` asset with a `StatusVisuals` asset, seeded by `StatusCatalog`.

## 6. Feedback

- **Icon row** on every unit: one icon per status with its turns; stacking statuses show their stacks.
- **Shield:** the icon shows the points left; a hit on a Shield shows the soaked amount in the Shield colour, and a breaking Shield gets its own short effect and sound.
- **Damage over time:** a number in the status colour at the start of the turn, as today.
- **Act last:** the Frozen enemy's icon makes it clear it will act last.
- **Perfect-only applications:** the extra status pops up with the "PERFECT!" result, so the player learns the reward.
- Never colour alone: every status icon has its own shape.

## 7. Starting numbers

Values live in the status assets (`StatusDefinition`) and in each ability's `StatusApplication`; these are first guesses for balancing.

| Status | Behaviours | Harmful | Stacking | Typical application |
|---|---|---|---|---|
| Bleed | damage over time | yes | Refresh | 3 turns, 6 per turn |
| Burn | damage over time | yes | Stack, max 5 | 2 turns, 4 per turn per stack |
| Freeze | damage taken +30%, act last | yes | Refresh | 1 turn |
| Curse | damage −30% | yes | Refresh | 2 turns |
| Shield | absorb damage | no | Refresh | 2 turns, 30 base points |
| Taunt | force targeting | no | Refresh | 1 turn |
| Stun | skip turn | yes | Refresh | 1 turn |
| Mark | crit chance +50 points, consumed by a critical hit | yes | Refresh | 2 turns or the first crit |
| Empower | damage +25% | no | Refresh | 1 turn |

The prototype's Poison and Weaken stay for the bird run until the heroes are reworked (#26); Shield becomes the absorbing Shield above, and Empower changes to +25% for its turns instead of +50% consumed on attack.

## 8. Done when

These become EditMode tests on the model:
- a `Refresh` status reapplied keeps one instance with the larger turns and potency;
- Burn stacks up to 5 and deals potency × stacks per turn;
- damage over time ticks at the start of its owner's side's turn, cannot crit, gets no timing bonus and gives Rage to its source side;
- a fresh status does not lose a turn at the end of the turn it was applied in;
- Shield points are its potency × the caster's Healing stat; it soaks damage up to its points, leaves at 0, and reapplying never adds points;
- Shield soaks damage over time;
- Freeze adds 30% damage taken, and a Frozen enemy acts after the others, keeping slot order;
- Empower and Curse on the same attacker add up to −5%;
- Mark adds 50 points of crit chance and leaves after the owner's first critical hit taken;
- a `Perfect timing` application happens only with a perfect attack, and never on enemy actions;
- cleanse removes harmful statuses only;
- a stunned taunting unit still forces targeting;
- a boss can be stunned;
- heroes keep their statuses between waves.

## 9. Open questions

- How character levels (M2) raise the Healing stat, and so Shield points.
- Whether a status can ever be stolen or copied to another unit (a building block to add when an ability needs it).
