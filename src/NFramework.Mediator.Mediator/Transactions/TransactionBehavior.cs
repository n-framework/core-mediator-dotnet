using System.Transactions;
using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions;
using NFramework.Mediator.Abstractions.Transactions;
using UnionRailway;

namespace NFramework.Mediator.Mediator.Transactions;

/// <summary>
/// Wraps transactional <see cref="IRailRequest{TValue}"/> requests in a <see cref="TransactionScope"/>
/// and returns a railway <see cref="UnionError.SystemFailure"/> on failure instead of re-throwing.
/// </summary>
public sealed class TransactionBehavior<TRequest, TValue>(
    ILogger<TransactionBehavior<TRequest, TValue>> logger,
    MediatorTransactionOptions options
) : IPipelineBehavior<TRequest, Rail<TValue>>
    where TRequest : IRailRequest<TValue>
{
    private static readonly Action<ILogger, string, Exception?> LogTransactionError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(Handle)),
            "Transaction error occurred for request: {RequestName}"
        );

    private static readonly Action<ILogger, string, Exception?> LogRollbackError =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, nameof(Handle)),
            "Failed to rollback transaction for request: {RequestName}"
        );

    /// <inheritdoc />
    public async ValueTask<Rail<TValue>> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, Rail<TValue>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not ITransactionalRequest)
        {
            return await next(request, cancellationToken).ConfigureAwait(false);
        }

        var transactionScope = CreateTransactionScope();
        await using (transactionScope.ConfigureAwait(false))
        {
            try
            {
                Rail<TValue> response = await next(request, cancellationToken).ConfigureAwait(false);
                await transactionScope.CommitAsync(cancellationToken).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogTransactionError(logger, typeof(TRequest).Name, ex);

                try
                {
                    await transactionScope.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (InvalidOperationException rollbackEx)
                {
                    LogRollbackError(logger, typeof(TRequest).Name, rollbackEx);
                }

                return new UnionError.SystemFailure(ex);
            }
        }
    }

    private SystemTransactionScope CreateTransactionScope()
    {
#pragma warning disable CA2000 // Ownership transferred to caller via await using
        var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = options.TransactionScopeTimeout,
            },
            TransactionScopeAsyncFlowOption.Enabled
        );
        return new SystemTransactionScope(scope);
#pragma warning restore CA2000
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
