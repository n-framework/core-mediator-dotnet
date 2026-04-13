using FluentValidation;
using Mediator;
using NFramework.Mediator.Abstractions.Behaviors;

namespace NFramework.Mediator.MartinothamarMediator.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IValidatableRequest)
        {
            return await next(request, cancellationToken);
        }

        if (validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next(request, cancellationToken);
    }
}
