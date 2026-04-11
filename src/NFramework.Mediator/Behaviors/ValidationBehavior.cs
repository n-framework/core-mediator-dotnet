using System.Collections.ObjectModel;
using NFramework.Mediator.Configuration;

namespace NFramework.Mediator.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : global::Mediator.IPipelineBehavior<TRequest, TResponse>
    where TRequest : global::Mediator.IMessage
{
    private readonly IReadOnlyList<IRequestValidator<TRequest>> _validators;
    private readonly IValidationFailureResponseFactory<TResponse>? _failureResponseFactory;
    private readonly IRequestPipelinePolicyProvider? _pipelinePolicyProvider;

    public ValidationBehavior(
        IEnumerable<IRequestValidator<TRequest>> validators,
        IValidationFailureResponseFactory<TResponse>? failureResponseFactory = null,
        IRequestPipelinePolicyProvider? pipelinePolicyProvider = null
    )
    {
        _validators = validators.ToArray();
        _failureResponseFactory = failureResponseFactory;
        _pipelinePolicyProvider = pipelinePolicyProvider;
    }

    public async ValueTask<TResponse> Handle(
        TRequest message,
        global::Mediator.MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (_pipelinePolicyProvider is not null)
        {
            var configuration = _pipelinePolicyProvider.GetConfiguration(typeof(TRequest));
            if (!configuration.UseValidation)
            {
                return await next(message, cancellationToken);
            }
        }

        if (_validators.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        List<IValidationError>? errors = null;

        foreach (var validator in _validators)
        {
            var validationErrors = await validator.ValidateAsync(message, cancellationToken);
            if (validationErrors.Count == 0)
            {
                continue;
            }

            errors ??= new List<IValidationError>(validationErrors.Count);
            errors.AddRange(validationErrors);
        }

        if (errors is null || errors.Count == 0)
        {
            return await next(message, cancellationToken);
        }

        if (_failureResponseFactory is null)
        {
            throw new InvalidOperationException(
                $"Validation failed for '{typeof(TRequest).FullName}', but no {nameof(IValidationFailureResponseFactory<TResponse>)} is registered for '{typeof(TResponse).FullName}'."
            );
        }

        return _failureResponseFactory.Create(new ReadOnlyCollection<IValidationError>(errors));
    }
}
