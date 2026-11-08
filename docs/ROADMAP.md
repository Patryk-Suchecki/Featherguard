# Production Roadmap

How we get from the current build to a finished game. The target design is [`GDD.md`](GDD.md); [`GAME_DESIGN.md`](GAME_DESIGN.md) describes the current build. This file covers the whole production, not only code: design, programming, UI/UX, art, animation, VFX, audio, narrative, QA and release.

Change this file through a PR both developers agree on. Record scope decisions in the decisions table at the end.

## How we track work

| Level | On GitHub | What it is | Example |
|---|---|---|---|
| Milestone | Milestone | a production phase with exit criteria below | `M1 Vertical slice` |
| Epic | an issue titled `Epic: <name>` with sub-issues | one feature or one content package | `Epic: Timing combat prototype`, `Epic: Rye Fields content` |
| Task | an issue from the Task template | one issue → one branch → one small PR | `Add attack timing to the battle model` |

- Every task belongs to an epic (as a sub-issue) and to a milestone.
- Epics are of two kinds: **feature epics** (a system across code, UI, VFX and audio) and **content epics** (a land, a hero batch, a music set).
- The discipline is shown by labels: `gameplay`, `UI`, `art`, `audio`, `balance`, `bug`.
- We make all art, animation, VFX, music and sound ourselves. Art and audio work is planned as tasks like code.
- Work is split by assigning tasks. Pick tasks by workstream (for example combat and UI vs. art and content), not by layer, so we rarely touch the same files or `Battle.unity`.
- Epics of the next milestone are broken into tasks at the end of the current one. Big content is produced in batches, after the vertical slice has measured how long one unit really takes.

## Scope

- **Platform:** Android only, landscape.
- **Business model:** free, with no ads and no in-app purchases.
- **Language:** English only.
- **MVP content:** the 6 heroes of the GDD starting roster, 3 lands and 3 bosses. Which lands is decided later; the vertical slice uses the Rye Fields.
- **No gambling.** The target design has no gambling. The bird prototype and its gambling rewards ([`GAME_DESIGN.md`](GAME_DESIGN.md)) stay in the build only until the run is reworked in M1.
- **Title:** stays `Featherguard` for now; the final title is decided before release.

## Phases and schedule

Capacity: about 10 hours a week each, 20 hours a week together. Dates are targets; we review them at the end of every milestone, first after M1 has measured real production speed.

| Milestone | Target | Goal | Done when |
|---|---|---|---|
| **M0 Pre-production** | 2026-12-20 | Settle the style, the pipelines and how combat feels | Style guide is in the repo; one test hero goes through the full art pipeline (model, painted texture, rig, animations, toon shader, in Unity); one music track and the perfect-timing sound are made with our audio workflow; the timing combat prototype feels good; all MVP screens are wireframed |
| **M1 Vertical slice** | 2027-03-14 | One land in final quality, every discipline together | Stronghold → land map → the Rye Fields story missions, side activities and Lady Midday are playable on an Android phone with final art, animation, VFX, music, SFX, UI and texts; we know how long each kind of asset takes |
| **M2 Alpha** | 2027-06-06 | Every system works | All 3 MVP lands can be finished (placeholder art is fine); roster, party pick, levels, equipment, forge, potions, saving, rumours and settings work; tutorial exists |
| **M3 Beta** | 2027-09-26 | Content complete | All MVP art, animation, VFX, music, SFX and texts are in; balance pass done; final title decided; only bug fixes and polish after this |
| **M4 Release** | 2027-11-07 | Ship it on Google Play | Runs smoothly on a mid-range Android phone; signed release build; store page, content rating and privacy policy done |

## Art style

Everything is 3D, rendered so it reads like a hand-drawn illustrated card game. The style must stay our own: never copy another game's characters, shapes or art.

- **Silhouettes first.** Chunky, exaggerated shapes: big heads, hands and weapons, thin limbs, strong poses. Every hero and enemy is recognisable as a black silhouette at phone size.
- **Bold dark outlines.** Thick, slightly uneven outlines around every character and prop (inverted hull or a post-process edge pass), thinner lines inside for folds and details.
- **Painted, not realistic.** Low-to-mid poly models with hand-painted textures: visible brush strokes, colour shifts instead of fine detail, no photo textures and no PBR shine.
- **Simple banded shading.** Toon shading with two or three flat bands, cool dark shadows and a single warm key light; no soft realistic gradients.
- **Colour.** Dark, muted, desaturated backgrounds; characters in a few saturated colours so they pop. Each land has its own limited palette, enemies lean colder and stranger than heroes.
- **Backdrops like paintings.** Battle environments are a shallow stage seen from the side at a slight angle: a few layered, painted 3D planes and props with depth fog, not a full explorable world.
- **Grim but playful.** Folklore horror with a storybook, slightly comic edge: creepy monsters, readable expressions, nothing gory.
- **Animation.** Skeletal 3D animation, snappy and pose-to-pose: strong anticipation, fast hits, short holds on key poses, squash and stretch on impacts.
- **UI and VFX match.** UI frames, icons and effects use the same outlines, flat colour and painted texture; VFX are bold shapes (stylised flames, shards, swirls) rather than realistic particles.

The style guide in M0 turns this into concrete references, palettes and shader settings. The current low-poly Blender pipeline is the starting point and is reworked towards this look in M0.

## Workstreams

Each workstream becomes one or more epics. The phase in brackets is when it must be done.

### Design and balance
- Combat rules: timing, Rage, statuses, archetypes (M0 prototype, M1 final).
- Difficulty curve, threat levels, the order of story missions across lands and land unlocks (M1 for the Rye Fields, M2 for all).
- Economy: gold, materials, forge costs, rewards of missions and side activities, Daring bonus (M2).
- Story mission scripts and encounters; side activity pools per land (battle, wave battle, riddle, miniboss) (M1 for the Rye Fields, M3 for all).
- Balance passes with simulations in EditMode tests (every phase).

### Programming
- Battle model: timing results, party Rage, new statuses, enemy brains per archetype, boss mechanics (M0 prototype, M1).
- Progression: Stronghold, land map, story missions, side activities, land unlocks, character levels; replaces the bird run and its gambling rewards (M1–M2).
- Roster and party pick, equipment, sets, forge, potions (M2).
- Rumours from the Road, Laws of the lands (M2).
- Save system and data migration between versions (M2).
- Content pipeline: catalogs and importers for new heroes, enemies and items (M1).
- Android build: signing and versioning (M1 first build, M4 release).

### UI/UX
- Screen flow and wireframes for every MVP screen (M0).
- Screens: Stronghold, land map, party pick, battle HUD, timing indicators, victory and defeat, roster, equipment, forge, Wise Woman's hut, tavern, bestiary, settings, pause (M1 for the slice path, M2 for the rest).
- Tutorial and onboarding, teaching timing first (M2).
- Fonts, icon set, UI animation and UI sounds (M1).
- Accessibility: auto-timing, shape and sound cues besides colour, fast-forward ×2 and ×3, readable text sizes (M2).
- Touch input and safe areas on notched phones (M1).

### Art
- Style guide: references, palettes per land, outline and shading rules, character proportions (M0).
- Shader and rendering: outline, banded toon shading, painted texture look, lighting and post-processing per land (M0 test, M1 final).
- Heroes: model, painted texture, rig, portrait (M0: 1 test hero; M1: 3 heroes; M3: all 6).
- Enemies and bosses: same as heroes (M1: Rye Fields; M3: all MVP lands).
- Environments: battle backdrop per land, the Stronghold, the land map (M1: Rye Fields, Stronghold, map; M3: all).
- Items and icons: weapons, artifacts, materials, potions, status icons (M2–M3).
- Store art: app icon, feature graphic, screenshots (M4).

### Animation
- Per character: idle, attack, support, Rage action, hit, block, death, victory (about 8 animations each).
- Boss-specific animations for their mechanics.
- Camera moves for Rage actions and boss moments.

### VFX
- One effect per ability (attack, support, Rage) for every hero and enemy.
- Status effects: Bleed, Burn, Freeze, Curse, Shield, Taunt, Stun, Mark, Empower.
- Timing feedback: shrinking ring, good and perfect hits, BLOCK! flash.
- Hits, crits, deaths, Law effects per land, UI effects (rewards, level up).

### Audio
- Music: Stronghold, land map, battle per land, boss per land, victory and defeat stingers.
- SFX per ability, statuses, hits, blocks and the perfect-timing sound (part of the mechanic, M0).
- Ambient per land, UI sounds.
- Mixer setup, volume settings, ducking (M1).

### Narrative and text
- Main story outline and ending, once the GDD open questions are answered (M1).
- Priest's story mission texts, tavern rumours, riddles, mission briefings (M1 for the Rye Fields, M3 for all).
- Bestiary entries, hero bios, item and set names and descriptions (M2–M3).

### QA
- EditMode tests for every rule and economy change (every PR).
- Play the build on an Android phone at the end of every milestone.

### Release
- Google Play store page, description, screenshots.
- Content rating questionnaire and privacy policy (required by Google Play).
- Release checklist and versioning.

## Content scale

| Content | Full GDD | MVP |
|---|---|---|
| Heroes | 15 | 6 |
| Enemies | 20 | 12 |
| Bosses | 5 | 3 |
| Battle environments | 5 lands + Stronghold + map | 3 lands + Stronghold + map |
| Character animations (≈ 8 each) | ≈ 320 | ≈ 170 |
| Abilities with VFX and SFX (3 per hero, 1–2 per enemy) | ≈ 80 | ≈ 40 |
| Music tracks | ≈ 14 | ≈ 10 |
| Story missions | 25 (5 per land) | 15 |

## Later

Not planned now; revisit after M1: which 3 lands make the MVP, iOS and Steam, ads or purchases, localization, voices, extra modes (Kupala Night, Veles' Well, Buyan Island). The open design questions are listed at the end of [`GDD.md`](GDD.md).

## Decisions

| Date | Decision | Agreed by |
|---|---|---|
| 2026-11-08 | Characters, environments and animation are 3D, in the illustrated card-game look described in Art style | Aurel and Patryk |
| 2026-11-08 | We make all art, animation, VFX, music and sound ourselves, planned as tasks | Patryk; Aurel by approving #9 |
| 2026-11-08 | No gambling in the target design; the bird prototype keeps it until the run is reworked | Aurel and Patryk |
| 2026-11-08 | MVP: 6 heroes, 3 lands, 3 bosses; lands chosen later | Patryk; Aurel by approving #9 |
| 2026-11-08 | Free on Android only, no ads or purchases, English only | Patryk; Aurel by approving #9 |
| 2026-11-08 | The title stays `Featherguard` until decided before release | Patryk; Aurel by approving #9 |
| 2026-11-08 | The roster size stays as in the GDD for now | Patryk; Aurel by approving #9 |
| 2026-11-08 | Phases and target dates as in Phases and schedule, at about 10 hours a week each | Patryk; Aurel by approving #9 |
