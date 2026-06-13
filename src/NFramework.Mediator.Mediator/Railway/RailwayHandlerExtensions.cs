using NFramework.Mediator.Abstractions.Validation;
using UnionRailway;

namespace NFramework.Mediator.Mediator.Railway;

/// <summary>
/// Helpers for handlers that produce <see cref="Rail{T}"/> results using NFramework's validation vocabulary.
/// </summary>
public static class RailwayHandlerExtensions
{
    /// <summary>
    /// Builds a failed <see cref="Rail{T}"/> from a collection of NFramework validation errors,
    /// grouping messages by property to match UnionRailway's validation problem shape.
    /// </summary>
    /// <typeparam name="T">The success type of the rail.</typeparam>
    /// <param name="errors">The validation errors to report.</param>
    /// <returns>A rail in the failed state carrying a validation error.</returns>
    public static Rail<T> ToValidationFailure<T>(this IEnumerable<IValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        Dictionary<string, string[]> fields = errors
            .Where(error => error is not null)
            .GroupBy(error =>
                string.IsNullOrWhiteSpace(error.PropertyName)
                    ? NFrameworkErrorMapping.UnscopedValidationKey
                    : error.PropertyName
            )
            .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray());

        return new UnionError.Validation(fields);
    }
}
