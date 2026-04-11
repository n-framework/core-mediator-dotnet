namespace NFramework.Mediator.Behaviors;

public interface ITransactionScope : IAsyncDisposable
{
    ValueTask CommitAsync(CancellationToken cancellationToken);

    ValueTask RollbackAsync(CancellationToken cancellationToken);
}

public interface ITransactionScopeFactory
{
    ValueTask<ITransactionScope> CreateAsync(CancellationToken cancellationToken);
}
