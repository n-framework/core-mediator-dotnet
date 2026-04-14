using System.Text.Json;
using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using NFramework.Mediator.Abstractions.Caching;
using NFramework.Mediator.MartinothamarMediator.Caching;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class CacheRemovingBehaviorTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task Handle_SkipsInvalidation_WhenRequestDoesNotImplementICacheRemover()
    {
        var cache = new Mock<IDistributedCache>();
        var logger = new LoggerFactory().CreateLogger<CacheRemovingBehavior<NonRemovingRequest, string>>();
        var behavior = new CacheRemovingBehavior<NonRemovingRequest, string>(cache.Object, logger);
        bool handlerInvoked = false;
        MessageHandlerDelegate<NonRemovingRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new NonRemovingRequest(), next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
        cache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RemovesSingleKey_WhenPatternIsNotAGroup()
    {
        var cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync("product-key", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

        var logger = new LoggerFactory().CreateLogger<CacheRemovingBehavior<CacheRemoverRequest, string>>();
        var behavior = new CacheRemovingBehavior<CacheRemoverRequest, string>(cache.Object, logger);
        MessageHandlerDelegate<CacheRemoverRequest, string> next = (_, _) => ValueTask.FromResult("success");

        _ = await behavior.Handle(new CacheRemoverRequest { CacheKeyPatterns = ["product-key"] }, next, default);

        cache.Verify(c => c.RemoveAsync("product-key", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RemovesAllGroupMembers_WhenPatternIsAGroupKey()
    {
        var groupMembers = new HashSet<string> { "product-1", "product-2", "product-3" };
        byte[] serializedGroup = JsonSerializer.SerializeToUtf8Bytes(groupMembers, JsonOptions);

        var cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync("products-group", It.IsAny<CancellationToken>())).ReturnsAsync(serializedGroup);

        var logger = new LoggerFactory().CreateLogger<CacheRemovingBehavior<CacheRemoverRequest, string>>();
        var behavior = new CacheRemovingBehavior<CacheRemoverRequest, string>(cache.Object, logger);
        MessageHandlerDelegate<CacheRemoverRequest, string> next = (_, _) => ValueTask.FromResult("success");

        _ = await behavior.Handle(new CacheRemoverRequest { CacheKeyPatterns = ["products-group"] }, next, default);

        cache.Verify(c => c.RemoveAsync("product-1", It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(c => c.RemoveAsync("product-2", It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(c => c.RemoveAsync("product-3", It.IsAny<CancellationToken>()), Times.Once);
        // Also removes the group key itself
        cache.Verify(c => c.RemoveAsync("products-group", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidatesOnlyAfterSuccessfulExecution()
    {
        var cache = new Mock<IDistributedCache>();
        var logger = new LoggerFactory().CreateLogger<CacheRemovingBehavior<CacheRemoverRequest, string>>();
        var behavior = new CacheRemovingBehavior<CacheRemoverRequest, string>(cache.Object, logger);
        MessageHandlerDelegate<CacheRemoverRequest, string> next = (_, _) =>
            throw new InvalidOperationException("Handler failed");

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new CacheRemoverRequest { CacheKeyPatterns = ["some-key"] }, next, default).AsTask()
        );

        cache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_HandlesCorruptedGroupData_Gracefully()
    {
        byte[] corruptedData = [0xFF, 0xFF, 0xFF];
        var cache = new Mock<IDistributedCache>();
        _ = cache.Setup(c => c.GetAsync("corrupt-group", It.IsAny<CancellationToken>())).ReturnsAsync(corruptedData);

        var logger = new LoggerFactory().CreateLogger<CacheRemovingBehavior<CacheRemoverRequest, string>>();
        var behavior = new CacheRemovingBehavior<CacheRemoverRequest, string>(cache.Object, logger);
        MessageHandlerDelegate<CacheRemoverRequest, string> next = (_, _) => ValueTask.FromResult("success");

        // Should not throw — corrupted group data is handled gracefully
        string result = await behavior.Handle(
            new CacheRemoverRequest { CacheKeyPatterns = ["corrupt-group"] },
            next,
            default
        );

        result.ShouldBe("success");
        // Still removes the key itself even if group deserialization failed
        cache.Verify(c => c.RemoveAsync("corrupt-group", It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed record NonRemovingRequest : IMessage;

    private sealed record CacheRemoverRequest : IMessage, ICacheRemoverRequest
    {
        public string[] CacheKeyPatterns { get; init; } = [];
    }
}
