using ShipmentTracker.Application.Abstractions;
using ShipmentTracker.Application.Common;
using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.Application.Shipments;

/// <summary>
/// Use cases for the Shipments feature. Orchestrates the domain model and
/// persistence; contains no HTTP or EF Core specifics.
/// </summary>
internal sealed class ShipmentService(
    IShipmentRepository repository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IShipmentService
{
    public async Task<Result<ShipmentDto>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(id, cancellationToken);
        return shipment is null ? ShipmentErrors.NotFound(id) : ShipmentDto.FromEntity(shipment);
    }

    public async Task<PagedResult<ShipmentDto>> ListAsync(ListShipmentsQuery query, CancellationToken cancellationToken)
    {
        var skip = (query.Page - 1) * query.PageSize;
        var (items, total) = await repository.ListAsync(query.Status, skip, query.PageSize, cancellationToken);
        return new PagedResult<ShipmentDto>(
            items.Select(ShipmentDto.FromEntity).ToList(),
            query.Page,
            query.PageSize,
            total);
    }

    public async Task<Result<ShipmentDto>> CreateAsync(CreateShipmentCommand command, CancellationToken cancellationToken)
    {
        if (IsSameLocation(command.Origin, command.Destination))
        {
            return ShipmentErrors.SameOriginAndDestination();
        }

        var reference = command.ReferenceNumber.Trim().ToUpperInvariant();
        if (await repository.ReferenceNumberExistsAsync(reference, cancellationToken))
        {
            return ShipmentErrors.DuplicateReference(reference);
        }

        var shipment = Shipment.Create(
            reference,
            command.Origin,
            command.Destination,
            command.WeightKg,
            timeProvider.GetUtcNow());

        repository.Add(shipment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ShipmentDto.FromEntity(shipment);
    }

    public async Task<Result<ShipmentDto>> UpdateAsync(UpdateShipmentCommand command, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (shipment is null)
        {
            return ShipmentErrors.NotFound(command.Id);
        }

        if (!shipment.CanBeEdited)
        {
            return ShipmentErrors.NotEditable(shipment.Status);
        }

        if (IsSameLocation(command.Origin, command.Destination))
        {
            return ShipmentErrors.SameOriginAndDestination();
        }

        shipment.UpdateDetails(command.Origin, command.Destination, command.WeightKg, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ShipmentDto.FromEntity(shipment);
    }

    public async Task<Result<ShipmentDto>> ChangeStatusAsync(ChangeShipmentStatusCommand command, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (shipment is null)
        {
            return ShipmentErrors.NotFound(command.Id);
        }

        if (!shipment.CanTransitionTo(command.Status))
        {
            return ShipmentErrors.InvalidTransition(shipment.Status, command.Status);
        }

        shipment.TransitionTo(command.Status, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ShipmentDto.FromEntity(shipment);
    }

    public async Task<Result<Success>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByIdAsync(id, cancellationToken);
        if (shipment is null)
        {
            return ShipmentErrors.NotFound(id);
        }

        repository.Remove(shipment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new Success();
    }

    private static bool IsSameLocation(string origin, string destination) =>
        string.Equals(origin.Trim(), destination.Trim(), StringComparison.OrdinalIgnoreCase);
}
