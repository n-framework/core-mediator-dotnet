using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Performance;
using NFramework.Mediator.MartinothamarMediator.Performance;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class PerformanceBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsMonitoring_WhenRequestDoesNotImplementIIntervalRequest()
    {
        var logger = new LoggerFactory().CreateLogger<PerformanceBehavior<NonMonitoredRequest, string>>();
        var behavior = new PerformanceBehavior<NonMonitoredRequest, string>(logger);
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
        var logger = new LoggerFactory().CreateLogger<PerformanceBehavior<MonitoredRequest, string>>();
        var behavior = new PerformanceBehavior<MonitoredRequest, string>(logger);
        MessageHandlerDelegate<MonitoredRequest, string> next = (_, _) => ValueTask.FromResult("fast-response");

        string result = await behavior.Handle(new MonitoredRequest(), next, default);

        result.ShouldBe("fast-response");
    }

    [Fact]
    public async Task Handle_StillReturnsResponse_WhenHandlerThrows()
    {
        var logger = new LoggerFactory().CreateLogger<PerformanceBehavior<MonitoredRequest, string>>();
        var behavior = new PerformanceBehavior<MonitoredRequest, string>(logger);
        MessageHandlerDelegate<MonitoredRequest, string> next = (_, _) =>
            throw new InvalidOperationException("Handler failed");

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new MonitoredRequest(), next, default).AsTask()
        );
    }

    private sealed record NonMonitoredRequest : IMessage;

    private sealed record MonitoredRequest : IMessage, IIntervalRequest;
}
