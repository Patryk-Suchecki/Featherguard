# Art Pipeline

How a hand-made 3D character goes from an idea to the game. How it should look (proportions, heights by role, fur, texture painting, colours, VFX) is in [`STYLE_GUIDE.md`](STYLE_GUIDE.md); this file covers the technical rules every character follows, so Unity and the battle code can use it without special cases.

## Steps

1. **Character sheet:** copy [`design/characters/TEMPLATE.md`](design/characters/TEMPLATE.md) to `docs/design/characters/<Name>.md` and fill in role, look, abilities, animations, VFX and sounds.
2. **Concept:** a concept sheet in `Art/Concepts/<Name>.png` (front and side silhouette, colours, key features, battle pose).
3. **Model** in `Art/Blender/Characters/<Name>.blend`.
4. **Texture:** paint the albedo texture; keep the paint source in `Art/Textures/<Name>.kra` (or `.blend` texture paint).
5. **Rig and animate** in the same `.blend`.
6. **Export** with the export script (below) into `Assets/Art/Characters`.
7. **In Unity:** the importer applies the clip settings automatically. Adding the character to the game (`UnitCatalog`, `UnitDefinition`, encounter) is a separate task.
8. **Tick the asset checklist** in the character sheet.

## Names and folders

The character name is one English word or PascalCase phrase, the same everywhere: `Bies`, `Wolf`, `LadyMidday`.

| What | Where |
|---|---|
| Character sheet | `docs/design/characters/<Name>.md` |
| Concept | `Art/Concepts/<Name>.png` |
| Blender source | `Art/Blender/Characters/<Name>.blend` |
| Texture source | `Art/Textures/<Name>.kra` |
| Exported model | `Assets/Art/Characters/<Name>.fbx` |
| Clip data | `Assets/Art/Characters/<Name>.clips.json` (written by the export script) |
| Texture | `Assets/Art/Characters/Textures/<Name>_Albedo.png` (written by the export script) |
| Portrait | `Assets/Art/UI/Portraits/<Name>.png` |

Inside the `.blend`:
- the armature object is `Rig`, the mesh object is `<Name>`;
- bones are lower-case English, with `.L` and `.R` suffixes for sides (`arm.L`, `leg.R`) so Blender can mirror them;
- the root bone is `root`, at the origin.

## Scale and orientation

- **Units:** 1 Blender unit = 1 metre. Model at the final in-game size; the unit's `modelScale` stays 1 and its `height` is the height the export script prints.
- **Up and front:** Z is up, the character faces −Y (you see its face in Blender's front view, numpad 1). The battle code turns heroes and enemies towards each other; never model a character facing sideways.
- **Origin:** the object origins and the root bone sit on the ground between the feet, at the world origin.
- **Apply transforms:** rotation and scale of the rig and the mesh are applied (Ctrl+A) before export.
- **Height:** the target height for each role is in the style guide.

## Budgets

Up to six characters are on screen on a phone at once.

| | Regular | Elite and boss |
|---|---|---|
| Triangles: aim for | 3,000–6,000 | 5,000–8,000 (the Bies has 5,444) |
| Triangles: never more than | 8,000 | 12,000 |
| Texture | 1024 × 1024 | 2048 × 2048 |

- Bones: up to 60, at most 4 bone weights per vertex.
- One material and one albedo texture per character (PNG, sRGB), so a character is one draw call; the export warns when more materials are used. No normal, roughness or metallic maps: shading comes from the toon shader.
- Weapons and other equipment will be separate models with their own material, attached to a hand bone in the game; their rules come with the equipment work.

## Rig

- One armature per file, with every mesh parented to it through an Armature modifier.
- Unity imports it as a Generic rig. Humanoid-style bone names are not required.
- Constraints and IK are fine while animating; the export bakes the result.
- Heroes and humanoid enemies can start from Rigify; quadrupeds need their own rig. Keep only the bones the animations need.

## Animation

- 30 fps.
- One Blender action per clip, named exactly as the code plays them:

| Clip | Loops | Typical length | Notes |
|---|---|---|---|
| `Idle` | yes | 40–60 frames | first and last pose identical |
| `AttackWindup` | yes | 8–16 frames | the held wind-up pose before the strike; the game loops it for a random time so the player cannot learn the block rhythm. Optional for now; required for enemies once blocking exists (#18, #19) |
| `Attack` | no | 20–30 frames | starts from the `AttackWindup` pose when there is one; needs the `OnHitFrame` marker |
| `Cast` | no | 25–35 frames | the special ability; needs the `OnHitFrame` marker |
| `Hit` | no | 10–15 frames | returns to the idle pose |
| `Death` | no | 25–40 frames | ends lying on the ground |
| `Victory` | yes | 30–60 frames | first and last pose identical |

- Set each action's frame range (Action → Manual Frame Range); the export uses it.
- **`OnHitFrame`:** in the Action editor, turn on Show Pose Markers and add a pose marker named `OnHitFrame` on the frame the blow lands or the spell fires. The game shows damage on that frame, so it must match the motion.
- Other actions in the file (tests, work in progress) are fine; the export leaves them out.
- Heroes will need more clips (support, Rage action, block). They are added to this table together with the code that plays them.

## Export

From the repository root:

```bash
blender -b Art/Blender/Characters/Bies.blend -P Art/Blender/export_character.py
```

`blender` is your Blender 5.x executable; use its full path if it is not on `PATH`. The script:
- runs only from the command line with `-b`, because it changes the open file while it works (in an open Blender a later Ctrl+S would save those changes);
- checks the rig, the mesh, the six clips and the `OnHitFrame` markers, and stops with a clear message if something is missing; `AttackWindup` is exported when it exists;
- warns about unapplied transforms (in any rotation mode), more than one material, a wrong frame rate, extra actions and budgets;
- leaves out preview outlines (a Solidify modifier with flipped normals or named `Outline`), because the toon shader draws the outline in the game;
- writes `<Name>.fbx` and `<Name>.clips.json` to `Assets/Art/Characters` and the material's image texture to `Textures/<Name>_Albedo.png`, linked from the FBX so Unity assigns it, and refuses to overwrite a file that is not locked;
- exports the material through its Principled BSDF, so a preview node setup (toon ramp, mix shaders) may stay in the `.blend`;
- prints the triangle count, bone count and height, measured on the first frame of `Idle`, the pose the unit stands in (use the height for the unit's `height`).

Pass a folder after `--` to export somewhere else for a test, for example `-- Temp/ExportTest`.

## Version control

- Every binary (`.blend`, `.fbx`, `.png`, `.kra`) is in Git LFS and lockable. Lock each existing file before you change it and unlock it after the merge, as [`CONTRIBUTING.md`](CONTRIBUTING.md) shows.
- A character is added or changed in its own `art/` PR, with no gameplay rule changes. Code that puts the character into the game comes in its own PR.
- Commit the `.blend`, the export, the texture and the concept together with their `.meta` files.

## Character workflow

What worked for the Bies, the first hand-made character. Follow it for every new character; the Wolf will add a quadruped section to the rig and animation steps.

### 1. Concept and references

- Make an orthographic sheet: front, side and back in one row, at one scale, on one ground line, in flat light and a neutral A-pose (arms about 45° from the body, hands open). A top view is optional and rarely consistent; use it only as a hint.
- Check it before modelling: horns, shoulders, knees and feet must sit at the same height in every view. Where the views disagree, take widths and heights from the front and depth and posture from the side.
- In Blender, crop the sheet into one image per view and add them as reference image empties in a `<Name>_Refs` collection: front on the XZ plane, side on the YZ plane, back facing +Y, scaled so the feet are at Z = 0 and the top matches the target height. Pack the images into the `.blend`.

### 2. Blockout and high-res sculpt

- Block out proportions with simple shapes (a skin-modifier skeleton works well) and compare them with the references in orthographic views before adding detail.
- Draw the direction of fur, hair or cloth on the model first, so the sculpt and the texture follow one flow.
- Sculpt one welded body mesh (voxel remesh, X symmetry on). Shape fur, hair and cloth as the style guide describes.
- Keep separate objects for parts that move or stay hard: lower jaw (origin at the hinge), teeth, eyes, claws or nails, horns and weapons.
- Create vertex groups per region (for example fur, bare skin, silhouette tufts); they are masks for sculpting and for the texture.

### 3. Retopology to the game mesh

- Aim for the triangle range in the budget table, well under the limit.
- Quads with clean edge loops around shoulders, elbows, wrists, hips, knees, ankles, the neck and the corners of the mouth. An even automatic quad remesh needs these loops added by hand before skinning.
- Exactly symmetric: build one half with a Mirror modifier and apply it.
- Silhouette features (horns, tufts, ears, claws) stay real geometry; small detail goes into the texture.
- Join everything into one mesh named `<Name>`, including the jaw and horns, so the character is one skinned mesh with one material.

### 4. UVs

- One 0–1 atlas for the whole character, no overlaps (mirrored halves get their own islands).
- Even texel density, about 500 px per metre on a 2048 texture; give the head 1.3× and hidden areas less.
- Island margin at least 8 px on 2048 (4 px on 1024).
- Hide seams from the battle camera: along the back of the spine, under the arms, on the inner side of arms and legs, under the jaw and under the mane or hair.

### 5. Baking and texture

- Bake from the sculpt onto the game mesh: ambient occlusion and a cavity mask, plus region masks from the vertex groups. They are guides for the paint, not separate maps in the game.
- Start the base colour from flat colours per region and multiply the bakes in lightly.
- Paint by hand on top, following the texture painting rules of the style guide.
- Connect the albedo to the Base Color of the material's Principled BSDF; the export saves it as `Assets/Art/Characters/Textures/<Name>_Albedo.png`.

### 6. Rig and skinning (biped)

- Layout:
  - `root` at the origin; it does not deform and is never animated.
  - A centre-of-gravity control.
  - `pelvis`, three spine bones, `chest`, two neck bones and `head`.
  - `jaw`, with its head at the hinge.
  - Ears, tail and hair or mane chains of one or two bones.
  - `clavicle`, `upper_arm`, `forearm` and `hand` per side, and two bones per finger.
  - `thigh`, `shin` and `foot` per side; digitigrade legs add a `metatarsal`.
- The 60-bone limit counts every bone, including controls and helpers. Give each helper one job or merge it: a single helper per leg can be both the IK target (its head) and the aim point (its tail).
- IK controls for the feet with knee poles. Arms need an FK/IK switch, FK for swings and IK only for planted hands (see Animation).
- Elbows and knees are hinges: lock the other two axes in the IK settings and set IK limits there. Limit Rotation constraints alone do not affect the IK solver.
- Skinning:
  - Start from automatic weights.
  - Put rigid parts (horns, teeth, eyes, claws) 100% on one bone, and the lower jaw 100% on `jaw`.
  - Limit to 4 influences per vertex, remove weights under 0.01 and normalize.
  - Test extreme poses (arms up, deep crouch, head turn, jaw open) before animating.
- Quadrupeds such as the Wolf need their own section here.

### 7. Animation

- Block key poses first, then breakdowns: snappy pose-to-pose with clear anticipation, a fast action and a follow-through that settles.
- Attack, Cast and Hit start and end exactly on Idle frame 1. Clips stay in place, with the feet planted through IK; the game moves the unit.
- Check every clip from the battle camera's side. Enemies face left, so you see them from +X and their near arm is `.L`; heroes face right, you see them from −X and their near arm is `.R`. Attack and point with the near arm.
- Attack:
  - The wind-up is slower than the strike, at most half its speed. Load the weight into the legs and twist the torso, elbow first.
  - Hold the wind-up as a moving hold; it is the block cue. Make it its own looping `AttackWindup` clip, so the game can stretch it to a random length.
  - Strike on an arc led by the hips, then the chest, then the arm.
  - Put `OnHitFrame` on the contact frame, right after the fastest frame.
  - Follow through past the target, then hold 2–3 frames for the hitstop.
- Wrists and elbows:
  - Keep wrists within about 60° and elbows between 30° and 165°.
  - A hand that copies the rotation of an unanimated IK control breaks the wrist as soon as the arm moves. Animate arms in FK, or key the IK control's rotation too.
  - Check that no hand or claw goes inside the body.

### 8. What stays in the `.blend`

- Keep:
  - the sculpt and its parts,
  - the bake source,
  - the game mesh `<Name>` with `Rig`,
  - the six clips,
  - the references,
  - the fur or hair flow guide.
- Name temporary backups with a `_Pre` suffix (`Attack_PreRebuild`, `Body_PreRig`) and delete them when the step is done.
- Purge orphan data (File → Clean Up) before committing.
- The `Outline` Solidify preview modifier may stay; the export leaves it out.

### 9. Unity import

- After exporting, open the project in Unity (or run it in batch mode) so the `.meta` files for the FBX, the clip data and the texture exist.
- Commit the `.meta` files together with the assets.

### Open points

- Portraits for hand-made characters are not covered yet (the generated ones come from `render_portraits.py`).
- The quadruped rig and animation rules come with the Wolf.
