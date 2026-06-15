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
    private static readonly Action<ILogger, string, Exception?> LogUserNotAuthenticated = LoggerMessage.Define<string>(
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
            LogUserLacksRequiredRoles(logger, string.Join(", ", requiredRoles), typeof(TRequest).Name, null);
            return new UnionError.Forbidden("Insufficient permissions.");
        }

        if (requiredOperations.Count > 0 && !securityContext.HasAllOperations(requiredOperations))
        {
            LogUserLacksRequiredPermissions(logger, string.Join(", ", requiredOperations), typeof(TRequest).Name, null);
            return new UnionError.Forbidden("Insufficient permissions.");
        }

        return await next(request, cancellationToken).ConfigureAwait(false);
    }
}
