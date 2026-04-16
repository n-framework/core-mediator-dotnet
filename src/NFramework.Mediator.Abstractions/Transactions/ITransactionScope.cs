namespace NFramework.Mediator.Abstractions.Transactions;

/// <summary>
/// Handles environment-specific transaction lifecycle (System.Transactions, IDbTransaction, etc.).
/// </summary>
public interface ITransactionScope : IAsyncDisposable
{
    /// <summary>
    /// Commits the transaction.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask CommitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask RollbackAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Factory for creating transaction scopes with specific configuration.
/// </summary>
public interface ITransactionScopeFactory
{
    /// <summary>
    /// Creates a new transaction scope with the specified timeout.
    /// </summary>
    /// <returns>A new <see cref="ITransactionScope"/> instance.</returns>
    ValueTask<ITransactionScope> CreateAsync(TimeSpan timeout, CancellationToken cancellationToken);
}
