using NFramework.Mediator.Abstractions.Contracts.Handlers;
using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Abstractions.Tests.Discovery.Fixtures;

public static class ValidHandlerFixtures
{
    public sealed record CreateOrderCommand(int Quantity) : ICommand<int>;

    public sealed record GetOrderQuery(string OrderId) : IQuery<string>;

    public sealed record GetOrderStreamQuery(string CustomerId) : IStreamQuery<string>;

    public sealed record OrderCreatedEvent(string OrderId) : IEvent;

    public sealed class ValidCommandHandler : ICommandHandler<CreateOrderCommand, int>
    {
        public ValueTask<int> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(command.Quantity);
        }
    }

    public sealed class ValidQueryHandler : IQueryHandler<GetOrderQuery, string>
    {
        public ValueTask<string> Handle(GetOrderQuery query, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(query.OrderId);
        }
    }

    public sealed class ValidStreamHandler : IStreamQueryHandler<GetOrderStreamQuery, string>
    {
        public async IAsyncEnumerable<string> Handle(
            GetOrderStreamQuery query,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            yield return query.CustomerId;
            await Task.CompletedTask;
        }
    }

    public sealed class ValidEventHandlerA : IEventHandler<OrderCreatedEvent>
    {
        public ValueTask Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    public sealed class ValidEventHandlerB : IEventHandler<OrderCreatedEvent>
    {
        public ValueTask Handle(OrderCreatedEvent @event, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    public sealed class DuplicateCommandHandler : ICommandHandler<CreateOrderCommand, int>
    {
        public ValueTask<int> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(command.Quantity * 2);
        }
    }

    public sealed class DuplicateQueryHandler : IQueryHandler<GetOrderQuery, string>
    {
        public ValueTask<string> Handle(GetOrderQuery query, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult($"{query.OrderId}-dup");
        }
    }

    public sealed class DuplicateStreamHandler : IStreamQueryHandler<GetOrderStreamQuery, string>
    {
        public async IAsyncEnumerable<string> Handle(
            GetOrderStreamQuery query,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            yield return $"{query.CustomerId}-dup";
            await Task.CompletedTask;
        }
    }
}
