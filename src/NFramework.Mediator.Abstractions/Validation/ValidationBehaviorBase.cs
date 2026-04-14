using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Validation;

/// <summary>
/// Orchestrates input validation and logs failures as warnings to prevent silent invalid state processing.
/// </summary>
public abstract class ValidationBehaviorBase<TRequest, TResponse>
{
    private readonly ILogger<ValidationBehaviorBase<TRequest, TResponse>> _logger;
    private readonly IValidationExceptionFactory? _exceptionFactory;

    protected ValidationBehaviorBase(
        ILogger<ValidationBehaviorBase<TRequest, TResponse>> logger,
        IValidationExceptionFactory? exceptionFactory = null
    )
    {
        _logger = logger;
        _exceptionFactory = exceptionFactory;
    }

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not IValidatableRequest)
        {
            return await next(cancellationToken);
        }

        var validators = GetValidators();
        if (validators.Any())
        {
            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(request, cancellationToken).AsTask())
            );

            var failures = validationResults.SelectMany(r => r).Where(f => f != null).ToList();

            if (failures.Count != 0)
            {
                _logger.LogWarning(
                    "Validation failed for {RequestName}. Errors: {Errors}",
                    typeof(TRequest).Name,
                    string.Join("; ", failures.Select(f => $"{f.Code}: {f.Message}"))
                );

                throw CreateValidationException(failures);
            }
        }

        return await next(cancellationToken);
    }

    protected abstract IEnumerable<IValidator<TRequest>> GetValidators();

    /// <summary>
    /// Uses <see cref="IValidationExceptionFactory"/> if provided; otherwise falls back to <see cref="InvalidOperationException"/>.
    /// Override to support specific validation libraries' exception types.
    /// </summary>
    protected virtual Exception CreateValidationException(IEnumerable<IValidationError> errors)
    {
        if (_exceptionFactory != null)
        {
            return _exceptionFactory.CreateValidationException(errors);
        }

        string errorMessages = string.Join("; ", errors.Select(e => $"{e.Code}: {e.Message}"));
        return new InvalidOperationException($"Validation failed: {errorMessages}");
    }
}
