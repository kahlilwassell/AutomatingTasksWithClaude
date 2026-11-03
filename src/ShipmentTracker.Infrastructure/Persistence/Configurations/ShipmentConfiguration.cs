using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments");
        builder.HasKey(s => s.Id);

        // Ids are generated in the domain (UUIDv7), not by the database.
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.ReferenceNumber)
            .HasMaxLength(Shipment.ReferenceNumberMaxLength)
            .IsRequired();
        builder.HasIndex(s => s.ReferenceNumber).IsUnique();

        builder.Property(s => s.Origin).HasMaxLength(Shipment.LocationMaxLength).IsRequired();
        builder.Property(s => s.Destination).HasMaxLength(Shipment.LocationMaxLength).IsRequired();
        builder.Property(s => s.WeightKg).HasPrecision(10, 2);

        // Store the enum as readable text ("InTransit") rather than an int.
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(s => s.Status);
    }
}
