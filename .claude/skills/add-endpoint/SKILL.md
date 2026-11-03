---
name: add-endpoint
description: Add a new REST endpoint or feature slice end-to-end (domain, application, infrastructure, API, tests) following this repo's Clean Architecture. Use when asked to add or extend an API operation.
argument-hint: "[what the endpoint should do]"
---

# Add an endpoint

Goal: $ARGUMENTS

Work through these steps in order. Skip a step only if it truly does not apply, and say so.

## 1. Plan (show this before writing code)

- HTTP method, route under `/api/v1/...`, request/response shapes, and every status code it can return.
- Which layers change. If the plan needs a new database column or index, flag that a migration is required.

## 2. Domain (`src/ShipmentTracker.Domain`)

- New behavior goes on the entity as a method that protects invariants. Add a `Can...` query if callers need to pre-check.
- Add unit tests in `tests/ShipmentTracker.UnitTests/Domain`.

## 3. Application (`src/ShipmentTracker.Application`)

- Add a command/query record in `<Feature>/<Feature>Models.cs`.
- Add the method to `I<Feature>Service` and implement it in `<Feature>Service`, returning `Result<T>`.
- New expected failures go in `<Feature>Errors` with a stable code like `Shipment.Something`.
- If the repository needs a new query, add it to the port (`Abstractions/`) AND to `tests/.../Fakes/InMemoryShipmentRepository.cs`.
- Add service tests in `tests/ShipmentTracker.UnitTests/Application`.

## 4. Infrastructure (`src/ShipmentTracker.Infrastructure`)

- Implement any new repository method with EF Core. Use `AsNoTracking()` for reads.
- Mapping changes go in `Persistence/Configurations`. If the schema changed, use the `add-migration` skill.

## 5. API (`src/ShipmentTracker.Api`)

- Request contract in `Contracts/` with DataAnnotations.
- Map the route in `Endpoints/<Feature>Endpoints.cs` with `.WithName(...)` and `.WithSummary(...)`.
- Handler returns `Results<..., ProblemHttpResult>` and maps errors with `ErrorMapper.ToProblem`.
- Add a sample request to `ShipmentTracker.Api.http`.

## 6. Integration test

- Add a test in `tests/ShipmentTracker.IntegrationTests` covering the happy path and at least one error status.

## 7. Verify

```bash
dotnet build
dotnet test tests/ShipmentTracker.UnitTests
dotnet test tests/ShipmentTracker.IntegrationTests   # needs Docker
dotnet format --verify-no-changes
```

Finish with a short summary: files changed, endpoints added, tests added, and anything left undone.
