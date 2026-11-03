using System.ComponentModel.DataAnnotations;
using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.Api.Contracts;

// HTTP request contracts. Validated automatically by .NET 10 minimal API
// validation (builder.Services.AddValidation()) before the handler runs.

public sealed record CreateShipmentRequest(
    [property: Required, StringLength(Shipment.ReferenceNumberMaxLength, MinimumLength = 3)] string ReferenceNumber,
    [property: Required, StringLength(Shipment.LocationMaxLength)] string Origin,
    [property: Required, StringLength(Shipment.LocationMaxLength)] string Destination,
    [property: Range(typeof(decimal), "0.01", "100000")] decimal WeightKg);

public sealed record UpdateShipmentRequest(
    [property: Required, StringLength(Shipment.LocationMaxLength)] string Origin,
    [property: Required, StringLength(Shipment.LocationMaxLength)] string Destination,
    [property: Range(typeof(decimal), "0.01", "100000")] decimal WeightKg);

public sealed record ChangeShipmentStatusRequest(
    [property: Required] ShipmentStatus? Status);

public sealed class ListShipmentsRequest
{
    public const int DefaultPageSize = 20;

    public ShipmentStatus? Status { get; init; }

    // Nullable so they are optional in the query string ([AsParameters] ignores initializers).
    [Range(1, int.MaxValue)]
    public int? Page { get; init; }

    [Range(1, 100)]
    public int? PageSize { get; init; }
}
