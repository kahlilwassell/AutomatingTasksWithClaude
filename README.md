# AutomatingTasksWithClaude
This repo is one where I experiment with automating tasks using LLMs.

It contains **ShipmentTracker**, a small but production-shaped .NET 10 REST API (Clean Architecture, EF Core, PostgreSQL), plus the Claude Code setup (context files, subagents, skills, hooks) and CI/CD used to practice automating everyday engineering work.

## Quick start

Prerequisites: .NET 10 SDK, Docker Desktop, and `jq` (preinstalled on macOS 15+; it's used by the Claude hooks).

```bash
docker compose up -d db                        # PostgreSQL on :5432
dotnet run --project src/ShipmentTracker.Api   # API on http://localhost:5080
```

Then send requests from `src/ShipmentTracker.Api/ShipmentTracker.Api.http`, or open the OpenAPI document at `http://localhost:5080/openapi/v1.json`.

To run everything in containers instead: `docker compose up --build` (API on `:8080`).

```bash
dotnet test tests/ShipmentTracker.UnitTests    # fast, no Docker
dotnet test                                    # + integration tests (Testcontainers, needs Docker)
```

## API

| Method | Route | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/v1/shipments?status=&page=&pageSize=` | Paged list, newest first | 200 | 400 |
| GET | `/api/v1/shipments/{id}` | Get one | 200 | 404 |
| POST | `/api/v1/shipments` | Create (reference numbers are unique) | 201 + Location | 400, 409 |
| PUT | `/api/v1/shipments/{id}` | Update origin/destination/weight (only while `Created`) | 200 | 400, 404, 409 |
| PATCH | `/api/v1/shipments/{id}/status` | Lifecycle: `Created -> InTransit -> Delivered`, or `-> Cancelled` | 200 | 400, 404, 409 |
| DELETE | `/api/v1/shipments/{id}` | Delete | 204 | 404 |
| GET | `/health` | Liveness + database check | 200 | 503 |

Errors are RFC 9457 problem details with a stable `code` (e.g. `Shipment.InvalidTransition`).

## Architecture

```
src/
  ShipmentTracker.Domain          entities + invariants, zero dependencies
  ShipmentTracker.Application     use cases, ports (IShipmentRepository, IUnitOfWork), Result<T>
  ShipmentTracker.Infrastructure  EF Core DbContext, configurations, repositories (PostgreSQL)
  ShipmentTracker.Api             minimal API endpoints, contracts, error mapping, Program.cs
tests/
  ShipmentTracker.UnitTests        domain + service tests with in-memory fakes
  ShipmentTracker.IntegrationTests WebApplicationFactory + Testcontainers PostgreSQL
```

Dependencies point inward: `Api -> Infrastructure -> Application -> Domain`. Package versions are centralized in `Directory.Packages.props`.

> The schema is created with `EnsureCreated` for now. There are no EF migrations yet. Introducing them is Exercise 1 (`/add-migration InitialCreate`).

## Claude Code setup

| Path | What it is |
|---|---|
| `CLAUDE.md` | Project context loaded every session: commands, architecture rules, conventions |
| `src/ShipmentTracker.Domain/CLAUDE.md`, `tests/CLAUDE.md` | Folder-scoped rules, loaded when working there |
| `.claude/settings.json` | Shared permissions (allow/ask/deny) and hooks |
| `.claude/hooks/protect-files.sh` | PreToolUse: blocks edits to generated migration files and `.env` |
| `.claude/hooks/format-csharp.sh` | PostToolUse: formats each `.cs` file Claude edits |
| `.claude/agents/code-reviewer.md` | Read-only reviewer subagent that enforces this repo's rules |
| `.claude/agents/test-writer.md` | Subagent that writes xUnit tests |
| `.claude/skills/add-endpoint` | `/add-endpoint <description>`: feature slice through every layer |
| `.claude/skills/add-migration` | `/add-migration <Name>`: generate, review SQL, apply |
| `.claude/skills/create-pr` | `/create-pr [issue#]`: quality gate, commit, push, open PR (manual only) |

Personal overrides go in `.claude/settings.local.json` (git-ignored).

## GitHub automation

| Workflow | Trigger | Does |
|---|---|---|
| `ci.yml` | PR, push to main | format check, build, unit + integration tests, Docker build |
| `claude.yml` | `@claude` in issues/PRs | Claude answers, or implements and opens a PR |
| `claude-pr-review.yml` | PR opened/updated | Claude reviews using `.claude/agents/code-reviewer.md` |
| `dependabot.yml` | weekly | NuGet, Actions and Docker update PRs |

To enable the Claude workflows: run `claude` in this repo, then `/install-github-app`. Or install the [Claude GitHub App](https://github.com/apps/claude) and add a `CLAUDE_CODE_OAUTH_TOKEN` (from `claude setup-token`) or `ANTHROPIC_API_KEY` repo secret. The workflows default to the OAuth token; swap the commented line if you use an API key.

`pipelines/azure-pipelines.yml` is the Azure DevOps equivalent of `ci.yml`, kept for comparison. It isn't used by this repo.
