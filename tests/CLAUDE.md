# Test conventions

- Framework: xUnit v3 (`[Fact]`, `[Theory]` + `[InlineData]`). Projects are `OutputType=Exe`; that is expected for v3.
- Arrange / Act / Assert with blank lines between sections. One behavior per test.
- Unit tests: use `Fakes/InMemoryShipmentRepository` and `Fakes/FixedTimeProvider`. Do not add Moq/NSubstitute.
- Integration tests: join the `[Collection(ApiCollection.Name)]` collection to reuse the PostgreSQL container. Generate unique reference numbers per test.
- Always pass `TestContext.Current.CancellationToken` to async calls.
- A bug fix starts with a failing test that reproduces it.
