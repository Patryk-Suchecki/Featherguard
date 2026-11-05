# Contributing to Featherguard

How the two of us work on this repo. The rules for Claude are in [`CLAUDE.md`](../CLAUDE.md) and the game itself is described in [`GAME_DESIGN.md`](GAME_DESIGN.md).

## 1. First-time setup

1. **Unity:** install Unity Hub and editor **6000.6.2f1**. It must be exactly this version (Hub → Installs → Install Editor → Archive if it is not listed). Add **Android Build Support** if you want to build an APK later.
2. **Git and Git LFS:** install Git (https://git-scm.com) with Git LFS, then run once:
   ```bash
   git lfs install
   ```
3. **Repository:** accept the GitHub invitation and clone the repo:
   ```bash
   git clone https://github.com/Patryk-Suchecki/Featherguard.git
   ```
   ```bash
   cd Featherguard
   ```
4. **Unity Smart Merge:** set it up once in this clone. Replace the path with your own editor location (in Unity Hub: Installs → ⋮ → Show in Explorer):
   ```bash
   git config merge.unityyamlmerge.name "Unity SmartMerge (UnityYAMLMerge)"
   ```
   ```bash
   git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.2f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -h -p --force %O %B %A %A"
   ```
   ```bash
   git config merge.unityyamlmerge.recursive binary
   ```
   ```bash
   git config lfs.https://github.com/Patryk-Suchecki/Featherguard.git/info/lfs.locksverify true
   ```
   Patryk's editor lives in `E:/6000.6.2f1/Editor`, so his path is `E:/6000.6.2f1/Editor/Data/Tools/UnityYAMLMerge.exe`.
5. **Project:** add the folder in Unity Hub (Add → Add project from disk), pick editor 6000.6.2f1 and wait for the first import (a few minutes).
6. **Scene:** open `Assets/Scenes/Battle.unity` and press Play.
7. **Tests:** Window → General → Test Runner → EditMode → Run All. Everything must be green.
8. **Claude Code:** if you use it, start it in the repo folder. It loads `CLAUDE.md` and `.claude/settings.json` automatically.

## 2. Daily workflow

1. **Task:** pick an issue from the board (*To do* column), assign yourself and move it to *In progress*.
2. **Branch:** create it from a fresh `main`:
   ```bash
   git switch main
   ```
   ```bash
   git pull
   ```
   ```bash
   git switch -c feature/12-slot-machine-sounds
   ```
   Branch names are `<type>/<issue>-<short-task>`, where type is `feature`, `fix`, `balance`, `art` or `chore`.
3. **Work:**
   - Keep changes small. Change balance in the assets under `Assets/Data`, not in code.
   - If you change game content or rules, update `docs/GAME_DESIGN.md` too.
   - Commit messages are the issue number plus what the commit does, e.g. `#12 Add jackpot sound to slot machine`.
   - No AI attribution: no `Co-Authored-By` lines and no "Generated with Claude Code". Claude Code has it turned off in `.claude/settings.json`.
4. **Before pushing:** run the EditMode tests and play the game for a moment in the editor.
5. **Push and PR:**
   ```bash
   git push -u origin feature/12-slot-machine-sounds
   ```
   Open a Pull Request on GitHub titled `#12 Slot machine sounds`. The template starts with `Closes #12`. Move the issue to *Review*.
6. **Merge:** the other person reviews and approves the PR, then use *Squash and merge*. The issue moves to *Done*.

### Things to avoid

- **One person at a time in `Battle.unity`.** If you need to change it, say so in the issue.
- **Noise after "Featherguard > Rebuild Battle Scene".** Rebuild regenerates the scene. If your PR is not about the scene, discard the changes to `Battle.unity`, `BattlePostFX.asset` and `ProjectSettings`. Close the scene in Unity first (File → New Scene) so Unity does not ask to reload it:
  ```bash
  git restore Assets/Scenes/Battle.unity Assets/Settings/BattlePostFX.asset ProjectSettings
  ```
- **Binaries (fbx, blend, png, wav, ogg) are in LFS and lockable**, meaning read-only until you lock them:
  - Lock a file before editing or regenerating it:
    ```bash
    git lfs lock Assets/Art/Characters/Knight.fbx
    ```
  - Unlock it after the merge:
    ```bash
    git lfs unlock Assets/Art/Characters/Knight.fbx
    ```
  - `git lfs locks` lists the current locks.
- **Regenerating art or sound (Blender scripts, icons, SFX) is its own PR**, with no gameplay code changes.
- **Never upgrade Unity or packages in a regular PR.** We do that together, in a dedicated PR.

## 3. GitHub settings (for Patryk to click)

- [ ] **Invite the collaborator:** Settings → Collaborators → Add people.
- [ ] **Merge options:** Settings → General → Pull Requests: enable "Allow squash merging" and "Automatically delete head branches".
- [ ] **Protect `main`:** Settings → Branches → add a ruleset (or rule) for `main` with:
  - Require a pull request before merging, 1 approval;
  - Block force pushes;
  - Restrict deletions.
- [ ] **Labels:** Issues → Labels: `gameplay`, `art`, `audio`, `UI`, `bug`, `balance`.
- [ ] **Milestone:** Issues → Milestones → New milestone, e.g. "Playable Android demo".
- [ ] **Board:** Projects → New project → Board with columns **Ideas / To do / In progress / Review / Done**, linked to the repository.
- [ ] **LFS:** Settings → General → Archives: enable "Include Git LFS objects in archives". Check the Git LFS quota of the free plan in your account settings (Billing and licensing), because binaries grow with every regeneration.
