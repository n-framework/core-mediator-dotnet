using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Abstractions.Contracts.Handlers;

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    ValueTask<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    ValueTask<TResult> Handle(TQuery query, CancellationToken cancellationToken);
}

public interface IStreamQueryHandler<TStreamQuery, TResult>
    where TStreamQuery : IStreamQuery<TResult>
{
    IAsyncEnumerable<TResult> Handle(TStreamQuery query, CancellationToken cancellationToken);
}
