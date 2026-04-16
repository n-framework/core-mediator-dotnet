namespace NFramework.Mediator.Abstractions.Caching;

/// <summary>
/// Only responses of requests implementing this interface will be cached.
/// </summary>
public interface ICacheableRequest
{
    /// <summary>
    /// If empty, the request type name will be used as the prefix.
    /// </summary>
    string? CacheKeyPrefix { get; }

    int CacheDurationMinutes { get; }

    int? SlidingExpirationMinutes { get; }

    /// <summary>
    /// Enables batch invalidation by linking this entry to a logical group.
    /// </summary>
    string? CacheGroupKey { get; }
}
