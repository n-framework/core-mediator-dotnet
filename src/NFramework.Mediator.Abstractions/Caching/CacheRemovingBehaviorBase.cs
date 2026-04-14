using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Caching;

/// <summary>
/// Invalidates the cache for specified patterns ONLY if the request completes successfully.
/// </summary>
public abstract class CacheRemovingBehaviorBase<TRequest, TResponse>
{
    private readonly ILogger<CacheRemovingBehaviorBase<TRequest, TResponse>> _logger;

    protected CacheRemovingBehaviorBase(ILogger<CacheRemovingBehaviorBase<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not ICacheRemoverRequest remover)
        {
            return await next(cancellationToken);
        }

        var response = await next(cancellationToken);

        if (remover.CacheKeyPatterns != null)
        {
            foreach (string pattern in remover.CacheKeyPatterns)
            {
                try
                {
                    await RemoveFromCacheAsync(pattern, cancellationToken);
                    _logger.LogDebug("Cache removed for pattern/group: {CacheKey}", pattern);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Failed to remove cache for pattern/group: {CacheKey}", pattern);
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
