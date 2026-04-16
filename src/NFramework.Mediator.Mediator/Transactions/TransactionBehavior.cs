using System.Transactions;
using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Transactions;

namespace NFramework.Mediator.Mediator.Transactions;

/// <summary>
/// Martinothamar Mediator implementation for atomic request processing using <see cref="TransactionScope"/>.
/// </summary>
public class TransactionBehavior<TRequest, TResponse>(
    ILogger<TransactionBehavior<TRequest, TResponse>> logger,
    MediatorTransactionOptions options
) : TransactionBehaviorBase<TRequest, TResponse>(logger), IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    protected override int TransactionScopeTimeoutSeconds => (int)options.TransactionScopeTimeout.TotalSeconds;

    protected override ValueTask<ITransactionScope> CreateTransactionScopeAsync(CancellationToken cancellationToken)
    {
        TransactionScope transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new System.Transactions.TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                Timeout = options.TransactionScopeTimeout,
            },
            TransactionScopeAsyncFlowOption.Enabled
        );

        return new ValueTask<ITransactionScope>(new SystemTransactionScope(transactionScope));
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return await HandleAsync(request, ct => next(request, ct), cancellationToken).ConfigureAwait(false);
    }

    private sealed class SystemTransactionScope(TransactionScope scope) : ITransactionScope
    {
        private readonly TransactionScope _scope = scope;
        private bool _committed;
        private bool _rolledBack;

        public ValueTask CommitAsync(CancellationToken cancellationToken)
        {
            if (_rolledBack)
            {
                throw new InvalidOperationException("Cannot commit a transaction that has been rolled back.");
            }

            if (!_committed)
            {
                _scope.Complete();
                _committed = true;
            }

            return default;
        }

        public ValueTask RollbackAsync(CancellationToken cancellationToken)
        {
            if (_committed)
            {
                throw new InvalidOperationException("Cannot rollback a transaction that has been committed.");
            }

            _rolledBack = true;
            return default;
        }

        public ValueTask DisposeAsync()
        {
            _scope.Dispose();
            return default;
        }
    }
}
