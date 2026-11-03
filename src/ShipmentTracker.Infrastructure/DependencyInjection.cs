using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShipmentTracker.Application.Abstractions;
using ShipmentTracker.Infrastructure.Persistence;
using ShipmentTracker.Infrastructure.Persistence.Repositories;

namespace ShipmentTracker.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "ShipmentTracker";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // The connection string is resolved lazily so tests (and any other
        // configuration source) can override it after registration.
        services.AddDbContext<ShipmentTrackerDbContext>((sp, options) =>
        {
            var connectionString = sp.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStringName)
                ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ShipmentTrackerDbContext>());
        services.AddScoped<IShipmentRepository, ShipmentRepository>();

        services.AddHealthChecks().AddDbContextCheck<ShipmentTrackerDbContext>("database");
        return services;
    }
}
