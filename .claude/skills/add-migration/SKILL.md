---
name: add-migration
description: Create, review and apply an EF Core migration after an entity or configuration change. Use when the database schema needs to change, or to introduce migrations for the first time.
argument-hint: "[MigrationName in PascalCase]"
allowed-tools: Bash(dotnet ef *) Bash(dotnet tool *) Bash(dotnet build *)
---

# Add an EF Core migration

Migration name: $ARGUMENTS (PascalCase, describes the change, e.g. `AddCarrierToShipments`).

## 1. Make sure the tool is available

```bash
dotnet tool restore   # dotnet-ef is pinned in .config/dotnet-tools.json
```

## 2. Generate

```bash
dotnet ef migrations add $ARGUMENTS \
  --project src/ShipmentTracker.Infrastructure \
  --startup-project src/ShipmentTracker.Api \
  --output-dir Persistence/Migrations
```

Never hand-edit `*.Designer.cs` or the model snapshot (a hook blocks it). If the migration is wrong, remove it with `dotnet ef migrations remove` (same `--project`/`--startup-project` flags), fix the model, and regenerate.

## 3. Review the SQL before applying

```bash
dotnet ef migrations script --idempotent \
  --project src/ShipmentTracker.Infrastructure \
  --startup-project src/ShipmentTracker.Api
```

Check for and call out:
- Dropped columns or tables (data loss).
- `NOT NULL` columns added to an existing table without a default.
- Renames that EF generated as drop + add.
- New indexes on large tables (lock risk in production).

## 4. First migration only: switch startup from EnsureCreated to Migrate

If this is the first migration in the repo:
- In `Infrastructure/Persistence/DatabaseInitializer.cs`, replace `EnsureCreatedAsync` with `MigrateAsync` and rename the method to `MigrateDatabaseAsync`; update `Program.cs`.
- Rename the config flag `Database:EnsureCreatedOnStartup` to `Database:MigrateOnStartup` everywhere (appsettings, docker-compose, `ApiFactory`).
- Drop your local database first (`docker compose down -v`), because EnsureCreated tables have no migrations history.
- Update the Database section of the root CLAUDE.md.

## 5. Verify

```bash
dotnet build
dotnet test   # integration tests apply the schema to a fresh container
```

Report the migration name, a summary of the SQL, and any risks from step 3.
