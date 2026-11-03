using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShipmentTracker.Domain.Common;

namespace ShipmentTracker.Api.ErrorHandling;

/// <summary>
/// Turns known exceptions into 4xx problem responses. Anything else falls through
/// to the default handler and becomes a 500 without leaking internals.
/// </summary>
internal sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<ApiExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Domain rule violated"),
            BadHttpRequestException bad => (bad.StatusCode, "Bad request"),
            _ => (0, string.Empty),
        };

        if (status == 0)
        {
            return false;
        }

        logger.LogInformation(exception, "Handled {ExceptionType} as HTTP {StatusCode}", exception.GetType().Name, status);
        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = exception.Message },
        });
    }
}
