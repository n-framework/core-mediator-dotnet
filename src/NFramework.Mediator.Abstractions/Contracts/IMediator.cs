using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Abstractions.Contracts;

/// <summary>
/// Mediator facade for dispatching commands, queries, stream queries, and events to their respective handlers.
/// All handlers should be registered as transient in the DI container.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a command to its registered handler and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the command.</typeparam>
    /// <param name="command">The command to send.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The result of command execution.</returns>
    ValueTask<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a query to its registered handler and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the query.</typeparam>
    /// <param name="query">The query to send.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The result of query execution.</returns>
    ValueTask<TResult> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams results from a stream query handler.
    /// </summary>
    /// <typeparam name="TResult">The type of items in the streamed sequence.</typeparam>
    /// <param name="query">The stream query to execute.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async sequence of results.</returns>
    IAsyncEnumerable<TResult> StreamAsync<TResult>(
        IStreamQuery<TResult> query,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Publishes an event to all registered handlers (fan-out).
    /// </summary>
    /// <typeparam name="TEvent">The event type to publish.</typeparam>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
}
