using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Logging;

namespace NFramework.Mediator.Mediator.Logging;

/// <summary>
/// Martinothamar Mediator implementation for formatted JSON request/response logging.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : LoggingBehaviorBase<TRequest, TResponse>(logger),
        IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return await HandleAsync(request, ct => next(request, ct), cancellationToken).ConfigureAwait(false);
    }
}
