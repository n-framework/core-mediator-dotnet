using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Abstractions.Contracts.Handlers;

/// <summary>
/// Handler contract for processing commands that produce a result.
/// Expected to be registered as transient in DI container.
/// </summary>
/// <typeparam name="TCommand">The command type being handled.</typeparam>
/// <typeparam name="TResult">The result type produced by command execution.</typeparam>
public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    ValueTask<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Handler contract for processing queries that return a result.
/// Expected to be registered as transient in DI container.
/// </summary>
/// <typeparam name="TQuery">The query type being handled.</typeparam>
/// <typeparam name="TResult">The result type returned by query execution.</typeparam>
public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    ValueTask<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}

/// <summary>
/// Handler contract for processing stream queries that return an async sequence.
/// Expected to be registered as transient in DI container.
/// </summary>
/// <typeparam name="TStreamQuery">The stream query type being handled.</typeparam>
/// <typeparam name="TResult">The type of items in the streamed sequence.</typeparam>
public interface IStreamQueryHandler<TStreamQuery, TResult>
    where TStreamQuery : IStreamQuery<TResult>
{
    IAsyncEnumerable<TResult> Handle(TStreamQuery query, CancellationToken cancellationToken);
}
