---
name: featherguard-task
description: Use when starting, testing or shipping any change in the Featherguard repo (code, data, docs or art) - the order of steps from the issue to the pull request, including worktrees while Unity is open, running tests, Rebuild and its noise, and stacked pull requests
---

# Working on a Featherguard task

The rules are in `CLAUDE.md`; this skill is the order of steps that has worked for both developers. Reply to your developer in their language; everything in the repo stays English.

## 1. Issue

- Every change starts from an issue. Find it, or propose one and create it with `gh issue create` from the matching template once your developer agrees.
- Make it a sub-issue of its epic and put it in the epic's milestone:
  ```bash
  id=$(gh api repos/Patryk-Suchecki/Featherguard/issues/<new> --jq .id)
  gh api -X POST repos/Patryk-Suchecki/Featherguard/issues/<epic>/sub_issues -F sub_issue_id=$id
  ```
- Docs and tooling issues need no label; others get `gameplay`, `art`, `audio`, `UI`, `bug` or `balance`.

## 2. Where to work

- **Check whether Unity is open on the main checkout** before touching it: `unity status --project-path <repo>` (state `ready`) or `tasklist /FI "IMAGENAME eq Unity.exe"`.
- **If it is open, never switch branches there.** Unity would reimport everything, and the developer may be in the middle of something. Work in a worktree instead:
  ```bash
  git fetch
  git worktree add -b <type>/<short-task> <path> origin/main
  ```
  - Docs-only work can skip the LFS download: prefix the command with `GIT_LFS_SKIP_SMUDGE=1`.
  - Code, data or art work needs the real binaries: run `git lfs checkout` in the worktree. Its first batch-mode run imports the project, which takes a few minutes.
- Stacked work (a PR that needs another unmerged PR) branches from the parent branch instead of `origin/main`.

## 3. Make the change

- Follow the layer, SOLID, data and style rules in `CLAUDE.md`.
- **Data assets:** change them through the catalogs (`Assets/Editor/Setup/*Catalog.cs`) so a fresh checkout rebuilds them. When existing assets must change (a renamed field, a new required value), write a temporary editor script with a static method, run it once in batch mode (`-executeMethod`), check the asset diff, then delete the script before committing. Never hand-edit `.asset` YAML while an editor has the project open.
- **Content or rule changes** update `docs/GAME_DESIGN.md` in the same PR.

## 4. Rebuild

- **Editor open on the checkout you work in:** `unity command editor invoke_menu --path "Featherguard/Rebuild Battle Scene" --project-path <path>`.
- **No editor on that checkout** (a worktree):
  ```bash
  "<Unity.exe>" -batchmode -nographics -projectPath <path> -executeMethod Featherguard.EditorTools.SetupMenu.Run -quit -logFile <log>
  ```
  It currently ends with an `UnauthorizedAccessException` on `Assets/Art/UI/AppIcon.png` after all data is saved; check the log for that and for `error CS`.
- **Discard the noise** unless the PR is about it:
  ```bash
  git restore Assets/Scenes/Battle.unity Assets/Settings/BattlePostFX.asset Assets/Settings/BattlePostFX.asset.meta ProjectSettings
  ```
  Also drop `.fbx.meta` files that only changed line endings, and an untracked `.vsconfig`.
- Review every changed asset: a reserialized asset may only reorder references, which is noise too.

## 5. Test

- **Editor open on the same checkout:**
  ```bash
  unity command test run --mode editor --project-path <path> --format json
  ```
  Read `data.result.Summary` (`Total`, `Passed`, `Failed`).
- **No editor (worktree or closed Unity):**
  ```bash
  "<Unity.exe>" -batchmode -nographics -projectPath <path> -runTests -testPlatform EditMode -testResults <xml> -logFile <log>
  ```
  Read the totals from the XML (`total="…" passed="…" failed="…"`) and compile errors from the log (`error CS`).
- `unity test` refuses a project that is open in an editor; never close the developer's editor to run it.
- Add or update EditMode tests for every rule or economy change; test the model directly with `FakeRandom`.
- Follow `verification-before-completion`: report the numbers you saw, and say plainly what was not checked (for example "not played in the editor").

## 6. Commit and pull request

- Commit only what belongs to the task (`git status` first). One short imperative English line, no issue number, no AI attribution.
- Push and open the PR with `gh pr create`, following `.github/pull_request_template.md`:
  - title: the task, no issue number;
  - the description starts with `Closes #<issue>`, or `Part of #<issue>` when the issue needs more PRs;
  - for a stacked PR, set `--base <parent branch>` and add "Based on #<parent>. Merge it first; this PR then retargets to `main`.";
  - add the label of the issue.
- `CODEOWNERS` requests the other developer's review. `gh pr create` may still print "Could not add requested reviewers"; check with `gh pr view <n> --json reviewRequests`.
- **Editing a PR description:** write the body to a file, check that the file is not empty, then `gh pr edit <n> --body-file <file>`. An empty file wipes the description.
- Never mention AI tools, generated images or Claude anywhere in the repo, commits or PRs.

## 7. After review and merge

- Never approve or merge without your developer asking in the conversation.
- A squash merge of a parent PR retargets the stacked child to `main`, dismisses its approval and usually leaves conflicts. Merge `main` into the child (when git cannot tell what is already in, `git merge-tree --merge-base=<old parent tip>` helps), push, and ask for a fresh approval.
- Never rename a PR's head branch: GitHub closes the PR.
- Unlock LFS files you locked once the PR is merged (`git lfs locks`, `git lfs unlock <path>`).
- Remove worktrees you created when their branch is merged (`git worktree remove <path>`; on Windows a very deep `Library` folder may need deleting by hand first).
