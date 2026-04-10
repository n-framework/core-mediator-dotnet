using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Abstractions.Contracts.Handlers;

/// <summary>
/// Handler contract for processing domain events.
/// Multiple handlers can be registered for the same event (fan-out pattern).
/// Expected to be registered as transient in DI container.
/// </summary>
/// <typeparam name="TEvent">The event type being handled.</typeparam>
public interface IEventHandler<TEvent>
    where TEvent : IEvent
{
    ValueTask Handle(TEvent @event, CancellationToken cancellationToken);
}
