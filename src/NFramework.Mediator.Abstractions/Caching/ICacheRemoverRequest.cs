namespace NFramework.Mediator.Abstractions.Caching;

/// <summary>
/// Cache will be invalidated after requests implementing this interface are successfully executed.
/// </summary>
public interface ICacheRemoverRequest
{
    string[] CacheKeyPatterns { get; }
}
