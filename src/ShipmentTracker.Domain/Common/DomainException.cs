namespace ShipmentTracker.Domain.Common;

/// <summary>
/// Thrown when a domain invariant is violated. The API maps this to HTTP 400.
/// The Application layer should validate first, so reaching this is a backstop.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
