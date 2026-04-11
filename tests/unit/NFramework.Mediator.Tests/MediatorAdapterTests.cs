using FluentAssertions;
using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Tests;

public sealed class MediatorAdapterTests
{
    [Fact]
    public async Task SendAsync_ForCommand_ShouldDispatchThroughMediator()
    {
        var mediator = new TestDoubles.FakeMediator(sendHandler: request =>
            request is CreateOrderCommand ? 42 : throw new InvalidOperationException("Unexpected request")
        );
        var adapter = new global::NFramework.Mediator.MediatorAdapter(mediator);

        int result = await adapter.SendAsync(new CreateOrderCommand());

        _ = result.Should().Be(42);
    }

    [Fact]
    public async Task SendAsync_ForQuery_ShouldDispatchThroughMediator()
    {
        var mediator = new TestDoubles.FakeMediator(sendHandler: request =>
            request is GetOrderQuery ? "order-1" : throw new InvalidOperationException("Unexpected request")
        );
        var adapter = new global::NFramework.Mediator.MediatorAdapter(mediator);

        string result = await adapter.SendAsync(new GetOrderQuery());

        _ = result.Should().Be("order-1");
    }

    [Fact]
    public async Task PublishAsync_ShouldForwardEvent()
    {
        var mediator = new TestDoubles.FakeMediator(sendHandler: _ => 1);
        var adapter = new global::NFramework.Mediator.MediatorAdapter(mediator);

        await adapter.PublishAsync(new OrderCreatedEvent());

        _ = mediator.PublishedEvents.Should().ContainSingle().Which.Should().BeOfType<OrderCreatedEvent>();
    }

    private sealed record CreateOrderCommand : ICommand<int>;

    private sealed record GetOrderQuery : IQuery<string>;

    private sealed record OrderCreatedEvent : IEvent;
}
