using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NFramework.Mediator.Abstractions.Caching;
using NFramework.Mediator.Mediator.Caching;

namespace NFramework.Mediator.Tests.Caching;

public sealed class CachingBehaviorTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task Handle_SkipsCache_WhenRequestDoesNotImplementICacheable()
    {
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<CachingBehavior<NonCacheableRequest, string>>();
        CachingBehavior<NonCacheableRequest, string> behavior = new CachingBehavior<NonCacheableRequest, string>(
            cache.Object,
            logger
        );
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
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync("test-key", It.IsAny<CancellationToken>())).ReturnsAsync(cachedValue);

        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<CachingBehavior<CacheableRequest, string>>();
        CachingBehavior<CacheableRequest, string> behavior = new CachingBehavior<CacheableRequest, string>(
            cache.Object,
            logger
        );
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
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<CachingBehavior<CacheableRequest, string>>();
        CachingBehavior<CacheableRequest, string> behavior = new CachingBehavior<CacheableRequest, string>(
            cache.Object,
            logger
        );
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
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync("test-key", It.IsAny<CancellationToken>())).ReturnsAsync(corruptedData);

        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<CachingBehavior<CacheableRequest, string>>();
        CachingBehavior<CacheableRequest, string> behavior = new CachingBehavior<CacheableRequest, string>(
            cache.Object,
            logger
        );
        MessageHandlerDelegate<CacheableRequest, string> next = (_, _) => ValueTask.FromResult("fresh-result");

        string result = await behavior.Handle(new CacheableRequest { CacheKeyPrefix = "test-key" }, next, default);

        result.ShouldBe("fresh-result");
    }

    [Fact]
    public async Task Handle_AddsToCacheGroup_WhenCacheGroupKeyIsSet()
    {
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync("test-key", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        _ = cache.Setup(c => c.GetAsync("products-group", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<CachingBehavior<CacheableWithGroupRequest, string>>();
        CachingBehavior<CacheableWithGroupRequest, string> behavior = new CachingBehavior<
            CacheableWithGroupRequest,
            string
        >(cache.Object, logger);
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

    [Fact]
    public async Task Handle_StillInvokesHandler_WhenCacheGetThrows()
    {
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        _ = cache
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache unavailable"));

        Mock<ILogger<CachingBehavior<CacheableRequest, string>>> logger =
            new Mock<ILogger<CachingBehavior<CacheableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        CachingBehavior<CacheableRequest, string> behavior = new CachingBehavior<CacheableRequest, string>(
            cache.Object,
            logger.Object
        );
        MessageHandlerDelegate<CacheableRequest, string> next = (_, _) => ValueTask.FromResult("fresh-result");

        string result = await behavior.Handle(new CacheableRequest { CacheKeyPrefix = "test-key" }, next, default);

        result.ShouldBe("fresh-result");
        // Should log error but not fail
        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task Handle_StillReturnsResponse_WhenCacheSetThrows()
    {
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);
        _ = cache
            .Setup(c =>
                c.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception("Cache set failed"));

        Mock<ILogger<CachingBehavior<CacheableRequest, string>>> logger =
            new Mock<ILogger<CachingBehavior<CacheableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        CachingBehavior<CacheableRequest, string> behavior = new CachingBehavior<CacheableRequest, string>(
            cache.Object,
            logger.Object
        );
        MessageHandlerDelegate<CacheableRequest, string> next = (_, _) => ValueTask.FromResult("fresh-result");

        string result = await behavior.Handle(new CacheableRequest { CacheKeyPrefix = "test-key" }, next, default);

        result.ShouldBe("fresh-result");
        // Should log error but return the freshly fetched result
        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task Handle_RespectsCancellationToken()
    {
        using var ctSource = new CancellationTokenSource();
        var ct = ctSource.Token;

        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), ct)).ReturnsAsync((byte[]?)null).Verifiable();
        cache
            .Setup(c =>
                c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), ct)
            )
            .Returns(Task.CompletedTask)
            .Verifiable();

        Mock<ILogger<CachingBehavior<CacheableRequest, string>>> logger =
            new Mock<ILogger<CachingBehavior<CacheableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        CachingBehavior<CacheableRequest, string> behavior = new CachingBehavior<CacheableRequest, string>(
            cache.Object,
            logger.Object
        );
        MessageHandlerDelegate<CacheableRequest, string> next = (_, t) =>
        {
            t.ShouldBe(ct);
            return ValueTask.FromResult("fresh-result");
        };

        _ = await behavior.Handle(new CacheableRequest { CacheKeyPrefix = "test-key" }, next, ct);

        // Verify that the token was passed down explicitly to cache provider
        cache.Verify();
    }

    [Theory]
    [InlineData(0, 30)] // Default fallback
    [InlineData(-1, 30)] // Negative fallback
    [InlineData(100, 100)] // Valid normal
    public async Task Handle_HandlesDurationEdgeCases(int requestedMinutes, double expectedMinutes)
    {
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<CachingBehavior<CacheableRequest, string>>();
        CachingBehavior<CacheableRequest, string> behavior = new CachingBehavior<CacheableRequest, string>(
            cache.Object,
            logger
        );
        MessageHandlerDelegate<CacheableRequest, string> next = (_, _) => ValueTask.FromResult("fresh-result");

        _ = await behavior.Handle(
            new CacheableRequest { CacheKeyPrefix = "test-key", CacheDurationMinutes = requestedMinutes },
            next,
            default
        );

        cache.Verify(
            c =>
                c.SetAsync(
                    "test-key",
                    It.IsAny<byte[]>(),
                    It.Is<DistributedCacheEntryOptions>(opts =>
                        opts.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(expectedMinutes)
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_RespectsSlidingExpiration_WhenConfigured()
    {
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<CachingBehavior<CacheableRequest, string>>();
        CachingBehavior<CacheableRequest, string> behavior = new CachingBehavior<CacheableRequest, string>(
            cache.Object,
            logger
        );
        MessageHandlerDelegate<CacheableRequest, string> next = (_, _) => ValueTask.FromResult("fresh-result");

        _ = await behavior.Handle(
            new CacheableRequest { CacheKeyPrefix = "test-key", SlidingExpirationMinutes = 15 },
            next,
            default
        );

        cache.Verify(
            c =>
                c.SetAsync(
                    "test-key",
                    It.IsAny<byte[]>(),
                    It.Is<DistributedCacheEntryOptions>(opts => opts.SlidingExpiration == TimeSpan.FromMinutes(15)),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_SetsSlidingExpiration_WhenStrategyIsSliding()
    {
        Mock<IDistributedCache> cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<CachingBehavior<CacheableWithSlidingRequest, string>>();
        CachingBehavior<CacheableWithSlidingRequest, string> behavior = new CachingBehavior<
            CacheableWithSlidingRequest,
            string
        >(cache.Object, logger);
        MessageHandlerDelegate<CacheableWithSlidingRequest, string> next = (_, _) =>
            ValueTask.FromResult("fresh-result");

        _ = await behavior.Handle(new CacheableWithSlidingRequest { CacheKeyPrefix = "test-key" }, next, default);

        cache.Verify(
            c =>
                c.SetAsync(
                    "test-key",
                    It.IsAny<byte[]>(),
                    It.Is<DistributedCacheEntryOptions>(o => o.SlidingExpiration != null),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    internal sealed record NonCacheableRequest : IMessage;

    internal sealed record CacheableRequest : IMessage, ICacheableRequest
    {
        public string? CacheKeyPrefix { get; init; } = string.Empty;
        public int CacheDurationMinutes { get; init; } = 30;
        public int? SlidingExpirationMinutes { get; init; }
        public string? CacheGroupKey { get; init; }
    }

    internal sealed record CacheableWithGroupRequest : IMessage, ICacheableRequest
    {
        public string? CacheKeyPrefix { get; init; } = string.Empty;
        public int CacheDurationMinutes { get; init; } = 30;
        public int? SlidingExpirationMinutes { get; init; }
        public string? CacheGroupKey { get; init; }
    }

    internal sealed record CacheableWithSlidingRequest : IMessage, ICacheableRequest
    {
        public string? CacheKeyPrefix { get; init; } = "sliding";
        public int CacheDurationMinutes { get; init; } = 30;
        public int? SlidingExpirationMinutes { get; init; } = 5;
        public string? CacheGroupKey { get; init; }
    }
}
