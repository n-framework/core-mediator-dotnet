using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Authorization;

namespace NFramework.Mediator.Mediator.Authorization;

/// <summary>
/// Martinothamar Mediator implementation for role and operation based authorization.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ISecurityContext securityContext,
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger
) : AuthorizationBehaviorBase<TRequest, TResponse>, IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    protected override ValueTask AuthorizeAsync(
        TRequest request,
        IReadOnlyList<string> requiredRoles,
        IReadOnlyList<string> requiredOperations,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(requiredRoles);
        ArgumentNullException.ThrowIfNull(requiredOperations);

        if (requiredRoles.Count == 0 && requiredOperations.Count == 0)
        {
            return default;
        }

        if (!securityContext.IsAuthenticated)
        {
            LogUserNotAuthenticated(logger, typeof(TRequest).Name, null);
            throw new UnauthorizedAccessException("You are not authenticated.");
        }

        if (requiredRoles.Count > 0 && !securityContext.HasAnyRole(requiredRoles))
        {
            string roles = string.Join(", ", requiredRoles);
            string requestName = typeof(TRequest).Name;
            LogUserLacksRequiredRoles(logger, roles, requestName, null);
            throw new UnauthorizedAccessException(
                $"Authorization failed: User lacks required roles ({roles}) for request {requestName}"
            );
        }

        if (requiredOperations.Count > 0 && !securityContext.HasAllOperations(requiredOperations))
        {
            string operations = string.Join(", ", requiredOperations);
            string requestName = typeof(TRequest).Name;
            LogUserLacksRequiredPermissions(logger, operations, requestName, null);
            throw new UnauthorizedAccessException(
                $"Authorization failed: User lacks required permissions ({operations}) for request {requestName}"
            );
        }

        return default;
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return await HandleAsync(request, ct => next(request, ct), cancellationToken).ConfigureAwait(false);
    }
}
