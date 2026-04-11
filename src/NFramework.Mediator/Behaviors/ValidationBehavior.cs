using System.Collections.ObjectModel;
using NFramework.Mediator.Configuration;

namespace NFramework.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that validates requests before passing to handlers.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : global::Mediator.IPipelineBehavior<TRequest, TResponse>
    where TRequest : global::Mediator.IMessage
{
    private readonly IReadOnlyList<IRequestValidator<TRequest>> _validators;
    private readonly IValidationFailureResponseFactory<TResponse>? _failureResponseFactory;
    private readonly IRequestPipelinePolicyProvider _pipelinePolicyProvider;

    /// <summary>
    /// Creates a new validation behavior instance.
    /// </summary>
    /// <param name="validators">The request validators.</param>
    /// <param name="failureResponseFactory">Optional factory for creating failure responses.</param>
    /// <param name="pipelinePolicyProvider">The pipeline policy provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when validators or pipelinePolicyProvider is null.</exception>
    public ValidationBehavior(
        IEnumerable<IRequestValidator<TRequest>> validators,
        IValidationFailureResponseFactory<TResponse>? failureResponseFactory,
        IRequestPipelinePolicyProvider pipelinePolicyProvider
    )
    {
        ArgumentNullException.ThrowIfNull(validators);
        ArgumentNullException.ThrowIfNull(pipelinePolicyProvider);

        _validators = validators.ToArray();
        _failureResponseFactory = failureResponseFactory;
        _pipelinePolicyProvider = pipelinePolicyProvider;
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TRequest message,
        global::Mediator.MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var configuration = _pipelinePolicyProvider.GetConfiguration(typeof(TRequest));
        if (!configuration.UseValidation)
        {
            return await next(message, cancellationToken);
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
                $"Validation failed for '{typeof(TRequest).FullName}', but no {nameof(IValidationFailureResponseFactory<TResponse>)} is registered for '{typeof(TResponse).FullName}'.\n"
                    + $"Registration example:\n"
                    + $"  services.AddMediator(options =>\n"
                    + $"  {{\n"
                    + $"      options.BehaviorOptions.UseValidation();\n"
                    + $"      services.AddTransient<IValidationFailureResponseFactory<{typeof(TResponse).FullName}>, YourResponseFactory>();\n"
                    + $"  }});"
            );
        }

        return _failureResponseFactory.Create(new ReadOnlyCollection<IValidationError>(errors));
    }
}
