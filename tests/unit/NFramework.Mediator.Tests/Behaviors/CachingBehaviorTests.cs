using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NFramework.Mediator.Abstractions.Caching;
using NFramework.Mediator.MartinothamarMediator.Caching;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class CachingBehaviorTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task Handle_SkipsCache_WhenRequestDoesNotImplementICacheable()
    {
        var cache = new Mock<IDistributedCache>();
        var logger = new LoggerFactory().CreateLogger<CachingBehavior<NonCacheableRequest, string>>();
        var behavior = new CachingBehavior<NonCacheableRequest, string>(cache.Object, logger);
        bool handlerInvoked = false;
        MessageHandlerDelegate<NonCacheableRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new NonCacheableRequest(), next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
        cache.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsCachedValue_WhenCacheHit()
    {
        byte[] cachedValue = JsonSerializer.SerializeToUtf8Bytes("cached-result", JsonOptions);
        var cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync("test-key", It.IsAny<CancellationToken>())).ReturnsAsync(cachedValue);

        var logger = new LoggerFactory().CreateLogger<CachingBehavior<CacheableRequest, string>>();
        var behavior = new CachingBehavior<CacheableRequest, string>(cache.Object, logger);
        bool handlerInvoked = false;
        MessageHandlerDelegate<CacheableRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("fresh-result");
        };

        string result = await behavior.Handle(new CacheableRequest { CacheKeyPrefix = "test-key" }, next, default);

        result.ShouldBe("cached-result");
        handlerInvoked.ShouldBeFalse("Handler should not be called on cache hit");
    }

    [Fact]
    public async Task Handle_CachesResponse_WhenCacheMiss()
    {
        var cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        var logger = new LoggerFactory().CreateLogger<CachingBehavior<CacheableRequest, string>>();
        var behavior = new CachingBehavior<CacheableRequest, string>(cache.Object, logger);
        MessageHandlerDelegate<CacheableRequest, string> next = (_, _) => ValueTask.FromResult("fresh-result");

        string result = await behavior.Handle(new CacheableRequest { CacheKeyPrefix = "test-key" }, next, default);

        result.ShouldBe("fresh-result");
        cache.Verify(
            c =>
                c.SetAsync(
                    "test-key",
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_FetchesFreshData_WhenCacheContainsCorruptedBytes()
    {
        byte[] corruptedData = [0xFF, 0xFF, 0xFF];
        var cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync("test-key", It.IsAny<CancellationToken>())).ReturnsAsync(corruptedData);

        var logger = new LoggerFactory().CreateLogger<CachingBehavior<CacheableRequest, string>>();
        var behavior = new CachingBehavior<CacheableRequest, string>(cache.Object, logger);
        MessageHandlerDelegate<CacheableRequest, string> next = (_, _) => ValueTask.FromResult("fresh-result");

        string result = await behavior.Handle(new CacheableRequest { CacheKeyPrefix = "test-key" }, next, default);

        result.ShouldBe("fresh-result");
    }

    [Fact]
    public async Task Handle_AddsToCacheGroup_WhenCacheGroupKeyIsSet()
    {
        var cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync("test-key", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        _ = cache.Setup(c => c.GetAsync("products-group", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        var logger = new LoggerFactory().CreateLogger<CachingBehavior<CacheableWithGroupRequest, string>>();
        var behavior = new CachingBehavior<CacheableWithGroupRequest, string>(cache.Object, logger);
        MessageHandlerDelegate<CacheableWithGroupRequest, string> next = (_, _) => ValueTask.FromResult("fresh-result");

        _ = await behavior.Handle(
            new CacheableWithGroupRequest { CacheKeyPrefix = "test-key", CacheGroupKey = "products-group" },
            next,
            default
        );

        // Verifies that the group key is stored in the cache
        cache.Verify(
            c =>
                c.SetAsync(
                    "products-group",
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    private sealed record NonCacheableRequest : IMessage;

    private sealed record CacheableRequest : IMessage, ICacheableRequest
    {
        public string? CacheKeyPrefix { get; init; } = string.Empty;
        public int CacheDurationMinutes { get; init; } = 30;
        public string? CacheGroupKey { get; init; }
    }

    private sealed record CacheableWithGroupRequest : IMessage, ICacheableRequest
    {
        public string? CacheKeyPrefix { get; init; } = string.Empty;
        public int CacheDurationMinutes { get; init; } = 30;
        public string? CacheGroupKey { get; init; }
    }
}
