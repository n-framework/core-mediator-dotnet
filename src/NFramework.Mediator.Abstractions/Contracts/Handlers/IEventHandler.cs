using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Abstractions.Contracts.Handlers;

public interface IEventHandler<TEvent>
    where TEvent : IEvent
{
    ValueTask Handle(TEvent @event, CancellationToken cancellationToken);
}
