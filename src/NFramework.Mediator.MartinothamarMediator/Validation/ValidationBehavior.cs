using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Validation;

namespace NFramework.Mediator.MartinothamarMediator.Validation;

/// <summary>
/// Martinothamar Mediator implementation for request validation using FluentValidation.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<FluentValidation.IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger
) : ValidationBehaviorBase<TRequest, TResponse>(logger), IPipelineBehavior<TRequest, TResponse>
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
        return await HandleAsync(request, ct => next(request, ct), cancellationToken);
    }

    private sealed class FluentValidatorAdapter : IValidator<TRequest>
    {
        private readonly FluentValidation.IValidator<TRequest> _validator;

        public FluentValidatorAdapter(FluentValidation.IValidator<TRequest> validator)
        {
            _validator = validator;
        }

        public async ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(
            TRequest instance,
            CancellationToken cancellationToken
        )
        {
            var result = await _validator.ValidateAsync(instance, cancellationToken);
            var errors = result.Errors.Select(e => new ValidationErrorAdapter(e)).ToList<IValidationError>();
            return errors;
        }
    }

    private sealed class ValidationErrorAdapter : IValidationError
    {
        private readonly FluentValidation.Results.ValidationFailure _failure;

        public ValidationErrorAdapter(FluentValidation.Results.ValidationFailure failure)
        {
            _failure = failure;
        }

        public string Code => _failure.ErrorCode;
        public string Message => _failure.ErrorMessage;
        public string? PropertyName => _failure.PropertyName;
    }
}
