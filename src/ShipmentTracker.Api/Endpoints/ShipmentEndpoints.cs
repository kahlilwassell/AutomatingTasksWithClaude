using Microsoft.AspNetCore.Http.HttpResults;
using ShipmentTracker.Api.Contracts;
using ShipmentTracker.Api.ErrorHandling;
using ShipmentTracker.Application.Common;
using ShipmentTracker.Application.Shipments;

namespace ShipmentTracker.Api.Endpoints;

public static class ShipmentEndpoints
{
    private const string GetShipmentRoute = "GetShipment";

    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/shipments").WithTags("Shipments");

        group.MapGet("/", ListShipments)
            .WithName("ListShipments")
            .WithSummary("List shipments, optionally filtered by status (paged).");

        group.MapGet("/{id:guid}", GetShipment)
            .WithName(GetShipmentRoute)
            .WithSummary("Get a single shipment.");

        group.MapPost("/", CreateShipment)
            .WithName("CreateShipment")
            .WithSummary("Create a shipment. Reference numbers are unique.");

        group.MapPut("/{id:guid}", UpdateShipment)
            .WithName("UpdateShipment")
            .WithSummary("Update origin, destination and weight. Only allowed while status is Created.");

        group.MapPatch("/{id:guid}/status", ChangeShipmentStatus)
            .WithName("ChangeShipmentStatus")
            .WithSummary("Move a shipment through its lifecycle: Created -> InTransit -> Delivered, or -> Cancelled.");

        group.MapDelete("/{id:guid}", DeleteShipment)
            .WithName("DeleteShipment")
            .WithSummary("Delete a shipment.");

        return app;
    }

    private static async Task<Ok<PagedResult<ShipmentDto>>> ListShipments(
        [AsParameters] ListShipmentsRequest request,
        IShipmentService service,
        CancellationToken cancellationToken)
    {
        var page = await service.ListAsync(
            new ListShipmentsQuery(request.Status, request.Page ?? 1, request.PageSize ?? ListShipmentsRequest.DefaultPageSize),
            cancellationToken);
        return TypedResults.Ok(page);
    }

    private static async Task<Results<Ok<ShipmentDto>, ProblemHttpResult>> GetShipment(
        Guid id,
        IShipmentService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return result.Match<Results<Ok<ShipmentDto>, ProblemHttpResult>>(
            dto => TypedResults.Ok(dto),
            error => ErrorMapper.ToProblem(error));
    }

    private static async Task<Results<CreatedAtRoute<ShipmentDto>, ProblemHttpResult>> CreateShipment(
        CreateShipmentRequest request,
        IShipmentService service,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(
            new CreateShipmentCommand(request.ReferenceNumber, request.Origin, request.Destination, request.WeightKg),
            cancellationToken);
        return result.Match<Results<CreatedAtRoute<ShipmentDto>, ProblemHttpResult>>(
            dto => TypedResults.CreatedAtRoute(dto, GetShipmentRoute, new { id = dto.Id }),
            error => ErrorMapper.ToProblem(error));
    }

    private static async Task<Results<Ok<ShipmentDto>, ProblemHttpResult>> UpdateShipment(
        Guid id,
        UpdateShipmentRequest request,
        IShipmentService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(
            new UpdateShipmentCommand(id, request.Origin, request.Destination, request.WeightKg),
            cancellationToken);
        return result.Match<Results<Ok<ShipmentDto>, ProblemHttpResult>>(
            dto => TypedResults.Ok(dto),
            error => ErrorMapper.ToProblem(error));
    }

    private static async Task<Results<Ok<ShipmentDto>, ProblemHttpResult>> ChangeShipmentStatus(
        Guid id,
        ChangeShipmentStatusRequest request,
        IShipmentService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ChangeStatusAsync(
            new ChangeShipmentStatusCommand(id, request.Status!.Value),
            cancellationToken);
        return result.Match<Results<Ok<ShipmentDto>, ProblemHttpResult>>(
            dto => TypedResults.Ok(dto),
            error => ErrorMapper.ToProblem(error));
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteShipment(
        Guid id,
        IShipmentService service,
        CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        return result.Match<Results<NoContent, ProblemHttpResult>>(
            _ => TypedResults.NoContent(),
            error => ErrorMapper.ToProblem(error));
    }
}
