using Microsoft.Extensions.DependencyInjection;
using ShipmentTracker.Application.Shipments;

namespace ShipmentTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IShipmentService, ShipmentService>();
        return services;
    }
}
