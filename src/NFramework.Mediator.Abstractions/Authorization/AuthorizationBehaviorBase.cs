namespace NFramework.Mediator.Abstractions.Authorization;

/// <summary>
/// Provides reusable authorization logic that checks <see cref="ISecuredRequest"/> requirements
/// against the target implementation's <see cref="AuthorizeAsync"/> logic.
/// </summary>
public abstract class AuthorizationBehaviorBase<TRequest, TResponse>
{
    protected AuthorizationBehaviorBase() { }

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not ISecuredRequest secured)
        {
            return await next(cancellationToken);
        }

        string[] requiredRoles = secured.RequiredRoles ?? [];
        string[] requiredOperations = secured.RequiredOperations ?? [];

        await AuthorizeAsync(request, requiredRoles, requiredOperations, cancellationToken);

        return await next(cancellationToken);
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
