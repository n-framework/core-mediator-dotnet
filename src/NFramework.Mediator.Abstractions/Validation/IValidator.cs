namespace NFramework.Mediator.Abstractions.Validation;

/// <summary>
/// Provides an abstraction over specific validation libraries (FluentValidation, DataAnnotations, etc.).
/// </summary>
public interface IValidator<in T>
{
    /// <summary>
    /// Validates the given request.
    /// </summary>
    /// <returns>A collection of validation errors. If the collection is empty, the request is valid.</returns>
    ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(T instance, CancellationToken cancellationToken);
}

/// <summary>
/// Represents a validation error.
/// </summary>
public interface IValidationError
{
    /// <summary>
    /// Gets the unique error code.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the localized error message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets the name of the property that failed validation, if applicable.
    /// </summary>
    string? PropertyName { get; }
}

/// <summary>
/// Factory for creating consistency in validation exceptions across different validator implementations.
/// </summary>
public interface IValidationExceptionFactory
{
    /// <summary>
    /// Creates a new exception populated with the provided validation errors.
    /// </summary>
    /// <returns>A new <see cref="Exception"/> instance (typically a <see cref="ValidationException"/>).</returns>
    Exception CreateValidationException(IEnumerable<IValidationError> errors);
}
