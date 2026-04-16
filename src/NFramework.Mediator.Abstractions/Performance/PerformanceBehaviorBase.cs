using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Performance;

/// <summary>
/// Monitors request execution time and logs a warning if it exceeds <see cref="SlowRequestThresholdMs"/>.
/// </summary>
public abstract class PerformanceBehaviorBase<TRequest, TResponse>(
    ILogger<PerformanceBehaviorBase<TRequest, TResponse>> logger
)
{
    private readonly ILogger<PerformanceBehaviorBase<TRequest, TResponse>> _logger = logger;

    private static readonly Action<ILogger, string, long, Exception?> LogSlowRequestAction = LoggerMessage.Define<
        string,
        long
    >(
        LogLevel.Warning,
        new EventId(1, nameof(HandleAsync)),
        "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)"
    );

    /// <summary>
    /// Requests exceeding this threshold (in milliseconds) will trigger a warning. Default is 500ms.
    /// </summary>
    protected virtual int SlowRequestThresholdMs => 500;

    /// <returns>The response from the next handler in the pipeline.</returns>
    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not IIntervalRequest)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken).ConfigureAwait(false);
            return response;
        }
        finally
        {
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            int threshold = SlowRequestThresholdMs;

            if (threshold > 0 && elapsedMilliseconds > threshold)
            {
                LogSlowRequestAction(_logger, typeof(TRequest).Name, elapsedMilliseconds, null);
            }
        }
    }
}
