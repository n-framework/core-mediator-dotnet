namespace NFramework.Mediator.Abstractions.Behaviors;

/// <summary>
/// Marker interface for the cache invalidation behavior.
/// Cache will be invalidated after requests implementing this interface are successfully executed.
/// </summary>
public interface ICacheRemoverRequest
{
    /// <summary>
    /// Cache key patterns to be invalidated.
    /// </summary>
    string[] CacheKeyPatterns { get; }
}
