using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions;
using NFramework.Mediator.Abstractions.Validation;
using UnionRailway;
using NFramework.Mediator.Mediator.Railway;

namespace NFramework.Mediator.Mediator.Validation;

/// <summary>
/// Validates <see cref="IRailRequest{TValue}"/> requests and short-circuits the pipeline with a
/// railway validation failure instead of throwing, so expected validation problems flow as data.
/// </summary>
/// <typeparam name="TRequest">The request type being validated.</typeparam>
/// <typeparam name="TValue">The success value carried by the request's rail response.</typeparam>
public sealed class ValidationBehavior<TRequest, TValue>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TValue>> logger
) : IPipelineBehavior<TRequest, Rail<TValue>>
    where TRequest : IRailRequest<TValue>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;
    private readonly ILogger<ValidationBehavior<TRequest, TValue>> _logger = logger;

    private static readonly Action<ILogger, string, string, Exception?> LogValidationFailureAction =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(1, nameof(Handle)),
            "Validation failed for {RequestName}. Errors: {Errors}"
        );

    /// <inheritdoc />
    public async ValueTask<Rail<TValue>> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, Rail<TValue>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (!_validators.Any())
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }

        IReadOnlyCollection<IValidationError>[] validationResults = await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(request, cancellationToken).AsTask())
            )
            .ConfigureAwait(false);

        List<IValidationError> failures = [.. validationResults.SelectMany(result => result).Where(failure => failure != null)];

        if (failures.Count == 0)
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }

        if (_logger.IsEnabled(LogLevel.Warning))
        {
            LogValidationFailureAction(
                _logger,
                typeof(TRequest).Name,
                string.Join("; ", failures.Select(failure => $"{failure.Code}: {failure.Message}")),
                null
            );
        }

        return failures.ToValidationFailure<TValue>();
    }
}
