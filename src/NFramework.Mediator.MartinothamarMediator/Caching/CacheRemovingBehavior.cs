using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Caching;

namespace NFramework.Mediator.MartinothamarMediator.Caching;

/// <summary>
/// Martinothamar Mediator implementation for cache invalidation (single keys or groups).
/// </summary>
public sealed class CacheRemovingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CacheRemovingBehavior<TRequest, TResponse>> logger
) : CacheRemovingBehaviorBase<TRequest, TResponse>(logger), IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    protected override async ValueTask RemoveFromCacheAsync(string pattern, CancellationToken cancellationToken)
    {
        // Check if the pattern is a group key
        byte[]? groupCache = await cache.GetAsync(pattern, cancellationToken);
        if (groupCache is not null)
        {
            try
            {
                var keys = JsonSerializer.Deserialize<HashSet<string>>(groupCache, JsonOptions);
                if (keys is not null)
                {
                    foreach (string key in keys)
                    {
                        await cache.RemoveAsync(key, cancellationToken);
                        logger.LogDebug("Removed key {CacheKey} from group {GroupKey}", key, pattern);
                    }
                }
            }
            catch (JsonException)
            {
                // Not a group key, just a regular key or corrupted data
                logger.LogTrace("Pattern {Pattern} is not a valid group key JSON. Removing as single key.", pattern);
            }
        }

        // Always attempt to remove the key itself (either single key or the group entry)
        await cache.RemoveAsync(pattern, cancellationToken);
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return await HandleAsync(request, ct => next(request, ct), cancellationToken);
    }
}
