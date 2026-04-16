using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Validation;

namespace NFramework.Mediator.Mediator.Validation.FluentValidation;

/// <summary>
/// Martinothamar Mediator implementation for request validation using FluentValidation.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<global::FluentValidation.IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger,
    IValidationExceptionFactory? exceptionFactory = null
) : ValidationBehaviorBase<TRequest, TResponse>(logger, exceptionFactory), IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    protected override IEnumerable<IValidator<TRequest>> GetValidators()
    {
        return validators.Select(v => new FluentValidatorAdapter(v));
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return await HandleAsync(request, ct => next(request, ct), cancellationToken).ConfigureAwait(false);
    }

    private sealed class FluentValidatorAdapter(global::FluentValidation.IValidator<TRequest> validator)
        : IValidator<TRequest>
    {
        public async ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(
            TRequest instance,
            CancellationToken cancellationToken
        )
        {
            var result = await validator.ValidateAsync(instance, cancellationToken).ConfigureAwait(false);
            if (result == null)
            {
                return Array.Empty<IValidationError>();
            }

            List<IValidationError> errors =
            [
                .. result.Errors.Where(e => e != null).Select(e => new ValidationErrorAdapter(e)),
            ];
            return errors;
        }
    }

    private sealed class ValidationErrorAdapter(global::FluentValidation.Results.ValidationFailure failure)
        : IValidationError
    {
        private readonly global::FluentValidation.Results.ValidationFailure _failure = failure;

        public string Code => _failure.ErrorCode;
        public string Message => _failure.ErrorMessage;
        public string? PropertyName => _failure.PropertyName;
    }
}
