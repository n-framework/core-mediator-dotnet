using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions;
using NFramework.Mediator.Abstractions.Authorization;
using UnionRailway;

namespace NFramework.Mediator.Mediator.Authorization;

/// <summary>
/// Authorizes <see cref="IRailRequest{TValue}"/> requests implementing <see cref="ISecuredRequest"/>
/// and short-circuits the pipeline with a railway failure instead of throwing.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TValue>(
    ISecurityContext securityContext,
    ILogger<AuthorizationBehavior<TRequest, TValue>> logger
) : IPipelineBehavior<TRequest, Rail<TValue>>
    where TRequest : IRailRequest<TValue>
{
    private static readonly Action<ILogger, string, Exception?> LogUserNotAuthenticated =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, nameof(Handle)),
            "Authorization failed: User is not authenticated for request {RequestName}"
        );

    private static readonly Action<ILogger, string, string, Exception?> LogUserLacksRequiredRoles =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(2, nameof(Handle)),
            "Authorization failed: User lacks required roles ({Roles}) for request {RequestName}"
        );

    private static readonly Action<ILogger, string, string, Exception?> LogUserLacksRequiredPermissions =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(3, nameof(Handle)),
            "Authorization failed: User lacks required permissions ({Permissions}) for request {RequestName}"
        );

    /// <inheritdoc />
    public async ValueTask<Rail<TValue>> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, Rail<TValue>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not ISecuredRequest secured)
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }

        var requiredRoles = secured.RequiredRoles ?? [];
        var requiredOperations = secured.RequiredOperations ?? [];

        if (requiredRoles.Count == 0 && requiredOperations.Count == 0)
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }

        if (!securityContext.IsAuthenticated)
        {
            LogUserNotAuthenticated(logger, typeof(TRequest).Name, null);
            return new UnionError.Unauthorized();
        }

        if (requiredRoles.Count > 0 && !securityContext.HasAnyRole(requiredRoles))
        {
            string roles = string.Join(", ", requiredRoles);
            string requestName = typeof(TRequest).Name;
            LogUserLacksRequiredRoles(logger, roles, requestName, null);
            return new UnionError.Forbidden($"User lacks required roles ({roles}) for request {requestName}");
        }

        if (requiredOperations.Count > 0 && !securityContext.HasAllOperations(requiredOperations))
        {
            string operations = string.Join(", ", requiredOperations);
            string requestName = typeof(TRequest).Name;
            LogUserLacksRequiredPermissions(logger, operations, requestName, null);
            return new UnionError.Forbidden($"User lacks required permissions ({operations}) for request {requestName}");
        }

        return await next(request, cancellationToken).ConfigureAwait(false);
    }
}
