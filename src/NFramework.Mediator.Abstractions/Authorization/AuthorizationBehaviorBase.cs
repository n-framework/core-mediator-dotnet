using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Authorization;

public abstract class AuthorizationBehaviorBase<TRequest, TResponse>
{
    protected static readonly Action<ILogger, string, Exception?> LogUserNotAuthenticated =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(1, nameof(LogUserNotAuthenticated)),
            "Authorization failed: User is not authenticated for request {RequestName}"
        );

    protected static readonly Action<ILogger, string, string, Exception?> LogUserLacksRequiredRoles =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(2, nameof(LogUserLacksRequiredRoles)),
            "Authorization failed: User lacks required roles ({Roles}) for request {RequestName}"
        );

    protected static readonly Action<ILogger, string, string, Exception?> LogUserLacksRequiredPermissions =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(3, nameof(LogUserLacksRequiredPermissions)),
            "Authorization failed: User lacks required permissions ({Permissions}) for request {RequestName}"
        );

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not ISecuredRequest secured)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var requiredRoles = secured.RequiredRoles ?? [];
        var requiredOperations = secured.RequiredOperations ?? [];

        await AuthorizeAsync(request, requiredRoles, requiredOperations, cancellationToken).ConfigureAwait(false);

        return await next(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Implement this to verify if the request should be allowed based on provided requirements.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Thrown when authorization fails.</exception>
    protected abstract ValueTask AuthorizeAsync(
        TRequest request,
        IReadOnlyList<string> requiredRoles,
        IReadOnlyList<string> requiredOperations,
        CancellationToken cancellationToken
    );
}
