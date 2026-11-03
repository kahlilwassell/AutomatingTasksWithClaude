using Microsoft.EntityFrameworkCore;
using ShipmentTracker.Application.Abstractions;
using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.Infrastructure.Persistence;

public sealed class ShipmentTrackerDbContext(DbContextOptions<ShipmentTrackerDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipmentTrackerDbContext).Assembly);
}
