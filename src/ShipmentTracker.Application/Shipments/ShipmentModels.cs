using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.Application.Shipments;

public sealed record ShipmentDto(
    Guid Id,
    string ReferenceNumber,
    string Origin,
    string Destination,
    decimal WeightKg,
    ShipmentStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeliveredAtUtc)
{
    public static ShipmentDto FromEntity(Shipment s) => new(
        s.Id,
        s.ReferenceNumber,
        s.Origin,
        s.Destination,
        s.WeightKg,
        s.Status,
        s.CreatedAtUtc,
        s.UpdatedAtUtc,
        s.DeliveredAtUtc);
}

public sealed record CreateShipmentCommand(string ReferenceNumber, string Origin, string Destination, decimal WeightKg);

public sealed record UpdateShipmentCommand(Guid Id, string Origin, string Destination, decimal WeightKg);

public sealed record ChangeShipmentStatusCommand(Guid Id, ShipmentStatus Status);

public sealed record ListShipmentsQuery(ShipmentStatus? Status, int Page, int PageSize);
