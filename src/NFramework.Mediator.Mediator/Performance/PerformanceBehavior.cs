using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Performance;

namespace NFramework.Mediator.Mediator.Performance;

/// <summary>
/// Martinothamar Mediator implementation for execution time monitoring.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
    PerformanceOptions options
) : PerformanceBehaviorBase<TRequest, TResponse>(logger), IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    protected override int SlowRequestThresholdMs => (int)options.PerformanceThreshold.TotalMilliseconds;

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return await HandleAsync(request, ct => next(request, ct), cancellationToken).ConfigureAwait(false);
    }
}
