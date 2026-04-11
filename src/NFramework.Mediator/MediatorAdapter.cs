using NFramework.Mediator.Abstractions.Contracts;
using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator;

/// <summary>
/// Bridges NFramework mediator abstractions to the underlying martinothamar/Mediator implementation.
/// </summary>
public sealed class MediatorAdapter : IMediator
{
    private readonly global::Mediator.IMediator _innerMediator;

    /// <summary>
    /// Creates a new mediator adapter.
    /// </summary>
    /// <param name="innerMediator">The underlying Mediator instance.</param>
    public MediatorAdapter(global::Mediator.IMediator innerMediator)
    {
        _innerMediator = innerMediator;
    }

    /// <inheritdoc />
    public ValueTask<TResult> SendAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default
    )
    {
        return sendObjectAsync<TResult>(command, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        return sendObjectAsync<TResult>(query, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResult> StreamAsync<TResult>(
        IStreamQuery<TResult> query,
        CancellationToken cancellationToken = default
    )
    {
        var sender = (global::Mediator.ISender)_innerMediator;
        return CastStream<TResult>(sender.CreateStream((object)query, cancellationToken), cancellationToken);
    }

    /// <inheritdoc />
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
                $"Mediator result type mismatch. Expected '{typeof(TResult).FullName}', received '{result?.GetType().FullName ?? "null"}'.\n"
                    + $"Troubleshooting:\n"
                    + $"  1. Verify your handler returns the correct type: ICommandHandler<TCommand, TResult> or IQuery<TQuery, TResult>\n"
                    + $"  2. Check that the command/query type in your request matches the registered handler\n"
                    + $"  3. Ensure your handler is registered in the DI container"
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
                $"Mediator stream item type mismatch. Expected '{typeof(TResult).FullName}', received '{item?.GetType().FullName ?? "null"}'.\n"
                    + $"Troubleshooting:\n"
                    + $"  1. Verify your stream query handler returns the correct type: IStreamQueryHandler<TStreamQuery, TResult>\n"
                    + $"  2. Check that the stream query type matches the registered handler\n"
                    + $"  3. Ensure your stream query handler is registered in the DI container"
            );
        }
    }
}
