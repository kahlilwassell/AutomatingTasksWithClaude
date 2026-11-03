using Microsoft.AspNetCore.Http.HttpResults;
using ShipmentTracker.Application.Common;

namespace ShipmentTracker.Api.ErrorHandling;

/// <summary>Translates Application <see cref="Error"/>s into RFC 9457 problem responses.</summary>
internal static class ErrorMapper
{
    public static ProblemHttpResult ToProblem(Error error)
    {
        var (status, title) = error.Type switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Resource not found"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation failed"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error"),
        };

        return TypedResults.Problem(
            title: title,
            detail: error.Message,
            statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
