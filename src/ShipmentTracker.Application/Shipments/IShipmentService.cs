using ShipmentTracker.Application.Common;

namespace ShipmentTracker.Application.Shipments;

public interface IShipmentService
{
    Task<Result<ShipmentDto>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<ShipmentDto>> ListAsync(ListShipmentsQuery query, CancellationToken cancellationToken);

    Task<Result<ShipmentDto>> CreateAsync(CreateShipmentCommand command, CancellationToken cancellationToken);

    Task<Result<ShipmentDto>> UpdateAsync(UpdateShipmentCommand command, CancellationToken cancellationToken);

    Task<Result<ShipmentDto>> ChangeStatusAsync(ChangeShipmentStatusCommand command, CancellationToken cancellationToken);

    Task<Result<Success>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
