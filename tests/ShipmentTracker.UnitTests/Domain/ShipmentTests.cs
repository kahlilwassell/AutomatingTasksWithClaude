using ShipmentTracker.Domain.Common;
using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.UnitTests.Domain;

public sealed class ShipmentTests
{
    private static readonly DateTimeOffset _now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    private static Shipment NewShipment() => Shipment.Create("shp-1", "Chicago", "Dallas", 10m, _now);

    [Fact]
    public void Create_sets_initial_state_and_normalizes_reference()
    {
        var shipment = Shipment.Create("  shp-1001 ", " Chicago ", "Dallas", 12.5m, _now);

        Assert.Equal("SHP-1001", shipment.ReferenceNumber);
        Assert.Equal("Chicago", shipment.Origin);
        Assert.Equal(ShipmentStatus.Created, shipment.Status);
        Assert.Equal(_now, shipment.CreatedAtUtc);
        Assert.Null(shipment.DeliveredAtUtc);
        Assert.NotEqual(Guid.Empty, shipment.Id);
    }

    [Theory]
    [InlineData("", "Chicago", "Dallas", 1)]
    [InlineData("SHP-1", " ", "Dallas", 1)]
    [InlineData("SHP-1", "Chicago", "Dallas", 0)]
    [InlineData("SHP-1", "Chicago", "Dallas", -5)]
    [InlineData("SHP-1", "Chicago", "chicago", 1)]
    public void Create_rejects_invalid_input(string reference, string origin, string destination, int weightKg)
    {
        Assert.Throws<DomainException>(() => Shipment.Create(reference, origin, destination, weightKg, _now));
    }

    [Theory]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.InTransit, true)]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.Cancelled, true)]
    [InlineData(ShipmentStatus.Created, ShipmentStatus.Delivered, false)]
    [InlineData(ShipmentStatus.InTransit, ShipmentStatus.Delivered, true)]
    [InlineData(ShipmentStatus.InTransit, ShipmentStatus.Cancelled, true)]
    [InlineData(ShipmentStatus.InTransit, ShipmentStatus.Created, false)]
    [InlineData(ShipmentStatus.Delivered, ShipmentStatus.Cancelled, false)]
    [InlineData(ShipmentStatus.Cancelled, ShipmentStatus.InTransit, false)]
    public void CanTransitionTo_follows_the_lifecycle(ShipmentStatus from, ShipmentStatus to, bool expected)
    {
        var shipment = InStatus(from);

        Assert.Equal(expected, shipment.CanTransitionTo(to));
    }

    [Fact]
    public void TransitionTo_delivered_stamps_delivery_time()
    {
        var shipment = NewShipment();
        var later = _now.AddHours(5);

        shipment.TransitionTo(ShipmentStatus.InTransit, _now.AddHours(1));
        shipment.TransitionTo(ShipmentStatus.Delivered, later);

        Assert.Equal(ShipmentStatus.Delivered, shipment.Status);
        Assert.Equal(later, shipment.DeliveredAtUtc);
        Assert.Equal(later, shipment.UpdatedAtUtc);
    }

    [Fact]
    public void TransitionTo_illegal_status_throws()
    {
        var shipment = NewShipment();

        Assert.Throws<DomainException>(() => shipment.TransitionTo(ShipmentStatus.Delivered, _now));
    }

    [Fact]
    public void UpdateDetails_is_only_allowed_while_created()
    {
        var shipment = NewShipment();
        shipment.UpdateDetails("Chicago", "Houston", 20m, _now);
        Assert.Equal("Houston", shipment.Destination);

        shipment.TransitionTo(ShipmentStatus.InTransit, _now);

        Assert.False(shipment.CanBeEdited);
        Assert.Throws<DomainException>(() => shipment.UpdateDetails("Chicago", "Austin", 20m, _now));
    }

    private static Shipment InStatus(ShipmentStatus status)
    {
        var shipment = NewShipment();
        switch (status)
        {
            case ShipmentStatus.InTransit:
                shipment.TransitionTo(ShipmentStatus.InTransit, _now);
                break;
            case ShipmentStatus.Delivered:
                shipment.TransitionTo(ShipmentStatus.InTransit, _now);
                shipment.TransitionTo(ShipmentStatus.Delivered, _now);
                break;
            case ShipmentStatus.Cancelled:
                shipment.TransitionTo(ShipmentStatus.Cancelled, _now);
                break;
        }

        return shipment;
    }
}
