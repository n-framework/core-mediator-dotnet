using Mediator;
using NFramework.Mediator.Abstractions.Behaviors;

namespace NFramework.Mediator.MartinothamarMediator.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not ISecuredRequest secured)
        {
            return await next(request, cancellationToken);
        }

        // Implementation requires tying into specific Identity/Auth abstraction
        var requiredRoles = secured.RequiredRoles ?? Array.Empty<string>();
        var requiredOperations = secured.RequiredOperations ?? Array.Empty<string>();

        return await next(request, cancellationToken);
    }
}
