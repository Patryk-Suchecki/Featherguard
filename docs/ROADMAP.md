# Production Roadmap

How we get from the current build to a finished game. The target design is [`GDD.md`](GDD.md); [`GAME_DESIGN.md`](GAME_DESIGN.md) describes the current build. This file covers the whole production, not only code: design, programming, UI/UX, art, animation, VFX, audio, narrative, QA and release.

Change this file through a PR both developers agree on. Record scope decisions in the decisions table at the end.

## How we track work

| Level | On GitHub | What it is | Example |
|---|---|---|---|
| Milestone | Milestone | a production phase with exit criteria below | `M1 Vertical slice` |
| Epic | an issue titled `Epic: <name>` with sub-issues | one feature or one content package | `Epic: Timing QTE`, `Epic: Rye Fields content` |
| Task | an issue from the Task template | one issue → one branch → one small PR | `Add attack timing ring to BattleModel` |

- Every task belongs to an epic (as a sub-issue) and to a milestone.
- Epics are of two kinds: **feature epics** (a system across code, UI, VFX and audio) and **content epics** (a land, a hero batch, a music set).
- The discipline is shown by labels: `gameplay`, `UI`, `art`, `audio`, `balance`, `bug`.
- Split work between the two of us by workstream (for example combat and UI vs. art and content), not by layer, so we rarely touch the same files or `Battle.unity`.
- Big content is produced in batches, after the vertical slice has measured how long one unit really takes.

## Phases

| Milestone | Goal | Done when |
|---|---|---|
| **M0 Pre-production** | Make the decisions that block everything else | All decisions below are recorded; a style guide is in the repo; one test character goes through the full art pipeline (model, rig, animation, shader, in Unity); the combat prototype with QTE feels good in a playtest; the MVP scope is fixed |
| **M1 Vertical slice** | One land in final quality, every discipline together | Stronghold → land map → four Rye Fields circles → Lady Midday is playable on a phone with final art, animation, VFX, music, SFX, UI and texts; someone outside the team can play it without explanation; we have measured how long each kind of asset takes |
| **M2 Alpha** | Every system works | All MVP lands can be finished (placeholder art is fine); roster, party pick, levels, equipment, forge, potions, saving, rumours and settings work; tutorial exists |
| **M3 Beta** | Content complete | All MVP art, animation, VFX, music, SFX and texts are in; balance pass done; only bug fixes and polish after this |
| **M4 Release** | Ship it | Performance target met on low-end devices; store builds signed; store pages, rating and privacy policy done; release checklist passed |

## Pre-production decisions

These block production. Each gets an Idea issue in M0 and is recorded in the table at the end.

1. ~~**2D or 3D characters.**~~ Decided: 3D, in the art style described below.
2. **Where art comes from.** Script-generated, bought asset packs, a freelancer, or a mix. Decide per asset type (characters, environments, icons, portraits).
3. **Where music comes from.** `SfxSynth` covers simple SFX only. Music needs a composer, licensed tracks or another source, and its licence must allow commercial use.
4. **The gambling layer.** Whether the current between-battle gambling stays. It affects the run structure, the age rating and how free-to-play monetisation may work (in-game currency only stays the rule while it exists).
5. **MVP scope.** The GDD's full scope (15 heroes, 20 enemies, 5 bosses, 5 lands) is too big for a first release by two people. Proposed MVP: 6 heroes, 3 lands, 3 bosses, the rest in updates.
6. **Monetisation and platforms.** Free-to-play or paid, ads or not, Android first or Android and iOS together, when Steam.
7. **Working title** and whether the repo and namespaces keep `Featherguard`.

## Art style

Everything is 3D, rendered so it reads like a hand-drawn illustrated card game. The style must stay our own: never copy another game's characters, shapes or art.

- **Silhouettes first.** Chunky, exaggerated shapes: big heads, hands and weapons, thin limbs, strong poses. Every hero and enemy is recognisable as a black silhouette at phone size.
- **Bold dark outlines.** Thick, slightly uneven outlines around every character and prop (inverted hull or a post-process edge pass), thinner lines inside for folds and details.
- **Painted, not realistic.** Low-to-mid poly models with hand-painted textures: visible brush strokes, colour shifts instead of fine detail, no photo textures and no PBR shine.
- **Simple banded shading.** Toon shading with two or three flat bands, cool dark shadows and a single warm key light; no soft realistic gradients.
- **Colour.** Dark, muted, desaturated backgrounds; characters in a few saturated colours so they pop. Each land has its own limited palette, enemies lean colder and stranger than heroes.
- **Backdrops like paintings.** Battle environments are a shallow stage seen from the side: a few layered, painted 3D planes and props with depth fog, not a full explorable world.
- **Grim but playful.** Folklore horror with a storybook, slightly comic edge: creepy monsters, readable expressions, nothing gory.
- **Animation.** Snappy and pose-to-pose: strong anticipation, fast hits, short holds on key poses, squash and stretch on impacts. Skeletal 3D animation in Unity.
- **UI and VFX match.** UI frames, icons and effects use the same outlines, flat colour and painted texture; VFX are bold shapes (stylised flames, shards, swirls) rather than realistic particles.

The style guide in M0 turns this into concrete references, palettes and shader settings. The current low-poly Blender pipeline is the starting point and is reworked towards this look in M0.

## Workstreams

Each workstream becomes one or more epics. Not every item is needed for M1; the phase in brackets is when it must be done.

### Design and balance
- Combat rules: QTE windows, Rage, statuses, archetypes (M0 prototype, M1 final).
- Difficulty curve, threat levels and circle structure (M1 for Rye Fields, M2 for all).
- Economy: gold, materials, forge costs, rewards, Daring bonus (M2).
- Mission scripts and encounters per circle; challenge rules (M1 for Rye Fields, M3 for all).
- Balance passes with simulations in EditMode tests (every phase).

### Programming
- Battle model: QTE results, party Rage, new statuses, enemy brains per archetype, boss mechanics (M1).
- Run and progression: Stronghold, land map, circles, chapters, unlocks, character levels (M1–M2).
- Roster and party pick, equipment, sets, forge, potions (M2).
- Rumours from the Road, Laws of the lands (M2).
- Save system and data migration between versions (M2).
- Content pipeline: catalogs and importers for new heroes, enemies and items (M1).
- Builds: Android, then iOS and Steam; signing and versioning (M1 Android, M4 all).

### UI/UX
- Screen flow and wireframes for every screen (M0).
- Screens: Stronghold, land map, party pick, battle HUD, QTE indicators, victory and defeat, roster, equipment, forge, Wise Woman's hut, tavern, bestiary, settings, pause (M1 for the slice path, M2 for the rest).
- Tutorial and onboarding, teaching QTE first (M2).
- Fonts, icon set, UI animation and UI sounds (M1).
- Accessibility: auto-timing, shape and sound cues besides colour, fast-forward ×2 and ×3, readable text sizes (M2).
- Touch and mouse on one pointer, safe areas on notched phones (M1).

### Art
- Style guide: palettes per land, outline and shading rules, character proportions, reference sheet (M0).
- Shader and rendering: outline, painterly texture, lighting and post-processing per land (M0 test, M1 final).
- Heroes: model or sprite set, rig, portrait (M1: 3 heroes; M3: MVP roster).
- Enemies and bosses: same as heroes (M1: Rye Fields; M3: MVP lands).
- Environments: battle backgrounds per land, the Stronghold, the land map (M1: Rye Fields, Stronghold, map).
- Items and icons: weapons, artifacts, materials, potions, status icons (M2–M3).
- Store art: app icon, key art, screenshots (M4).

### Animation
- Per character: idle, attack, support, Rage action, hit, block, death, victory (about 8 animations each).
- Boss-specific animations for their mechanics.
- Camera moves for Rage actions and boss moments.

### VFX
- One effect per ability (attack, support, Rage) for every hero and enemy.
- Status effects: Bleed, Burn, Freeze, Curse, Shield, Taunt, Stun, Mark, Empower.
- QTE feedback: shrinking ring, good and perfect hits, BLOCK! flash.
- Hits, crits, deaths, Law effects per land, UI effects (rewards, level up).

### Audio
- Music: Stronghold, land map, battle per land, boss per land, victory and defeat stingers.
- SFX per ability, statuses, hits, blocks and the perfect-timing sound (part of the mechanic, M0).
- Ambient per land, UI sounds.
- Mixer setup, volume settings, ducking (M1).
- Voices: later, after release scope is clear.

### Narrative and text
- Story outline for the chapters and the Koschei legend (M0).
- Priest's chapter texts, tavern rumours, mission briefings (M1 for Rye Fields, M3 for all).
- Bestiary entries, hero bios, item and set names and descriptions (M2–M3).
- All in English; localization is decided later.

### QA and playtests
- EditMode tests for every rule and economy change (every PR).
- Playtests with people outside the team at the end of M0, M1, M2 and M3.
- Device tests on at least one low-end and one high-end phone (from M1).
- Performance budget: frame time, memory, build size (set in M1, met in M4).

### Release
- Store accounts, store pages, descriptions and screenshots, trailer.
- Age rating (IARC and PEGI) and privacy policy; check rules on gambling-like mechanics.
- Crash reporting and basic analytics, if we decide to collect any (privacy policy must cover it).
- Release checklist and versioning.

## Content scale

The full GDD implies roughly this much content. Use it to plan batches and to cut scope in M0.

| Content | Full GDD | Proposed MVP |
|---|---|---|
| Heroes | 15 | 6 |
| Enemies | 20 | 12 |
| Bosses | 5 | 3 |
| Lands (battle environments) | 5 + Stronghold + map | 3 + Stronghold + map |
| Character animations (≈ 8 each) | ≈ 320 | ≈ 170 |
| Abilities with VFX and SFX (3 per hero, 1–2 per enemy) | ≈ 80 | ≈ 40 |
| Music tracks | ≈ 14 | ≈ 10 |
| Story missions | 20 circles | 12 circles |

## Decisions

| Date | Decision | Agreed by |
|---|---|---|
| 2026-11-08 | Characters and environments are 3D, in the illustrated card-game look described in Art style | Aurel and Patryk |
