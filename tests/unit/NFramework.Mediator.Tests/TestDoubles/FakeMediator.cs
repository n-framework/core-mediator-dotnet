namespace NFramework.Mediator.Tests.TestDoubles;

internal sealed class FakeMediator : global::Mediator.IMediator
{
    private readonly Func<object, object?> _sendHandler;

    public FakeMediator(Func<object, object?> sendHandler)
    {
        _sendHandler = sendHandler;
    }

    public List<object> PublishedEvents { get; } = [];

    public ValueTask<TResponse> Send<TResponse>(
        global::Mediator.IRequest<TResponse> request,
        CancellationToken cancellationToken = default
    )
    {
        return ValueTask.FromResult((TResponse)_sendHandler(request)!);
    }

    public ValueTask<TResponse> Send<TResponse>(
        global::Mediator.ICommand<TResponse> command,
        CancellationToken cancellationToken = default
    )
    {
        return ValueTask.FromResult((TResponse)_sendHandler(command)!);
    }

    public ValueTask<TResponse> Send<TResponse>(
        global::Mediator.IQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
    {
        return ValueTask.FromResult((TResponse)_sendHandler(query)!);
    }

    public ValueTask<object?> Send(object message, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(_sendHandler(message));
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        global::Mediator.IStreamQuery<TResponse> query,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotSupportedException("Streaming is not needed in current tests.");
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        global::Mediator.IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotSupportedException("Streaming is not needed in current tests.");
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        global::Mediator.IStreamCommand<TResponse> command,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotSupportedException("Streaming is not needed in current tests.");
    }

    public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Streaming is not needed in current tests.");
    }

    public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : global::Mediator.INotification
    {
        PublishedEvents.Add(notification!);
        return ValueTask.CompletedTask;
    }

    public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
    {
        PublishedEvents.Add(notification);
        return ValueTask.CompletedTask;
    }
}
