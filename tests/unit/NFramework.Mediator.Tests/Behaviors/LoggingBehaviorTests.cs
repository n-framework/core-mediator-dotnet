using FluentAssertions;
using NFramework.Mediator.Behaviors;
using NFramework.Mediator.Tests.TestDoubles;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldLogStartAndCompletion_ForSuccessfulRequest()
    {
        var logger = new FakeRequestLogger<TestRequest>();
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(logger, new FakeRequestPipelinePolicyProvider());

        var response = await behavior.Handle(
            new TestRequest(),
            (_, _) => ValueTask.FromResult(new TestResponse(false)),
            default
        );

        _ = response.IsShortCircuited.Should().BeFalse();
        _ = logger.StartCallCount.Should().Be(1);
        _ = logger.CompleteCallCount.Should().Be(1);
        _ = logger.ShortCircuitCallCount.Should().Be(0);
        _ = logger.FailedCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldLogShortCircuit_WhenResponseMarksIt()
    {
        var logger = new FakeRequestLogger<TestRequest>();
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(logger, new FakeRequestPipelinePolicyProvider());

        var response = await behavior.Handle(
            new TestRequest(),
            (_, _) => ValueTask.FromResult(new TestResponse(true)),
            default
        );

        _ = response.IsShortCircuited.Should().BeTrue();
        _ = logger.ShortCircuitCallCount.Should().Be(1);
        _ = logger.CompleteCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldLogFailure_WhenHandlerThrows()
    {
        var logger = new FakeRequestLogger<TestRequest>();
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(logger, new FakeRequestPipelinePolicyProvider());

        var action = async () =>
            await behavior.Handle(
                new TestRequest(),
                (_, _) => ValueTask.FromException<TestResponse>(new InvalidOperationException("boom")),
                default
            );

        _ = await action.Should().ThrowAsync<InvalidOperationException>();
        _ = logger.FailedCallCount.Should().Be(1);
    }

    private sealed record TestRequest : global::Mediator.IMessage;

    private sealed record TestResponse(bool IsShortCircuited) : IShortCircuitResult;
}
