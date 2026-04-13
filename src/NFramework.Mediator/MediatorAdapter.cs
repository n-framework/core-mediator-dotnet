using NFramework.Mediator.Abstractions.Contracts;
using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator;

/// <summary>
/// Bridges NFramework mediator abstractions to the underlying martinothamar/Mediator implementation.
/// Uses generic dispatch to forward calls directly to the source-generated monomorphized handlers.
/// </summary>
public sealed class MediatorAdapter : IMediator
{
    private readonly global::Mediator.ISender _sender;
    private readonly global::Mediator.IPublisher _publisher;

    /// <summary>
    /// Creates a new mediator adapter.
    /// </summary>
    /// <param name="innerMediator">The underlying Mediator instance.</param>
    public MediatorAdapter(global::Mediator.IMediator innerMediator)
    {
        _sender = innerMediator;
        _publisher = innerMediator;
    }

    /// <inheritdoc />
    public ValueTask<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default
    )
    {
        return _sender.Send<TResult>((global::Mediator.ICommand<TResult>)command, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        return _sender.Send<TResult>((global::Mediator.IQuery<TResult>)query, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResult> StreamAsync<TResult>(
        IStreamQuery<TResult> query,
        CancellationToken cancellationToken = default
    )
    {
        return _sender.CreateStream<TResult>((global::Mediator.IStreamQuery<TResult>)query, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        return _publisher.Publish((global::Mediator.INotification)(object)@event, cancellationToken);
    }
}
