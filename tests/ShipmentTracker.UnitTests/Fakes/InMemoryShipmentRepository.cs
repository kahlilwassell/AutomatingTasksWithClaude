using ShipmentTracker.Application.Abstractions;
using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.UnitTests.Fakes;

/// <summary>
/// Hand-rolled fake of the persistence port. Preferred over a mocking library:
/// tests assert on state, not on which methods were called.
/// </summary>
public sealed class InMemoryShipmentRepository : IShipmentRepository, IUnitOfWork
{
    private readonly List<Shipment> _shipments = [];

    public int SaveChangesCount { get; private set; }

    public IReadOnlyList<Shipment> All => _shipments;

    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_shipments.FirstOrDefault(s => s.Id == id));

    public Task<bool> ReferenceNumberExistsAsync(string referenceNumber, CancellationToken cancellationToken) =>
        Task.FromResult(_shipments.Any(s => s.ReferenceNumber == referenceNumber));

    public Task<(IReadOnlyList<Shipment> Items, int TotalCount)> ListAsync(
        ShipmentStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var filtered = _shipments
            .Where(s => status is null || s.Status == status)
            .OrderByDescending(s => s.CreatedAtUtc)
            .ThenBy(s => s.Id)
            .ToList();
        IReadOnlyList<Shipment> page = filtered.Skip(skip).Take(take).ToList();
        return Task.FromResult((page, filtered.Count));
    }

    public void Add(Shipment shipment) => _shipments.Add(shipment);

    public void Remove(Shipment shipment) => _shipments.Remove(shipment);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCount++;
        return Task.FromResult(1);
    }
}
