namespace NFramework.Mediator.MartinothamarMediator.Behaviors;

/// <summary>
/// Represents a transaction scope that can be committed or rolled back.
/// </summary>
public interface ITransactionScope : IAsyncDisposable
{
    /// <summary>
    /// Commits the transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask CommitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back the transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    ValueTask RollbackAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Factory for creating transaction scopes.
/// </summary>
public interface ITransactionScopeFactory
{
    /// <summary>
    /// Creates a new transaction scope.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A new transaction scope.</returns>
    ValueTask<ITransactionScope> CreateAsync(CancellationToken cancellationToken);
}
