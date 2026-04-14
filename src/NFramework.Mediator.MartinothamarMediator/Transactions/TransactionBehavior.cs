using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Transactions;

namespace NFramework.Mediator.MartinothamarMediator.Transactions;

/// <summary>
/// Martinothamar Mediator implementation for atomic request processing using <see cref="TransactionScope"/>.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>(ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : TransactionBehaviorBase<TRequest, TResponse>(logger),
        IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    protected override ValueTask<ITransactionScope> CreateTransactionScopeAsync(CancellationToken cancellationToken)
    {
        var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted,
                Timeout = TimeSpan.FromSeconds(TransactionScopeTimeoutSeconds),
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
        return await HandleAsync(request, ct => next(request, ct), cancellationToken);
    }

    private sealed class SystemTransactionScope(TransactionScope scope) : ITransactionScope
    {
        private readonly TransactionScope _scope = scope;

        public ValueTask CommitAsync(CancellationToken cancellationToken)
        {
            _scope.Complete();
            return default;
        }

        public ValueTask RollbackAsync(CancellationToken cancellationToken) => default;

        public ValueTask DisposeAsync()
        {
            _scope.Dispose();
            return default;
        }
    }
}
