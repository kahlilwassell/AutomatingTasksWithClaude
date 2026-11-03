# ShipmentTracker: project context for Claude

A small .NET 10 REST API for tracking freight shipments. It exists as a sandbox for
practicing AI-assisted engineering workflows. Keep the code production-quality anyway.

## Commands

```bash
docker compose up -d db                          # start PostgreSQL on :5432
dotnet build                                     # build everything (warnings are errors)
dotnet test tests/ShipmentTracker.UnitTests      # fast tests, no Docker needed
dotnet test                                      # all tests; integration tests need Docker running
dotnet run --project src/ShipmentTracker.Api     # API on http://localhost:5080
dotnet format --verify-no-changes                # style check (CI runs this)
```

After changing C# code, always run `dotnet build` and the unit tests before saying you are done.
Run the integration tests too when you touch Infrastructure, endpoints, or contracts.

## Architecture (Clean Architecture)

Dependencies point inward only: `Api -> Infrastructure -> Application -> Domain`.

| Project | Owns | Must NOT reference |
|---|---|---|
| `Domain` | Entities, enums, invariants (`Shipment`, `ShipmentStatus`) | anything (no packages, no projects) |
| `Application` | Use cases (`ShipmentService`), ports (`IShipmentRepository`, `IUnitOfWork`), DTOs, `Result`/`Error` | EF Core, ASP.NET Core, Infrastructure |
| `Infrastructure` | EF Core `DbContext`, entity configurations, repository implementations | Api |
| `Api` | Minimal API endpoints, request contracts, HTTP error mapping, composition root (`Program.cs`) | (top of the graph) |

## Conventions

- **Minimal APIs** grouped per feature in `Api/Endpoints/<Feature>Endpoints.cs`; return `TypedResults` with `Results<...>` union types so OpenAPI is accurate.
- **Request contracts** live in `Api/Contracts` and use DataAnnotations; .NET 10 `AddValidation()` returns 400s automatically. Map contracts to Application commands in the endpoint.
- **Expected failures** (not found, conflict, business validation) return `Result<T>` with an `Error` from the feature's `*Errors` class. Do not throw for these. `ErrorMapper` turns them into RFC 9457 problem details.
- **Domain invariants** are enforced inside entities and throw `DomainException` (mapped to 400) as a backstop.
- **Time**: inject `TimeProvider`; never call `DateTime.Now`/`UtcNow` directly. Store `DateTimeOffset` in UTC.
- **Ids**: `Guid.CreateVersion7()` generated in the domain.
- **Async**: every I/O method is async and takes a `CancellationToken` as its last parameter.
- **Packages**: versions live only in `Directory.Packages.props` (Central Package Management). Never put `Version=` in a csproj.
- Private fields are `_camelCase`; file-scoped namespaces; one public type per file unless they are tiny related records.

## Testing

- `tests/ShipmentTracker.UnitTests`: xUnit v3. Domain rules and `ShipmentService` against `Fakes/InMemoryShipmentRepository` and `FixedTimeProvider`. No mocking library; assert on state.
- `tests/ShipmentTracker.IntegrationTests`: `WebApplicationFactory<Program>` + Testcontainers PostgreSQL. Tests share one database, so always use unique reference numbers.
- Test names describe behavior: `Update_after_dispatch_is_a_conflict`.
- Pass `TestContext.Current.CancellationToken` to async calls in tests (xUnit analyzer requires it).

## Database

- PostgreSQL via Npgsql. Schema is currently created with `EnsureCreated` on startup (`Database:EnsureCreatedOnStartup`). There are **no EF migrations yet**; adding them is a planned exercise. Use the `add-migration` skill.
- Never hand-edit a generated migration that has been merged to `main`; add a new one.

## Git and PRs

- Branch names: `feat/<short-desc>`, `fix/<short-desc>`, `chore/<short-desc>`.
- Commit messages: Conventional Commits (`feat: add shipment search endpoint`).
- Open PRs with the `create-pr` skill; fill in the PR template.

## Where to look

- Adding an endpoint end-to-end: `.claude/skills/add-endpoint/SKILL.md`
- Reviewing a change: the `code-reviewer` subagent
- Writing tests for a change: the `test-writer` subagent
