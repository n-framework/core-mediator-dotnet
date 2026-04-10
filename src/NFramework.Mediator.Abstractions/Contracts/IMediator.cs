using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Abstractions.Contracts;

public interface IMediator
{
    ValueTask<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    ValueTask<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

    IAsyncEnumerable<TResult> StreamAsync<TResult>(
        IStreamQuery<TResult> query,
        CancellationToken cancellationToken = default
    );

    ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}
