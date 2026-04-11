namespace NFramework.Mediator.Tests.TestDoubles;

internal sealed class FakeMediator : global::Mediator.IMediator
{
    private readonly Func<object, object?> _sendHandler;
    private readonly Func<object, IAsyncEnumerable<object>>? _streamHandler;

    public FakeMediator(Func<object, object?> sendHandler, Func<object, IAsyncEnumerable<object>>? streamHandler = null)
    {
        _sendHandler = sendHandler;
        _streamHandler = streamHandler;
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
        if (_streamHandler is null)
        {
            throw new NotSupportedException("Streaming is not configured in this test.");
        }

        var stream = _streamHandler(query);
        return stream.Cast<TResponse>();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        global::Mediator.IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default
    )
    {
        if (_streamHandler is null)
        {
            throw new NotSupportedException("Streaming is not configured in this test.");
        }

        var stream = _streamHandler(request);
        return stream.Cast<TResponse>();
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        global::Mediator.IStreamCommand<TResponse> command,
        CancellationToken cancellationToken = default
    )
    {
        if (_streamHandler is null)
        {
            throw new NotSupportedException("Streaming is not configured in this test.");
        }

        var stream = _streamHandler(command);
        return stream.Cast<TResponse>();
    }

    public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        if (_streamHandler is null)
        {
            throw new NotSupportedException("Streaming is not configured in this test.");
        }

        return _streamHandler(request);
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
