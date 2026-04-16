using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Caching;

namespace NFramework.Mediator.Mediator.Caching;

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
        ArgumentNullException.ThrowIfNull(cacheable);
        return string.IsNullOrEmpty(cacheable.CacheKeyPrefix)
            ? $"{requestName}_{SerializeRequestForCacheKey(request)}"
            : cacheable.CacheKeyPrefix!;
    }

    protected override string SerializeRequestForCacheKey(TRequest request)
    {
        return JsonSerializer.Serialize(request, JsonOptions);
    }

    protected override async ValueTask<byte[]?> GetFromCacheAsync(string key, CancellationToken cancellationToken)
    {
        return await cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
    }

    protected override async ValueTask SetCacheAsync(
        string key,
        byte[] data,
        CacheEntryOptions options,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        DistributedCacheEntryOptions distributedCacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = options.SlidingExpiration,
        };
        await cache.SetAsync(key, data, distributedCacheOptions, cancellationToken).ConfigureAwait(false);
    }

    protected override async ValueTask AddToCacheGroupAsync(
        string groupKey,
        string cacheKey,
        CancellationToken cancellationToken
    )
    {
        byte[]? groupCache = await cache.GetAsync(groupKey, cancellationToken).ConfigureAwait(false);
        var cacheKeysInGroup = groupCache is not null
            ? JsonSerializer.Deserialize<HashSet<string>>(groupCache, JsonOptions) ?? []
            : [];

        if (cacheKeysInGroup.Add(cacheKey))
        {
            byte[] serializedGroup = JsonSerializer.SerializeToUtf8Bytes(cacheKeysInGroup, JsonOptions);
            await cache.SetAsync(groupKey, serializedGroup, cancellationToken).ConfigureAwait(false);
        }
    }

    protected override byte[] SerializeResponse(TResponse response)
    {
        return JsonSerializer.SerializeToUtf8Bytes(response, JsonOptions);
    }

    protected override TResponse? DeserializeResponse(byte[]? data)
    {
        return data == null ? default : JsonSerializer.Deserialize<TResponse>(data, JsonOptions);
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return await HandleAsync(request, ct => next(request, ct), cancellationToken).ConfigureAwait(false);
    }
}
