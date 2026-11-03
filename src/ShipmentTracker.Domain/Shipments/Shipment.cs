using ShipmentTracker.Domain.Common;

namespace ShipmentTracker.Domain.Shipments;

/// <summary>
/// Aggregate root for a single shipment. All state changes go through methods
/// on this class so invariants (valid fields, legal status transitions) hold.
/// </summary>
public sealed class Shipment
{
    public const int ReferenceNumberMaxLength = 32;
    public const int LocationMaxLength = 128;
    public const decimal MaxWeightKg = 100_000m;

    private static readonly Dictionary<ShipmentStatus, ShipmentStatus[]> _allowedTransitions = new()
    {
        [ShipmentStatus.Created] = [ShipmentStatus.InTransit, ShipmentStatus.Cancelled],
        [ShipmentStatus.InTransit] = [ShipmentStatus.Delivered, ShipmentStatus.Cancelled],
        [ShipmentStatus.Delivered] = [],
        [ShipmentStatus.Cancelled] = [],
    };

    // Required by EF Core for materialization.
    private Shipment()
    {
    }

    public Guid Id { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
    public string Origin { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public decimal WeightKg { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? DeliveredAtUtc { get; private set; }

    public static Shipment Create(
        string referenceNumber,
        string origin,
        string destination,
        decimal weightKg,
        DateTimeOffset nowUtc)
    {
        var shipment = new Shipment
        {
            Id = Guid.CreateVersion7(nowUtc),
            ReferenceNumber = Require(referenceNumber, nameof(referenceNumber), ReferenceNumberMaxLength).ToUpperInvariant(),
            Status = ShipmentStatus.Created,
            CreatedAtUtc = nowUtc,
        };
        shipment.SetDetails(origin, destination, weightKg, nowUtc);
        return shipment;
    }

    public bool CanBeEdited => Status == ShipmentStatus.Created;

    public bool CanTransitionTo(ShipmentStatus next) => _allowedTransitions[Status].Contains(next);

    public void UpdateDetails(string origin, string destination, decimal weightKg, DateTimeOffset nowUtc)
    {
        if (!CanBeEdited)
        {
            throw new DomainException($"Shipment {ReferenceNumber} cannot be edited once it is {Status}.");
        }

        SetDetails(origin, destination, weightKg, nowUtc);
    }

    public void TransitionTo(ShipmentStatus next, DateTimeOffset nowUtc)
    {
        if (!CanTransitionTo(next))
        {
            throw new DomainException($"Cannot move shipment {ReferenceNumber} from {Status} to {next}.");
        }

        Status = next;
        UpdatedAtUtc = nowUtc;
        if (next == ShipmentStatus.Delivered)
        {
            DeliveredAtUtc = nowUtc;
        }
    }

    private void SetDetails(string origin, string destination, decimal weightKg, DateTimeOffset nowUtc)
    {
        if (weightKg <= 0 || weightKg > MaxWeightKg)
        {
            throw new DomainException($"Weight must be greater than 0 and at most {MaxWeightKg} kg.");
        }

        Origin = Require(origin, nameof(origin), LocationMaxLength);
        Destination = Require(destination, nameof(destination), LocationMaxLength);
        if (string.Equals(Origin, Destination, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Origin and destination must be different.");
        }

        WeightKg = weightKg;
        UpdatedAtUtc = nowUtc;
    }

    private static string Require(string value, string name, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{name} is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{name} must be at most {maxLength} characters.");
        }

        return trimmed;
    }
}
