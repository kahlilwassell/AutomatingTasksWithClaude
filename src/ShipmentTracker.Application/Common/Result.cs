namespace ShipmentTracker.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
}

/// <summary>An expected, non-exceptional failure. The API maps <see cref="Type"/> to an HTTP status.</summary>
public sealed record Error(ErrorType Type, string Code, string Message)
{
    public static Error NotFound(string code, string message) => new(ErrorType.NotFound, code, message);
    public static Error Conflict(string code, string message) => new(ErrorType.Conflict, code, message);
    public static Error Validation(string code, string message) => new(ErrorType.Validation, code, message);
}

/// <summary>
/// Either a value or an <see cref="Error"/>. Use for outcomes callers are expected
/// to handle (not found, conflicts). Use exceptions only for truly unexpected failures.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(Error error)
    {
        Error = error;
    }

    public bool IsSuccess { get; }

    public Error? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(Error!);
}

/// <summary>Marker value for operations that succeed without returning data.</summary>
public readonly record struct Success;
