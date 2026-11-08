# Art Pipeline

How a hand-made 3D character goes from an idea to the game. The look is described in the art direction of [`GDD.md`](GDD.md); this file covers the technical rules every character follows, so Unity and the battle code can use it without special cases.

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
| Texture | `Assets/Art/Characters/Textures/<Name>_Albedo.png` |
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

Target heights, so characters read well next to each other:

| Character | Height |
|---|---|
| Hero | ≈ 1.4 m |
| Regular enemy | ≈ 1.5 m (a quadruped is judged by its length and head height instead) |
| Elite | ≈ 2.0 m |
| Boss | ≈ 2.4 m |

## Budgets

Up to six characters are on screen on a phone at once.

| | Regular | Elite and boss |
|---|---|---|
| Triangles | ≤ 8,000 | ≤ 12,000 |
| Texture | 1024 × 1024 | 2048 × 2048 |

- Bones: up to 60, at most 4 bone weights per vertex.
- One material per character (two at most, for example for a separate weapon).
- One hand-painted albedo texture, PNG, sRGB. No normal, roughness or metallic maps: shading comes from the toon shader.

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
| `Attack` | no | 20–30 frames | needs the `OnHitFrame` marker |
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
- checks the rig, the mesh, the six clips and the `OnHitFrame` markers, and stops with a clear message if something is missing;
- warns about unapplied transforms, a wrong frame rate, extra actions and budgets;
- writes `<Name>.fbx` and `<Name>.clips.json` to `Assets/Art/Characters`, and refuses to overwrite a file that is not locked;
- prints the triangle count, bone count and height (use the height for the unit's `height`).

Pass a folder after `--` to export somewhere else for a test, for example `-- Temp/ExportTest`.

## Version control

- Every binary (`.blend`, `.fbx`, `.png`, `.kra`) is in Git LFS and lockable. Lock each existing file before you change it, and unlock it after the PR is merged:
  ```bash
  git lfs lock Assets/Art/Characters/Bies.fbx
  ```
- A character is added or changed in its own `art/` PR, with no gameplay code. Code that uses the character comes in its own PR.
- Commit the `.blend`, the export, the texture and the concept together with their `.meta` files.
