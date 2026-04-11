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

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenResultTypeMismatch()
    {
        var mediator = new TestDoubles.FakeMediator(sendHandler: _ => "wrong type");
        var adapter = new global::NFramework.Mediator.MediatorAdapter(mediator);

        var action = async () => await adapter.SendAsync<int>(new CreateOrderCommand());

        _ = await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*type mismatch*Expected*received*");
    }

    [Fact]
    public async Task SendAsync_ShouldThrow_WhenResultIsNull()
    {
        var mediator = new TestDoubles.FakeMediator(sendHandler: _ => null);
        var adapter = new global::NFramework.Mediator.MediatorAdapter(mediator);

        var action = async () => await adapter.SendAsync<int>(new CreateOrderCommand());

        _ = await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*type mismatch*received*null*");
    }

    [Fact]
    public async Task StreamAsync_ShouldReturnItems_WhenTypesMatch()
    {
        var items = new[] { "item1", "item2", "item3" };
        var mediator = new TestDoubles.FakeMediator(
            sendHandler: _ => 1,
            streamHandler: _ => items.ToAsyncEnumerable()
        );
        var adapter = new global::NFramework.Mediator.MediatorAdapter(mediator);

        var results = new List<string>();
        await foreach (var item in adapter.StreamAsync<string>(new GetItemsStreamQuery()))
        {
            results.Add(item);
        }

        _ = results.Should().HaveCount(3);
        _ = results.Should().ContainInOrder("item1", "item2", "item3");
    }

    [Fact]
    public async Task StreamAsync_ShouldThrow_WhenItemTypeMismatch()
    {
        var items = new object[] { "wrong", "types" };
        var mediator = new TestDoubles.FakeMediator(
            sendHandler: _ => 1,
            streamHandler: _ => items.ToAsyncEnumerable()
        );
        var adapter = new global::NFramework.Mediator.MediatorAdapter(mediator);

        var action = async () =>
        {
            await foreach (var _ in adapter.StreamAsync<int>(new GetIntStreamQuery()))
            {
            }
        };

        _ = await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*type mismatch*Expected*received*");
    }

    private sealed record CreateOrderCommand : ICommand<int>;

    private sealed record GetOrderQuery : IQuery<string>;

    private sealed record GetItemsStreamQuery : IStreamQuery<string>;

    private sealed record GetIntStreamQuery : IStreamQuery<int>;

    private sealed record StreamItem(string Value);

    private sealed record OrderCreatedEvent : IEvent;
}
