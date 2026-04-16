using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Validation;

/// <summary>
/// Orchestrates input validation and logs failures as warnings to prevent silent invalid state processing.
/// </summary>
public abstract class ValidationBehaviorBase<TRequest, TResponse>(
    ILogger<ValidationBehaviorBase<TRequest, TResponse>> logger,
    IValidationExceptionFactory? exceptionFactory = null
)
{
    private readonly ILogger<ValidationBehaviorBase<TRequest, TResponse>> _logger = logger;
    private readonly IValidationExceptionFactory? _exceptionFactory = exceptionFactory;

    private static readonly Action<ILogger, string, string, Exception?> LogValidationFailureAction =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(1, nameof(HandleAsync)),
            "Validation failed for {RequestName}. Errors: {Errors}"
        );

    /// <returns>The response from the next handler in the pipeline.</returns>
    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not IValidatableRequest)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var validators = GetValidators();
        if (validators.Any())
        {
            var validationResults = await Task.WhenAll(
                    validators.Select(v => v.ValidateAsync(request, cancellationToken).AsTask())
                )
                .ConfigureAwait(false);

            List<IValidationError> failures = [.. validationResults.SelectMany(r => r).Where(f => f != null)];

            if (failures.Count != 0)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    LogValidationFailureAction(
                        _logger,
                        typeof(TRequest).Name,
                        string.Join("; ", failures.Select(f => $"{f.Code}: {f.Message}")),
                        null
                    );
                }

                throw CreateValidationException(failures);
            }
        }

        return await next(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Implement to provide the validators for the request.
    /// </summary>
    /// <returns>An enumeration of validators.</returns>
    protected abstract IEnumerable<IValidator<TRequest>> GetValidators();

    /// <summary>
    /// Uses <see cref="IValidationExceptionFactory"/> if provided; otherwise falls back to <see cref="ValidationException"/>.
    /// Override to support specific validation libraries' exception types.
    /// </summary>
    protected virtual Exception CreateValidationException(IEnumerable<IValidationError> errors)
    {
        return _exceptionFactory is not null
            ? _exceptionFactory.CreateValidationException(errors)
            : new ValidationException(errors);
    }
}
