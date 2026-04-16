namespace NFramework.Mediator.Abstractions.Caching;

/// <summary>
/// Handles low-level cache interactions (Redis, MemoryCache, etc.) using binary data.
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Retrieves a value from the cache.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the cached value or null if not found.</returns>
    ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask SetAsync(string key, byte[] value, CacheEntryOptions? options, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask RemoveAsync(string key, CancellationToken cancellationToken);
}

public sealed class CacheEntryOptions
{
    public CacheEntryOptions(TimeSpan? absoluteExpirationRelativeToNow = null, TimeSpan? slidingExpiration = null)
    {
        if (absoluteExpirationRelativeToNow == null && slidingExpiration == null)
        {
            throw new ArgumentException("At least one expiration strategy (Absolute or Sliding) must be specified.");
        }

        if (absoluteExpirationRelativeToNow is { TotalMilliseconds: <= 0 })
        {
            throw new ArgumentOutOfRangeException(
                nameof(absoluteExpirationRelativeToNow),
                "Absolute expiration must be a positive TimeSpan."
            );
        }

        if (slidingExpiration is { TotalMilliseconds: <= 0 })
        {
            throw new ArgumentOutOfRangeException(
                nameof(slidingExpiration),
                "Sliding expiration must be a positive TimeSpan."
            );
        }

        AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
        SlidingExpiration = slidingExpiration;
    }

    public TimeSpan? AbsoluteExpirationRelativeToNow { get; }

    public TimeSpan? SlidingExpiration { get; }
}
