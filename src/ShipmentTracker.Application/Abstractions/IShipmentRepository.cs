using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.Application.Abstractions;

/// <summary>
/// Persistence port for shipments. Implemented in Infrastructure (EF Core) and
/// by an in-memory fake in the unit tests.
/// </summary>
public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ReferenceNumberExistsAsync(string referenceNumber, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Shipment> Items, int TotalCount)> ListAsync(
        ShipmentStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken);

    void Add(Shipment shipment);

    void Remove(Shipment shipment);
}
