using System.Threading;
using System.Threading.Tasks;
using Mediator;
using NFramework.Mediator.Abstractions.Authorization;

namespace NFramework.Mediator.MartinothamarMediator.Authorization;

/// <summary>
/// Martinothamar Mediator implementation for role and operation based authorization.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(ISecurityContext securityContext)
    : AuthorizationBehaviorBase<TRequest, TResponse>,
        IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    protected override ValueTask AuthorizeAsync(
        TRequest request,
        IReadOnlyList<string> requiredRoles,
        IReadOnlyList<string> requiredOperations,
        CancellationToken cancellationToken
    )
    {
        if (requiredRoles.Count == 0 && requiredOperations.Count == 0)
        {
            return default;
        }

        if (!securityContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("You are not authenticated.");
        }

        if (requiredRoles.Count > 0 && !securityContext.HasAnyRole(requiredRoles))
        {
            throw new UnauthorizedAccessException("You don't have the required roles to perform this operation.");
        }

        if (requiredOperations.Count > 0 && !securityContext.HasAllOperations(requiredOperations))
        {
            throw new UnauthorizedAccessException("You don't have the required permissions to perform this operation.");
        }

        return default;
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return await HandleAsync(request, ct => next(request, ct), cancellationToken);
    }
}
