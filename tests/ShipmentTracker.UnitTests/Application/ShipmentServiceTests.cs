using ShipmentTracker.Application.Common;
using ShipmentTracker.Application.Shipments;
using ShipmentTracker.Domain.Shipments;
using ShipmentTracker.UnitTests.Fakes;

namespace ShipmentTracker.UnitTests.Application;

public sealed class ShipmentServiceTests
{
    private readonly InMemoryShipmentRepository _repository = new();
    private readonly FixedTimeProvider _clock = new();
    private readonly ShipmentService _service;

    public ShipmentServiceTests()
    {
        _service = new ShipmentService(_repository, _repository, _clock);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Create_persists_and_returns_the_shipment()
    {
        var result = await _service.CreateAsync(new CreateShipmentCommand("shp-1", "Chicago", "Dallas", 10m), Ct);

        Assert.True(result.IsSuccess);
        Assert.Equal("SHP-1", result.Value.ReferenceNumber);
        Assert.Single(_repository.All);
        Assert.Equal(1, _repository.SaveChangesCount);
    }

    [Fact]
    public async Task Create_with_duplicate_reference_is_a_conflict()
    {
        await _service.CreateAsync(new CreateShipmentCommand("SHP-1", "Chicago", "Dallas", 10m), Ct);

        var result = await _service.CreateAsync(new CreateShipmentCommand("shp-1", "Denver", "Austin", 5m), Ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("Shipment.DuplicateReference", result.Error.Code);
        Assert.Single(_repository.All);
    }

    [Fact]
    public async Task Create_with_same_origin_and_destination_is_a_validation_error()
    {
        var result = await _service.CreateAsync(new CreateShipmentCommand("SHP-1", "Dallas", " dallas ", 10m), Ct);

        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Empty(_repository.All);
    }

    [Fact]
    public async Task Get_unknown_id_is_not_found()
    {
        var result = await _service.GetAsync(Guid.NewGuid(), Ct);

        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task ChangeStatus_follows_lifecycle_and_rejects_illegal_moves()
    {
        var created = (await _service.CreateAsync(new CreateShipmentCommand("SHP-1", "Chicago", "Dallas", 10m), Ct)).Value;

        var illegal = await _service.ChangeStatusAsync(new ChangeShipmentStatusCommand(created.Id, ShipmentStatus.Delivered), Ct);
        Assert.Equal("Shipment.InvalidTransition", illegal.Error!.Code);

        _clock.Advance(TimeSpan.FromHours(2));
        var inTransit = await _service.ChangeStatusAsync(new ChangeShipmentStatusCommand(created.Id, ShipmentStatus.InTransit), Ct);
        Assert.Equal(ShipmentStatus.InTransit, inTransit.Value.Status);
        Assert.Equal(_clock.GetUtcNow(), inTransit.Value.UpdatedAtUtc);
    }

    [Fact]
    public async Task Update_after_dispatch_is_a_conflict()
    {
        var created = (await _service.CreateAsync(new CreateShipmentCommand("SHP-1", "Chicago", "Dallas", 10m), Ct)).Value;
        await _service.ChangeStatusAsync(new ChangeShipmentStatusCommand(created.Id, ShipmentStatus.InTransit), Ct);

        var result = await _service.UpdateAsync(new UpdateShipmentCommand(created.Id, "Chicago", "Austin", 10m), Ct);

        Assert.Equal("Shipment.NotEditable", result.Error!.Code);
    }

    [Fact]
    public async Task List_filters_by_status_and_pages()
    {
        for (var i = 0; i < 5; i++)
        {
            _clock.Advance(TimeSpan.FromMinutes(1));
            await _service.CreateAsync(new CreateShipmentCommand($"SHP-{i}", "Chicago", "Dallas", 10m), Ct);
        }

        var first = _repository.All[0];
        await _service.ChangeStatusAsync(new ChangeShipmentStatusCommand(first.Id, ShipmentStatus.Cancelled), Ct);

        var page = await _service.ListAsync(new ListShipmentsQuery(ShipmentStatus.Created, Page: 1, PageSize: 3), Ct);

        Assert.Equal(4, page.TotalCount);
        Assert.Equal(3, page.Items.Count);
        Assert.Equal(2, page.TotalPages);
        Assert.Equal("SHP-4", page.Items[0].ReferenceNumber); // newest first
    }

    [Fact]
    public async Task Delete_removes_the_shipment()
    {
        var created = (await _service.CreateAsync(new CreateShipmentCommand("SHP-1", "Chicago", "Dallas", 10m), Ct)).Value;

        var result = await _service.DeleteAsync(created.Id, Ct);

        Assert.True(result.IsSuccess);
        Assert.Empty(_repository.All);
    }
}
