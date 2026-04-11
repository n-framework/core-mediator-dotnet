using NFramework.Mediator.Abstractions.Contracts;
using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator;

/// <summary>
/// Bridges NFramework mediator abstractions to the underlying martinothamar/Mediator implementation.
/// </summary>
public sealed class MediatorAdapter : IMediator
{
    private readonly global::Mediator.IMediator _innerMediator;

    public MediatorAdapter(global::Mediator.IMediator innerMediator)
    {
        _innerMediator = innerMediator;
    }

    public ValueTask<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default
    )
    {
        return sendObjectAsync<TResult>(command, cancellationToken);
    }

    public ValueTask<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        return sendObjectAsync<TResult>(query, cancellationToken);
    }

    public IAsyncEnumerable<TResult> StreamAsync<TResult>(
        IStreamQuery<TResult> query,
        CancellationToken cancellationToken = default
    )
    {
        var sender = (global::Mediator.ISender)_innerMediator;
        return CastStream<TResult>(sender.CreateStream((object)query, cancellationToken), cancellationToken);
    }

    public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var publisher = (global::Mediator.IPublisher)_innerMediator;
        return publisher.Publish((object)@event, cancellationToken);
    }

    private async ValueTask<TResult> sendObjectAsync<TResult>(object request, CancellationToken cancellationToken)
    {
        var sender = (global::Mediator.ISender)_innerMediator;
        object? result = await sender.Send(request, cancellationToken);

        return result is TResult typed
            ? typed
            : throw new InvalidOperationException(
                $"Mediator result type mismatch. Expected '{typeof(TResult).FullName}', received '{result?.GetType().FullName ?? "null"}'."
            );
    }

    private static async IAsyncEnumerable<TResult> CastStream<TResult>(
        IAsyncEnumerable<object?> source,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (object? item in source.WithCancellation(cancellationToken))
        {
            if (item is TResult typed)
            {
                yield return typed;
                continue;
            }

            throw new InvalidOperationException(
                $"Mediator stream item type mismatch. Expected '{typeof(TResult).FullName}', received '{item?.GetType().FullName ?? "null"}'."
            );
        }
    }
}
