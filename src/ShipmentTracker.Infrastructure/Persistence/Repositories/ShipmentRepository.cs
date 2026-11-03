using Microsoft.EntityFrameworkCore;
using ShipmentTracker.Application.Abstractions;
using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.Infrastructure.Persistence.Repositories;

internal sealed class ShipmentRepository(ShipmentTrackerDbContext db) : IShipmentRepository
{
    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Shipments.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<bool> ReferenceNumberExistsAsync(string referenceNumber, CancellationToken cancellationToken) =>
        db.Shipments.AnyAsync(s => s.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task<(IReadOnlyList<Shipment> Items, int TotalCount)> ListAsync(
        ShipmentStatus? status,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var query = db.Shipments.AsNoTracking();
        if (status is not null)
        {
            query = query.Where(s => s.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(s => s.CreatedAtUtc)
            .ThenBy(s => s.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public void Add(Shipment shipment) => db.Shipments.Add(shipment);

    public void Remove(Shipment shipment) => db.Shipments.Remove(shipment);
}
