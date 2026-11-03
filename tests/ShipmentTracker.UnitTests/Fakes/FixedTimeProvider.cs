namespace ShipmentTracker.UnitTests.Fakes;

/// <summary>A controllable clock. Inject instead of TimeProvider.System in tests.</summary>
public sealed class FixedTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public FixedTimeProvider()
        : this(new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.Zero))
    {
    }

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now = _now.Add(by);
}
