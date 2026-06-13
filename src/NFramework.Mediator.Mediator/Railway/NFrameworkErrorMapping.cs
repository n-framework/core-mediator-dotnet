using NFramework.Mediator.Abstractions.Validation;
using UnionRailway;

namespace NFramework.Mediator.Mediator.Railway;

/// <summary>
/// Translates NFramework framework exceptions into UnionRailway <see cref="UnionError"/> cases.
/// The mapping is a pure, allocation-light cascade with no reflection, keeping the adapter
/// Native AOT compatible.
/// </summary>
public static class NFrameworkErrorMapping
{
    /// <summary>
    /// Key used to group validation messages that are not associated with a specific property.
    /// </summary>
    public const string UnscopedValidationKey = "$";

    private const string ConcurrencyConflictExceptionTypeName = "ConcurrencyConflictException";

    /// <summary>
    /// Maps an exception to its corresponding <see cref="UnionError"/>.
    /// </summary>
    /// <param name="exception">The exception thrown by the framework pipeline or a handler.</param>
    /// <returns>The semantic error describing the failure.</returns>
    public static UnionError ToUnionError(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            ValidationException validation => ToValidationError(validation),
            UnauthorizedAccessException => new UnionError.Unauthorized(),
            _ when IsConcurrencyConflict(exception) => new UnionError.Conflict(exception.Message),
            _ => new UnionError.SystemFailure(exception),
        };
    }

    /// <summary>
    /// Determines whether the framework recognizes the exception as a framework error with a
    /// dedicated <see cref="UnionError"/> mapping (as opposed to an unexpected system failure).
    /// </summary>
    /// <param name="exception">The exception to inspect.</param>
    /// <returns><c>true</c> when a non-system mapping applies; otherwise <c>false</c>.</returns>
    public static bool IsMappedFrameworkError(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception is ValidationException or UnauthorizedAccessException || IsConcurrencyConflict(exception);
    }

    private static UnionError ToValidationError(ValidationException exception)
    {
        Dictionary<string, string[]> fields = exception
            .Errors.GroupBy(error => NormalizePropertyName(error.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray());

        return UnionError.CreateValidation(fields);
    }

    /// <summary>
    /// The persistence package is not referenced here to keep the mediator framework free of a
    /// persistence dependency, so the concurrency conflict is matched by its type name.
    /// </summary>
    private static bool IsConcurrencyConflict(Exception exception) =>
        string.Equals(exception.GetType().Name, ConcurrencyConflictExceptionTypeName, StringComparison.Ordinal);

    private static string NormalizePropertyName(string? propertyName) =>
        string.IsNullOrWhiteSpace(propertyName) ? UnscopedValidationKey : propertyName;
}
