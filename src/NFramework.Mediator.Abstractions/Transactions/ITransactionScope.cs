using System;
using System.Threading.Tasks;

namespace NFramework.Mediator.Abstractions.Transactions;

/// <summary>
/// Handles environment-specific transaction lifecycle (System.Transactions, IDbTransaction, etc.).
/// </summary>
public interface ITransactionScope : IAsyncDisposable
{
    ValueTask CommitAsync(CancellationToken cancellationToken);

    ValueTask RollbackAsync(CancellationToken cancellationToken);
}

public interface ITransactionScopeFactory
{
    ValueTask<ITransactionScope> CreateAsync(TimeSpan timeout, CancellationToken cancellationToken);
}
