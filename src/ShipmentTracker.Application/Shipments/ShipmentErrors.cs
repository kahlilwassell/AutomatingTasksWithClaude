using ShipmentTracker.Application.Common;
using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.Application.Shipments;

/// <summary>All expected errors for the Shipments feature, in one place.</summary>
public static class ShipmentErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Shipment.NotFound", $"Shipment '{id}' was not found.");

    public static Error DuplicateReference(string reference) =>
        Error.Conflict("Shipment.DuplicateReference", $"A shipment with reference '{reference}' already exists.");

    public static Error NotEditable(ShipmentStatus status) =>
        Error.Conflict("Shipment.NotEditable", $"Shipments can only be edited while Created (current: {status}).");

    public static Error InvalidTransition(ShipmentStatus from, ShipmentStatus to) =>
        Error.Conflict("Shipment.InvalidTransition", $"Cannot move a shipment from {from} to {to}.");

    public static Error SameOriginAndDestination() =>
        Error.Validation("Shipment.SameOriginAndDestination", "Origin and destination must be different.");
}
