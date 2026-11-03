using System.Text.Json.Serialization;
using ShipmentTracker.Api.ErrorHandling;

namespace ShipmentTracker.Api;

public static class ApiServiceCollectionExtensions
{
    /// <summary>HTTP-layer services: JSON, validation and RFC 9457 problem details.</summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddValidation();
        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();
        return services;
    }
}
