using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using ShipmentTracker.Application.Common;
using ShipmentTracker.Application.Shipments;
using ShipmentTracker.Domain.Shipments;

namespace ShipmentTracker.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class ShipmentEndpointsTests(ApiFactory factory)
{
    private const string BaseUrl = "/api/v1/shipments";

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // Tests share one database, so every test uses its own unique reference numbers.
    private static string NewReference() => $"IT-{Guid.NewGuid():N}"[..20].ToUpperInvariant();

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await _client.GetAsync("/health", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_then_get_round_trips_through_the_database()
    {
        var reference = NewReference();

        var create = await _client.PostAsJsonAsync(
            BaseUrl,
            new { referenceNumber = reference, origin = "Chicago", destination = "Dallas", weightKg = 12.5m },
            _json,
            Ct);

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.NotNull(create.Headers.Location);
        var created = await create.Content.ReadFromJsonAsync<ShipmentDto>(_json, Ct);

        var fetched = await _client.GetFromJsonAsync<ShipmentDto>(create.Headers.Location, _json, Ct);
        Assert.Equal(created!.Id, fetched!.Id);
        Assert.Equal(reference, fetched.ReferenceNumber);
        Assert.Equal(ShipmentStatus.Created, fetched.Status);
    }

    [Fact]
    public async Task Duplicate_reference_returns_409()
    {
        var body = new { referenceNumber = NewReference(), origin = "Chicago", destination = "Dallas", weightKg = 1m };
        await _client.PostAsJsonAsync(BaseUrl, body, _json, Ct);

        var second = await _client.PostAsJsonAsync(BaseUrl, body, _json, Ct);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>(_json, Ct);
        Assert.Equal("Shipment.DuplicateReference", problem!.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task Invalid_body_returns_400_with_field_errors()
    {
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            new { referenceNumber = "x", origin = "", destination = "Dallas", weightKg = 0m },
            _json,
            Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(_json, Ct);
        Assert.True(problem!.Errors.ContainsKey("Origin"));
        Assert.True(problem.Errors.ContainsKey("WeightKg"));
    }

    [Fact]
    public async Task Status_lifecycle_is_enforced()
    {
        var created = await CreateShipmentAsync();
        var statusUrl = $"{BaseUrl}/{created.Id}/status";

        var skip = await _client.PatchAsJsonAsync(statusUrl, new { status = "Delivered" }, _json, Ct);
        Assert.Equal(HttpStatusCode.Conflict, skip.StatusCode);

        var dispatch = await _client.PatchAsJsonAsync(statusUrl, new { status = "InTransit" }, _json, Ct);
        Assert.Equal(HttpStatusCode.OK, dispatch.StatusCode);

        var deliver = await _client.PatchAsJsonAsync(statusUrl, new { status = "Delivered" }, _json, Ct);
        var delivered = await deliver.Content.ReadFromJsonAsync<ShipmentDto>(_json, Ct);
        Assert.Equal(ShipmentStatus.Delivered, delivered!.Status);
        Assert.NotNull(delivered.DeliveredAtUtc);
    }

    [Fact]
    public async Task List_filters_by_status()
    {
        var cancelled = await CreateShipmentAsync();
        await _client.PatchAsJsonAsync($"{BaseUrl}/{cancelled.Id}/status", new { status = "Cancelled" }, _json, Ct);

        var page = await _client.GetFromJsonAsync<PagedResult<ShipmentDto>>(
            $"{BaseUrl}?status=Cancelled&pageSize=100",
            _json,
            Ct);

        Assert.Contains(page!.Items, s => s.Id == cancelled.Id);
        Assert.All(page.Items, s => Assert.Equal(ShipmentStatus.Cancelled, s.Status));
    }

    [Fact]
    public async Task Delete_then_get_returns_404()
    {
        var created = await CreateShipmentAsync();

        var delete = await _client.DeleteAsync($"{BaseUrl}/{created.Id}", Ct);
        var get = await _client.GetAsync($"{BaseUrl}/{created.Id}", Ct);

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    private async Task<ShipmentDto> CreateShipmentAsync()
    {
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            new { referenceNumber = NewReference(), origin = "Chicago", destination = "Dallas", weightKg = 10m },
            _json,
            Ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ShipmentDto>(_json, Ct))!;
    }
}
