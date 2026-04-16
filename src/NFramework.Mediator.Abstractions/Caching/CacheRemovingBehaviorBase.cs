using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Caching;

/// <summary>
/// Invalidates the cache for specified patterns ONLY if the request completes successfully.
/// </summary>
public abstract class CacheRemovingBehaviorBase<TRequest, TResponse>(
    ILogger<CacheRemovingBehaviorBase<TRequest, TResponse>> logger
)
{
    private readonly ILogger<CacheRemovingBehaviorBase<TRequest, TResponse>> _logger = logger;

    private static readonly Action<ILogger, string, Exception?> LogCacheRemovedAction = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(1, nameof(HandleAsync)),
        "Cache removed for pattern/group: {CacheKey}"
    );

    private static readonly Action<ILogger, string, Exception?> LogCacheRemovalErrorAction =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, nameof(HandleAsync)),
            "Failed to remove cache for pattern/group: {CacheKey}"
        );

    protected static readonly Action<ILogger, string, string, Exception?> LogRemovedKeyFromGroup = LoggerMessage.Define<
        string,
        string
    >(LogLevel.Debug, new EventId(3, nameof(LogRemovedKeyFromGroup)), "Removed key {CacheKey} from group {GroupKey}");

    protected static readonly Action<ILogger, string, Exception?> LogCorruptedGroupKey = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(4, nameof(LogCorruptedGroupKey)),
        "Pattern {Pattern} is not a valid group key JSON or data is corrupted. Deleting corrupted group key."
    );

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not ICacheRemoverRequest remover)
            return await next(cancellationToken).ConfigureAwait(false);

        var response = await next(cancellationToken).ConfigureAwait(false);

        if (remover.CacheKeyPatterns != null)
        {
            foreach (string pattern in remover.CacheKeyPatterns)
            {
                try
                {
                    await RemoveFromCacheAsync(pattern, cancellationToken).ConfigureAwait(false);
                    LogCacheRemovedAction(_logger, pattern, null);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    LogCacheRemovalErrorAction(_logger, pattern, ex);
                }
            }
        }

        return response;
    }

    /// <summary>
    /// Implement to remove data from the cache. Should support both single keys and logical groups/patterns.
    /// </summary>
    protected abstract ValueTask RemoveFromCacheAsync(string pattern, CancellationToken cancellationToken);
}
