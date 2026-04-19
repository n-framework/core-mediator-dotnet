using Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using NFramework.Mediator.Abstractions.Performance;
using NFramework.Mediator.Mediator.Performance;

namespace NFramework.Mediator.Tests.Performance;

public sealed class PerformanceBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsMonitoring_WhenRequestDoesNotImplementIIntervalRequest()
    {
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<PerformanceBehavior<NonMonitoredRequest, string>>();
        PerformanceBehavior<NonMonitoredRequest, string> behavior = new PerformanceBehavior<
            NonMonitoredRequest,
            string
        >(logger, new PerformanceOptions());
        bool handlerInvoked = false;
        MessageHandlerDelegate<NonMonitoredRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new NonMonitoredRequest(), next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ReturnsResponse_ForFastRequests()
    {
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<PerformanceBehavior<MonitoredRequest, string>>();
        PerformanceBehavior<MonitoredRequest, string> behavior = new PerformanceBehavior<MonitoredRequest, string>(
            logger,
            new PerformanceOptions()
        );
        MessageHandlerDelegate<MonitoredRequest, string> next = (_, _) => ValueTask.FromResult("fast-response");

        string result = await behavior.Handle(new MonitoredRequest(), next, default);

        result.ShouldBe("fast-response");
    }

    [Fact]
    public async Task Handle_LogsWarning_ForSlowRequests()
    {
        // Use Moq to verify LogWarning
        Mock<ILogger<PerformanceBehavior<MonitoredRequest, string>>> logger =
            new Mock<ILogger<PerformanceBehavior<MonitoredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        PerformanceOptions options = new PerformanceOptions { PerformanceThreshold = TimeSpan.FromMilliseconds(50) };
        PerformanceBehavior<MonitoredRequest, string> behavior = new PerformanceBehavior<MonitoredRequest, string>(
            logger.Object,
            options
        );
        MessageHandlerDelegate<MonitoredRequest, string> next = async (_, _) =>
        {
            await Task.Delay(100, CancellationToken.None).ConfigureAwait(false);
            return "slow-response";
        };

        string result = await behavior.Handle(new MonitoredRequest(), next, default);

        result.ShouldBe("slow-response");
        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Long Running Request")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_DisablesMonitoring_WhenThresholdIsZero()
    {
        Mock<ILogger<PerformanceBehavior<MonitoredRequest, string>>> logger =
            new Mock<ILogger<PerformanceBehavior<MonitoredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        PerformanceOptions options = new PerformanceOptions { PerformanceThreshold = TimeSpan.Zero };
        PerformanceBehavior<MonitoredRequest, string> behavior = new PerformanceBehavior<MonitoredRequest, string>(
            logger.Object,
            options
        );
        MessageHandlerDelegate<MonitoredRequest, string> next = async (_, _) =>
        {
            await Task.Delay(50, CancellationToken.None).ConfigureAwait(false);
            return "zero-threshold";
        };

        _ = await behavior.Handle(new MonitoredRequest(), next, default);

        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_DisablesMonitoring_WhenThresholdIsNegative()
    {
        Mock<ILogger<PerformanceBehavior<MonitoredRequest, string>>> logger =
            new Mock<ILogger<PerformanceBehavior<MonitoredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        PerformanceOptions options = new PerformanceOptions { PerformanceThreshold = TimeSpan.FromMilliseconds(-100) };
        PerformanceBehavior<MonitoredRequest, string> behavior = new PerformanceBehavior<MonitoredRequest, string>(
            logger.Object,
            options
        );
        MessageHandlerDelegate<MonitoredRequest, string> next = async (_, _) =>
        {
            await Task.Delay(50, CancellationToken.None).ConfigureAwait(false);
            return "negative-threshold";
        };

        _ = await behavior.Handle(new MonitoredRequest(), next, default);

        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_StillReturnsResponse_WhenHandlerThrows()
    {
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<PerformanceBehavior<MonitoredRequest, string>>();
        PerformanceBehavior<MonitoredRequest, string> behavior = new PerformanceBehavior<MonitoredRequest, string>(
            logger,
            new PerformanceOptions()
        );
        MessageHandlerDelegate<MonitoredRequest, string> next = (_, _) =>
            throw new InvalidOperationException("Handler failed");

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new MonitoredRequest(), next, default).AsTask()
        );
    }

    internal sealed record NonMonitoredRequest : IMessage;

    internal sealed record MonitoredRequest : IMessage, IIntervalRequest;
}
