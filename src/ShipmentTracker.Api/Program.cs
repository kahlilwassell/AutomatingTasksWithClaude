using ShipmentTracker.Api;
using ShipmentTracker.Api.Endpoints;
using ShipmentTracker.Application;
using ShipmentTracker.Infrastructure;
using ShipmentTracker.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddApi();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    // OpenAPI document at /openapi/v1.json
    app.MapOpenApi();
}

if (app.Configuration.GetValue<bool>("Database:EnsureCreatedOnStartup"))
{
    await app.Services.EnsureDatabaseCreatedAsync();
}

app.MapHealthChecks("/health");
app.MapShipmentEndpoints();

await app.RunAsync();

// Exposes Program to WebApplicationFactory<Program> in the integration tests.
public partial class Program;
