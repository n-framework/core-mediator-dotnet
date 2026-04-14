using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Transactions;

/// <summary>
/// Manages atomicity by wrapping the request in an implementation-defined transaction scope.
/// </summary>
public abstract class TransactionBehaviorBase<TRequest, TResponse>
{
    private readonly ILogger<TransactionBehaviorBase<TRequest, TResponse>> _logger;

    protected TransactionBehaviorBase(ILogger<TransactionBehaviorBase<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    protected virtual int TransactionScopeTimeoutSeconds => 30;

    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not ITransactionalRequest)
        {
            return await next(cancellationToken);
        }

        await using var transactionScope = await CreateTransactionScopeAsync(cancellationToken);

        TResponse response;
        try
        {
            response = await next(cancellationToken);
            await transactionScope.CommitAsync(cancellationToken);
            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Transaction error occurred for request: {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }

    /// <summary>
    /// Implement to provide the target transaction mechanism (e.g. System.Transactions or EF Core).
    /// </summary>
    protected abstract ValueTask<ITransactionScope> CreateTransactionScopeAsync(CancellationToken cancellationToken);
}
