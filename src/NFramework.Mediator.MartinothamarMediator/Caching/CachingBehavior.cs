using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Caching;

namespace NFramework.Mediator.MartinothamarMediator.Caching;

/// <summary>
/// Martinothamar Mediator implementation for response caching using <see cref="IDistributedCache"/>.
/// </summary>
public sealed class CachingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger
) : CachingBehaviorBase<TRequest, TResponse>(logger), IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    protected override string GetCacheKey(TRequest request, ICacheableRequest cacheable, string requestName)
    {
        return string.IsNullOrEmpty(cacheable.CacheKeyPrefix)
            ? $"{requestName}_{JsonSerializer.Serialize(request, JsonOptions)}"
            : cacheable.CacheKeyPrefix!;
    }

    protected override async ValueTask<byte[]?> GetFromCacheAsync(string key, CancellationToken cancellationToken)
    {
        return await cache.GetAsync(key, cancellationToken);
    }

    protected override async ValueTask SetCacheAsync(
        string key,
        byte[] data,
        CacheEntryOptions options,
        CancellationToken cancellationToken
    )
    {
        var distributedCacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.SlidingExpiration,
        };
        await cache.SetAsync(key, data, distributedCacheOptions, cancellationToken);
    }

    protected override async ValueTask AddToCacheGroupAsync(
        string groupKey,
        string cacheKey,
        CancellationToken cancellationToken
    )
    {
        byte[]? groupCache = await cache.GetAsync(groupKey, cancellationToken);
        HashSet<string> cacheKeysInGroup = groupCache is not null
            ? JsonSerializer.Deserialize<HashSet<string>>(groupCache, JsonOptions) ?? []
            : [];

        if (cacheKeysInGroup.Add(cacheKey))
        {
            byte[] serializedGroup = JsonSerializer.SerializeToUtf8Bytes(cacheKeysInGroup, JsonOptions);
            await cache.SetAsync(groupKey, serializedGroup, cancellationToken);
        }
    }

    protected override byte[] SerializeResponse(TResponse response)
    {
        return JsonSerializer.SerializeToUtf8Bytes(response, JsonOptions);
    }

    protected override TResponse? DeserializeResponse(byte[]? data)
    {
        if (data == null)
            return default;
        return JsonSerializer.Deserialize<TResponse>(data, JsonOptions);
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
