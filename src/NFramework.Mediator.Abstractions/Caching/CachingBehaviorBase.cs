using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Caching;

/// <summary>
/// Standardizes the "get-if-cached-else-fetch-and-store" logic.
/// Errors during cache operations are logged but do not disrupt the request lifecycle.
/// </summary>
public abstract class CachingBehaviorBase<TRequest, TResponse>(ILogger<CachingBehaviorBase<TRequest, TResponse>> logger)
{
    private readonly ILogger<CachingBehaviorBase<TRequest, TResponse>> _logger = logger;

    private static readonly Action<ILogger, string, Exception?> LogFetchedFromCacheAction =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(1, nameof(HandleAsync)),
            "Fetched from cache: {CacheKey}"
        );

    private static readonly Action<ILogger, string, Exception?> LogCacheNullWarningAction =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(2, nameof(HandleAsync)),
            "Cache contained null data for key: {CacheKey}. Fetching fresh data."
        );

    private static readonly Action<ILogger, string, string, string, Exception?> LogCacheReadErrorAction =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(3, nameof(HandleAsync)),
            "Failed to read from cache (GET) for request {RequestName}. Key: {CacheKey}. Error: {Message}"
        );

    private static readonly Action<ILogger, string, Exception?> LogCachedResponseAction = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(4, nameof(HandleAsync)),
        "Cached response for key: {CacheKey}"
    );

    private static readonly Action<ILogger, string, string, string, Exception?> LogCacheWriteErrorAction =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Error,
            new EventId(5, nameof(HandleAsync)),
            "Failed to write to cache (SET/GROUP) for request {RequestName}. Key: {CacheKey}. Error: {Message}"
        );

    /// <returns>The response from the next handler in the pipeline.</returns>
    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not ICacheableRequest cacheable)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        string requestName = typeof(TRequest).Name;
        string cacheKey = GetCacheKey(request, cacheable, requestName);

        try
        {
            byte[]? cachedResponse = await GetFromCacheAsync(cacheKey, cancellationToken).ConfigureAwait(false);
            if (cachedResponse != null)
            {
                var deserialized = DeserializeResponse(cachedResponse);
                if (deserialized is not null)
                {
                    LogFetchedFromCacheAction(_logger, cacheKey, null);
                    return deserialized;
                }
                else
                {
                    LogCacheNullWarningAction(_logger, cacheKey, null);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogCacheReadErrorAction(_logger, requestName, cacheKey, ex.Message, ex);
        }

        var response = await next(cancellationToken).ConfigureAwait(false);

        var cacheOptions = CreateCacheOptions(cacheable);

        try
        {
            byte[] responseBytes = SerializeResponse(response);
            await SetCacheAsync(cacheKey, responseBytes, cacheOptions, cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(cacheable.CacheGroupKey))
            {
                await AddToCacheGroupAsync(cacheable.CacheGroupKey, cacheKey, cancellationToken).ConfigureAwait(false);
            }

            LogCachedResponseAction(_logger, cacheKey, null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogCacheWriteErrorAction(_logger, requestName, cacheKey, ex.Message, ex);
        }

        return response;
    }

    /// <returns>A string representing the cache key.</returns>
    protected virtual string GetCacheKey(TRequest request, ICacheableRequest cacheable, string requestName)
    {
        ArgumentNullException.ThrowIfNull(cacheable);

        return string.IsNullOrEmpty(cacheable.CacheKeyPrefix)
            ? $"{requestName}_{SerializeRequestForCacheKey(request)}"
            : cacheable.CacheKeyPrefix;
    }

    /// <summary>
    /// Provides a deterministic string representation of the request for the cache key.
    /// Implementations must ensure determinism across process restarts and machines.
    /// </summary>
    /// <returns>A serialized string representation of the request.</returns>
    protected abstract string SerializeRequestForCacheKey(TRequest request);

    /// <returns>A new <see cref="CacheEntryOptions"/> instance.</returns>
    protected virtual CacheEntryOptions CreateCacheOptions(ICacheableRequest cacheable)
    {
        ArgumentNullException.ThrowIfNull(cacheable);

        double absoluteMinutes = cacheable.CacheDurationMinutes > 0 ? cacheable.CacheDurationMinutes : 30;

        TimeSpan? slidingExpiration =
            cacheable.SlidingExpirationMinutes.HasValue && cacheable.SlidingExpirationMinutes.Value > 0
                ? TimeSpan.FromMinutes(cacheable.SlidingExpirationMinutes.Value)
                : null;

        return new CacheEntryOptions(
            absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(absoluteMinutes),
            slidingExpiration: slidingExpiration
        );
    }

    /// <summary>
    /// Implement to provide the cache retrieval logic.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the cached value or null if not found.</returns>
    protected abstract ValueTask<byte[]?> GetFromCacheAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Implement to provide the cache storage logic.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract ValueTask SetCacheAsync(
        string key,
        byte[] data,
        CacheEntryOptions options,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Implement to track membership of a cache key in a logical group for batch invalidation.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected abstract ValueTask AddToCacheGroupAsync(
        string groupKey,
        string cacheKey,
        CancellationToken cancellationToken
    );

    /// <returns>The serialized response in binary format.</returns>
    protected abstract byte[] SerializeResponse(TResponse response);

    /// <returns>The deserialized response of type <typeparamref name="TResponse"/>, or null if deserialization fails.</returns>
    protected abstract TResponse? DeserializeResponse(byte[]? data);
}
