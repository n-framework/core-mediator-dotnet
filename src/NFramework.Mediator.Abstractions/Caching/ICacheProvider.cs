namespace NFramework.Mediator.Abstractions.Caching;

/// <summary>
/// Handles low-level cache interactions (Redis, MemoryCache, etc.) using binary data.
/// </summary>
public interface ICacheProvider
{
    ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken);

    ValueTask SetAsync(string key, byte[] value, CacheEntryOptions? options, CancellationToken cancellationToken);

    ValueTask RemoveAsync(string key, CancellationToken cancellationToken);
}

public sealed class CacheEntryOptions
{
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    public TimeSpan? SlidingExpiration { get; set; }
}
