namespace NFramework.Mediator.Abstractions.Contracts.Pipeline;

/// <summary>
/// Pipeline behavior for cross-cutting concerns (logging, validation, caching, etc.).
/// Behaviors are executed in order for each request/response pair.
/// </summary>
/// <typeparam name="TRequest">The request type being processed.</typeparam>
/// <typeparam name="TResponse">The response type returned.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
{
    ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    );
}
