using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Caching;

/// <summary>
/// Standardizes the "get-if-cached-else-fetch-and-store" logic.
/// Errors during cache operations are logged but do not disrupt the request lifecycle.
/// </summary>
public abstract class CachingBehaviorBase<TRequest, TResponse>
{
    private readonly ILogger<CachingBehaviorBase<TRequest, TResponse>> _logger;

    protected CachingBehaviorBase(ILogger<CachingBehaviorBase<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not ICacheableRequest cacheable)
        {
            return await next(cancellationToken);
        }

        string requestName = typeof(TRequest).Name;
        string cacheKey = GetCacheKey(request, cacheable, requestName);

        try
        {
            byte[]? cachedResponse = await GetFromCacheAsync(cacheKey, cancellationToken);
            if (cachedResponse != null)
            {
                var deserialized = DeserializeResponse(cachedResponse);
                if (deserialized is not null)
                {
                    _logger.LogDebug("Fetched from cache: {CacheKey}", cacheKey);
                    return deserialized;
                }
                else
                {
                    _logger.LogWarning("Cache contained null data for key: {CacheKey}. Fetching fresh data.", cacheKey);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to get from cache for key: {CacheKey}. Fetching fresh data.", cacheKey);
        }

        var response = await next(cancellationToken);

        var cacheOptions = CreateCacheOptions(cacheable);

        try
        {
            byte[] responseBytes = SerializeResponse(response);
            await SetCacheAsync(cacheKey, responseBytes, cacheOptions, cancellationToken);

            if (!string.IsNullOrEmpty(cacheable.CacheGroupKey))
            {
                await AddToCacheGroupAsync(cacheable.CacheGroupKey!, cacheKey, cancellationToken);
            }

            _logger.LogDebug("Cached response for key: {CacheKey}", cacheKey);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to write to cache for key: {CacheKey}. Response was not cached.", cacheKey);
        }

        return response;
    }

    /// <summary>
    /// Override to customize the cache key structure.
    /// Default uses {RequestName}_{SerializedRequest}.
    /// </summary>
    protected virtual string GetCacheKey(TRequest request, ICacheableRequest cacheable, string requestName)
    {
        return string.IsNullOrEmpty(cacheable.CacheKeyPrefix)
            ? $"{requestName}_{SerializeRequestForCacheKey(request)}"
            : cacheable.CacheKeyPrefix!;
    }

    /// <summary>
    /// Provides a deterministic string representation of the request for the cache key.
    /// </summary>
    protected virtual string SerializeRequestForCacheKey(TRequest request)
    {
        return request!.GetHashCode().ToString();
    }

    protected virtual CacheEntryOptions CreateCacheOptions(ICacheableRequest cacheable)
    {
        return new CacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                cacheable.CacheDurationMinutes > 0 ? cacheable.CacheDurationMinutes : 30
            ),
        };
    }

    protected abstract ValueTask<byte[]?> GetFromCacheAsync(string key, CancellationToken cancellationToken);

    protected abstract ValueTask SetCacheAsync(
        string key,
        byte[] data,
        CacheEntryOptions options,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Implement to track membership of a cache key in a logical group for batch invalidation.
    /// </summary>
    protected abstract ValueTask AddToCacheGroupAsync(
        string groupKey,
        string cacheKey,
        CancellationToken cancellationToken
    );

    protected abstract byte[] SerializeResponse(TResponse response);

    protected abstract TResponse? DeserializeResponse(byte[]? data);
}
