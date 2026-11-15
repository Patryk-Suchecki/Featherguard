---
name: featherguard-spec
description: Use when writing or changing a Featherguard design spec or character sheet in docs/design (combat layers, other systems, a hero or enemy sheet) - draft with proposals, get the developer's decisions, then one spec per pull request
---

# Writing a Featherguard spec or character sheet

How specs are structured is in `docs/design/README.md`; character sheets follow `docs/design/characters/TEMPLATE.md`. This skill is the way of working that turns a draft into an agreed document.

## 1. Read first

- The GDD section the spec covers (`docs/GDD.md`) and the specs it builds on (the layers above it in `docs/design/README.md`).
- The code and data names it will map to (`StatusDefinition`, `AbilityEffect`, `BattleRules`, ...). Use the same names in the spec.
- For a character: the art rules in `docs/ART_PIPELINE.md` and `docs/STYLE_GUIDE.md` (heights by role, budgets, land palettes) and the sheets of characters it fights with.

## 2. Write only what is needed now

- Write a spec just before the code that needs it, and only for rules that are new or unclear. A rule the GDD already states clearly can go straight to code.
- A small rule (for example "Elites act twice") needs no spec.

## 3. Draft with proposals, then ask

1. Write the full draft in the template's order (the nine questions for a spec; role, look, abilities, animations, sounds, checklist and open questions for a sheet). Where the GDD is silent, write your proposal into the draft, not a gap.
2. Show the draft to your developer and list the **key decisions** as a short numbered list in their language: what you proposed and why, one line each. Put what you could not decide under open questions.
3. Apply the answers exactly. Answers often settle open questions: remove them from the list and write the rule where it belongs. If an answer changes another document (for example a character sheet that mentions the new rule), update that document in the same PR.
4. Keep design decisions with the developers. Do not settle a rule on your own because it is convenient for the code.

## 4. One spec per pull request

- Branch `chore/<name>-spec` or `chore/<name>-sheet`, docs only.
- Link the spec in its row of `docs/design/README.md`.
- PR description: `Closes #<issue>`, a summary of the rules, and a line "Agreed with <developer>: ..." listing the decisions they made.
- The code that implements a spec is a separate PR, usually stacked on the spec branch. When the code shows the spec was unclear or wrong, fix the spec on its own branch and merge it into the code branch, rather than letting them drift apart.

For a character's concept, Blender references and model, continue with `featherguard-character`.
