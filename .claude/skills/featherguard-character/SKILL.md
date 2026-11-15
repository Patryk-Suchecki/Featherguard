---
name: featherguard-character
description: Use when making a hand-made Featherguard character (hero or enemy) - the concept prompt, checking the concept, setting up Blender reference images, modelling through the Blender connection, export to Unity and the art pull requests
---

# Making a Featherguard character

The technical rules (names, scale, budgets, rig, clips, export) are in `docs/ART_PIPELINE.md`, the look in `docs/STYLE_GUIDE.md`. Every character starts from its sheet in `docs/design/characters/<Name>.md` (see `featherguard-spec`). This skill covers the steps around them.

## 1. Concept prompt

Your developer makes the concept with an image generator. Give them one prompt in English with:
- **Layout:** an orthographic turnaround at one scale on one ground line: side view (enemies face left), front view and back view, a neutral pose; below them, the battle pose from the sheet if it has one. Flat even light, plain light grey background, no text, no labels.
- **Body:** the silhouette and proportions from the sheet, the three shapes that carry it, and the sizes in metres.
- **Fur, hair, cloth:** big hard-edged clumps following one flow direction, jagged only where they build the silhouette; no thin strands.
- **Colours:** the hex values from the sheet, and which colours are the only saturated ones.
- **Style:** stylised hand-painted fantasy game art, bold readable shapes, flat colour regions with soft shading, no photorealism, no specular highlights, no gear the sheet does not name, an original design.

For a variant of the same character (another colouring, a possessed version), have them edit the chosen image instead of generating a new one, so every shape stays the same; often a second albedo of the same model is enough and no second concept is needed.

## 2. Check and add the concept

- Before modelling, check that ears, horns, shoulders and feet sit at the same height in every view, and that the scale matches the sheet. Note what the model should do differently (for example bigger hands or paws per the style guide, a closed jaw as a separate part) in the PR rather than asking for a new image.
- Save it as `Art/Concepts/<Name>.png` (PNG, LFS). An image from the generator in another format (for example `.webp`) converts with Blender headless:
  ```bash
  "<blender.exe>" -b --factory-startup --python-expr "import bpy; i=bpy.data.images.load(r'<in.webp>'); i.filepath_raw=r'<out.png>'; i.file_format='PNG'; i.save()"
  ```
- Open its own `art/<name>-concept` PR, `Part of #<issue>`, label `art`. Never write how the concept was made.

## 3. Blender reference images

`scripts/make_refs.py` in this folder cuts the views out of the concept sheet and places them as packed reference image empties in a new `.blend`, as `ART_PIPELINE.md` asks (front on XZ, side on YZ facing −Y, back facing +Y, feet on Z = 0, origin between the feet, visible in orthographic views only):

```bash
"<blender.exe>" -b --factory-startup --python .claude/skills/featherguard-character/scripts/make_refs.py -- --image Art/Concepts/<Name>.png --blend Art/Blender/Characters/<Name>.blend --name <Name> --length 1.6
```

- Scale by a measurement from the sheet: `--length <metres>` (the side view from end to end, good for quadrupeds) or `--height <metres>` (the tallest point in the side view, good for upright characters).
- The script expects three views in the top row and an optional extra pose below, each on a thin dark ground line; it prints what it measured. Compare the printed heights of the views: they should agree within a few per cent.
- It refuses to overwrite an existing `.blend`. Never overwrite a hand-made `.blend` from a script.
- Tell the developer the references show only in orthographic views (numpad 1, 3 and Ctrl+numpad 1).

## 4. Modelling through the Blender connection

- Check the connection first (`get_addon_status`). If Blender is not connected, ask the developer to open the `.blend`, press N, open the MCP tab and click Connect.
- Follow `ART_PIPELINE.md` steps 2–5 (blockout, fur flow, sculpt with separate jaw, teeth, eyes and claws, retopology within the budget, UVs, bake and flat-colour albedo) and show the developer `look` images at each milestone: the blockout before sculpting, the game mesh before UVs, the textured model before committing.
- Ask before irreversible steps (applying a remesh over the sculpt, deleting a blockout).
- Report the triangle count, the size and the height with `get_scene_info`, and compare them with the budget and the sheet.
- When the colours of the result differ from the sheet (they often do once the concept is projected), update the sheet's colour table in the same art PR, so the sheet keeps describing the character.

## 5. Rig, animation and export

- Rig and clips follow `ART_PIPELINE.md` steps 6–7; a new kind of body (the first quadruped, the first flyer) adds its section to `ART_PIPELINE.md` in the same PR.
- Export only with the export script, from the command line:
  ```bash
  "<blender.exe>" -b Art/Blender/Characters/<Name>.blend -P Art/Blender/export_character.py
  ```
  Use the height it prints for the unit's `UnitVisuals.height`.
- Lock existing binaries before changing them (`git lfs lock`), open the model and the rig as `art/` PRs with no gameplay changes, and bring the character into the game (`UnitCatalog`, `UnitVisuals`, an encounter) in its own feature PR (see `featherguard-task`).
- Tick the asset checklist in the character's sheet as each step lands.
