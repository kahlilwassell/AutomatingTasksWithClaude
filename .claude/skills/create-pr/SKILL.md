---
name: create-pr
description: Verify the branch, write a Conventional Commit, push, and open a GitHub pull request using the repo template. Only run when the user asks to open a PR.
disable-model-invocation: true
argument-hint: "[optional: issue number this PR closes]"
allowed-tools: Bash(git *) Bash(gh pr create *) Bash(gh pr view *) Bash(dotnet build *) Bash(dotnet test *) Bash(dotnet format *)
---

# Create a pull request

Related issue: $ARGUMENTS

## 1. Preconditions

- Current branch is NOT `main`. If it is, create one: `feat/...`, `fix/...` or `chore/...` from the change.
- `git status` shows what will be committed. Do not commit unrelated files.

## 2. Quality gate (stop and report if any fail)

```bash
dotnet format --verify-no-changes
dotnet build
dotnet test tests/ShipmentTracker.UnitTests
```

Run integration tests too if Docker is available: `dotnet test tests/ShipmentTracker.IntegrationTests`.

## 3. Commit

Conventional Commit subject under 72 chars (`feat: ...`, `fix: ...`, `test: ...`, `chore: ...`), then a body explaining **why**.

## 4. Push and open the PR

```bash
git push -u origin HEAD
gh pr create --base main --title "<commit subject>" --body-file <path to generated body>
```

Write the body by filling in `.github/pull_request_template.md`:
- **What / Why** from the diff and commits.
- **How to test**: exact commands or `.http` requests.
- **Risk**: schema changes, breaking API changes.
- If an issue number was given, add `Closes #<number>`.

Print the PR URL when done.

> Azure DevOps equivalent of the last step:
> `az repos pr create --source-branch <branch> --target-branch main --title "..." --description "..." --work-items <id>`
