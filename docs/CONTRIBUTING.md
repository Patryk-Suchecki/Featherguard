# Contributing to Featherguard

How the two of us work on this repo, step by step. The rules themselves (architecture, code style, workflow, Unity pitfalls) are in [`CLAUDE.md`](../CLAUDE.md); this file does not repeat them. The game is described in [`GDD.md`](GDD.md), and the plan in [`ROADMAP.md`](ROADMAP.md).

## 1. First-time setup

1. **Unity:** install Unity Hub and editor **6000.6.2f1**. It must be exactly this version (Hub → Installs → Install Editor → Archive if it is not listed). Add **Android Build Support**.
2. **Git and Git LFS:** install Git (https://git-scm.com) with Git LFS, then run once:
   ```bash
   git lfs install
   ```
3. **GitHub CLI:** install it and log in once, so you and your Claude can create issues and pull requests from the terminal:
   ```bash
   winget install --id GitHub.cli
   ```
   Open a new terminal window, then:
   ```bash
   gh auth login --web --git-protocol https
   ```
   Copy the 8-character code, paste it in the browser and click **Authorize GitHub CLI**.
4. **Repository:** accept the GitHub invitation and clone the repo:
   ```bash
   git clone https://github.com/Patryk-Suchecki/Featherguard.git
   ```
   ```bash
   cd Featherguard
   ```
5. **Unity Smart Merge:** set it up once in this clone. Replace the path with your own editor location (in Unity Hub: Installs → ⋮ → Show in Explorer):
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
6. **Project:** add the folder in Unity Hub (Add → Add project from disk), pick editor 6000.6.2f1 and wait for the first import (a few minutes).
7. **Scene:** open `Assets/Scenes/Battle.unity` and press Play.
8. **Tests:** Window → General → Test Runner → EditMode → Run All. Everything must be green.
9. **Claude Code:** install it from https://claude.com/claude-code (desktop app or CLI) and open it in the repo folder. It loads `CLAUDE.md` and `.claude/settings.json` automatically. Your own Claude memory is not shared with the other developer: anything you both need goes into `CLAUDE.md` through a PR.
10. **Optional tools:**
    - **Blender 5.x** – for character and environment models in `Art/Blender`.
    - **Unity CLI (`unity`)** – lets Claude run tests and drive an open editor from the terminal. Without it, run tests in the Test Runner when Claude asks.
    - Your IDE (Rider or Visual Studio) picks up `.editorconfig`, which keeps formatting and naming the same for both of us.

## 2. How we track work

| Level | On GitHub | What it is |
|---|---|---|
| Milestone | Milestone | a production phase from [`ROADMAP.md`](ROADMAP.md), for example `M0 Pre-production` |
| Epic | an issue titled `Epic: <name>` with sub-issues | one feature (a system across code, UI, VFX and audio) or one content package (a land, a hero batch, a music set) |
| Task | an issue from the Task template | one issue → one branch → one small PR |

- Every task is a sub-issue of an epic and belongs to a milestone. Labels show the discipline.
- Art, animation, VFX, music and sound are tasks like code.
- Assign yourself when you start a task. Pick tasks by workstream (for example combat and UI vs. art and content), so we rarely touch the same files.
- Epics of the next milestone are broken into tasks at the end of the current one.
- Design specs follow [`design/README.md`](design/README.md).

## 3. Daily workflow

1. **Task:** pick an issue in the current milestone and assign yourself.
2. **Branch:** create it from a fresh `main`, named as `CLAUDE.md` describes:
   ```bash
   git switch main
   ```
   ```bash
   git pull
   ```
   ```bash
   git switch -c feature/attack-timing
   ```
3. **Binaries:** lock every binary you will change before editing it, and unlock it after the merge:
   ```bash
   git lfs lock Assets/Art/Characters/Knight.fbx
   ```
   ```bash
   git lfs unlock Assets/Art/Characters/Knight.fbx
   ```
   `git lfs locks` lists the current locks.
4. **Work and commit** following `CLAUDE.md`.
5. **Before pushing:** run the EditMode tests and play the game for a moment in the editor. If Rebuild touched files your PR is not about, close the scene in Unity (File → New Scene) and discard them:
   ```bash
   git restore Assets/Scenes Assets/Settings/BattlePostFX.asset Assets/Settings/StrongholdPostFX.asset ProjectSettings
   ```
6. **Push and PR:**
   ```bash
   git push -u origin feature/attack-timing
   ```
   Open the pull request from the template; its description starts with `Closes #17`, the issue it finishes.
7. **Merge:** the other person reviews and approves (one approval is required), then *Squash and merge*. The issue closes automatically.
