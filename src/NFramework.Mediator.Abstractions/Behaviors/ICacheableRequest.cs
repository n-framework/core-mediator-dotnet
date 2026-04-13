namespace NFramework.Mediator.Abstractions.Behaviors;

/// <summary>
/// Marker interface for the response caching behavior.
/// Only responses of requests implementing this interface will be cached.
/// </summary>
public interface ICacheableRequest
{
    /// <summary>
    /// Cache key prefix (if empty, the type name is used).
    /// </summary>
    string? CacheKeyPrefix { get; }

    /// <summary>
    /// Cache duration in minutes.
    /// </summary>
    int CacheDurationMinutes { get; }
}
