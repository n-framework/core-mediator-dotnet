using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Behaviors;

namespace NFramework.Mediator.MartinothamarMediator.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not ICacheableRequest cacheable)
        {
            return await next(request, cancellationToken);
        }

        var requestName = typeof(TRequest).Name;
        var cacheKey = string.IsNullOrEmpty(cacheable.CacheKeyPrefix)
            ? $"{requestName}_{JsonSerializer.Serialize(request)}"
            : $"{cacheable.CacheKeyPrefix}";

        var cachedResponse = await cache.GetAsync(cacheKey, cancellationToken);
        if (cachedResponse != null)
        {
            logger.LogDebug("Fetched from cache: {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;
        }

        var response = await next(request, cancellationToken);

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                cacheable.CacheDurationMinutes > 0 ? cacheable.CacheDurationMinutes : 30
            ),
        };

        var responseBytes = JsonSerializer.SerializeToUtf8Bytes(response);
        await cache.SetAsync(cacheKey, responseBytes, cacheOptions, cancellationToken);

        return response;
    }
}
