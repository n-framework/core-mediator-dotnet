using NFramework.Mediator.Abstractions.Contracts;
using NFramework.Mediator.Abstractions.Contracts.Handlers;
using NFramework.Mediator.Abstractions.Contracts.Pipeline;
using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Abstractions.Tests.ConsumerSmoke;

internal static class AbstractionsOnlyConsumer
{
    public sealed record CreateCustomerCommand(string Name) : ICommand<Guid>;

    public sealed record GetCustomerQuery(Guid Id) : IQuery<string>;

    public sealed record ListCustomerNamesQuery() : IStreamQuery<string>;

    public sealed record CustomerCreated(Guid CustomerId) : IEvent;

    public sealed class CreateCustomerHandler : ICommandHandler<CreateCustomerCommand, Guid>
    {
        public ValueTask<Guid> Handle(CreateCustomerCommand command, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(Guid.NewGuid());
        }
    }

    public sealed class GetCustomerHandler : IQueryHandler<GetCustomerQuery, string>
    {
        public ValueTask<string> Handle(GetCustomerQuery query, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(query.Id.ToString("N"));
        }
    }

    public sealed class ListCustomersHandler : IStreamQueryHandler<ListCustomerNamesQuery, string>
    {
        public async IAsyncEnumerable<string> Handle(
            ListCustomerNamesQuery query,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            yield return "customer";
            await Task.CompletedTask;
        }
    }

    public sealed class CustomerCreatedHandler : IEventHandler<CustomerCreated>
    {
        public ValueTask Handle(CustomerCreated @event, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    public sealed class ExampleBehavior : IPipelineBehavior<CreateCustomerCommand, Guid>
    {
        public ValueTask<Guid> Handle(
            CreateCustomerCommand request,
            RequestHandlerDelegate<Guid> next,
            CancellationToken cancellationToken
        )
        {
            return next(cancellationToken);
        }
    }

    public static async Task ExerciseMediatorAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        var customerId = await mediator.SendAsync(new CreateCustomerCommand("name"), cancellationToken);
        _ = await mediator.SendAsync(new GetCustomerQuery(customerId), cancellationToken);

        await foreach (string? _ in mediator.StreamAsync(new ListCustomerNamesQuery(), cancellationToken)) { }

        await mediator.PublishAsync(new CustomerCreated(customerId), cancellationToken);
    }
}
