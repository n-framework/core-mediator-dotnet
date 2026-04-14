using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Performance;

/// <summary>
/// Monitors request execution time and logs a warning if it exceeds <see cref="SlowRequestThresholdMs"/>.
/// </summary>
public abstract class PerformanceBehaviorBase<TRequest, TResponse>
{
    private readonly ILogger<PerformanceBehaviorBase<TRequest, TResponse>> _logger;

    protected PerformanceBehaviorBase(ILogger<PerformanceBehaviorBase<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Requests exceeding this threshold (in milliseconds) will trigger a warning. Default is 500ms.
    /// </summary>
    protected virtual int SlowRequestThresholdMs => 500;

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not IIntervalRequest)
        {
            return await next(cancellationToken);
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            return response;
        }
        finally
        {
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > SlowRequestThresholdMs)
            {
                string requestName = typeof(TRequest).Name;
                _logger.LogWarning(
                    "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)",
                    requestName,
                    elapsedMilliseconds
                );
            }
        }
    }
}
