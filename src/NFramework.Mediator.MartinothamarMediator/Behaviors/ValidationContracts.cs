namespace NFramework.Mediator.MartinothamarMediator.Behaviors;

/// <summary>
/// Represents a validation error with code and message.
/// </summary>
public interface IValidationError
{
    /// <summary>
    /// Gets the error code.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the error message.
    /// </summary>
    string Message { get; }
}

/// <summary>
/// Validator for requests.
/// </summary>
/// <typeparam name="TRequest">The request type to validate.</typeparam>
public interface IRequestValidator<TRequest>
{
    /// <summary>
    /// Validates the request asynchronously.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A collection of validation errors (empty if valid).</returns>
    ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(
        TRequest request,
        CancellationToken cancellationToken
    );
}

/// <summary>
/// Factory for creating failure responses when validation fails.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IValidationFailureResponseFactory<TResponse>
{
    /// <summary>
    /// Creates a failure response from validation errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>A failure response.</returns>
    TResponse Create(IReadOnlyCollection<IValidationError> errors);
}
