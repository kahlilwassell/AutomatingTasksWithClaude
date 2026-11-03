# Domain layer rules

This file loads when Claude works in this folder, on top of the root CLAUDE.md.

- Zero dependencies. Do not add `PackageReference` or `ProjectReference` items to this csproj.
- Entities protect their own invariants: private setters, a static factory (`Create`), and intention-revealing methods (`TransitionTo`, `UpdateDetails`). No public setters.
- No async, no I/O, no logging, no `DateTime.UtcNow` (callers pass the current time in).
- Throw `DomainException` for invariant violations. Expose `Can...` query methods (e.g. `CanTransitionTo`) so the Application layer can check first and return a `Result` instead.
- Every new rule needs a unit test in `tests/ShipmentTracker.UnitTests/Domain`.
