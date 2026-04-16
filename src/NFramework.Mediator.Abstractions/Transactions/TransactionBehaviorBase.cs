using Microsoft.Extensions.Logging;

namespace NFramework.Mediator.Abstractions.Transactions;

/// <summary>
/// Manages atomicity by wrapping the request in an implementation-defined transaction scope.
/// </summary>
public abstract class TransactionBehaviorBase<TRequest, TResponse>(
    ILogger<TransactionBehaviorBase<TRequest, TResponse>> logger
)
{
    private readonly ILogger<TransactionBehaviorBase<TRequest, TResponse>> _logger = logger;

    private static readonly Action<ILogger, string, Exception?> LogTransactionErrorAction =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(1, nameof(HandleAsync)),
            "Transaction error occurred for request: {RequestName}"
        );

    private static readonly Action<ILogger, string, Exception?> LogRollbackErrorAction = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(2, nameof(HandleAsync)),
        "Failed to rollback transaction for request: {RequestName}"
    );

    protected virtual int TransactionScopeTimeoutSeconds => 30;

    /// <returns>The response from the next handler in the pipeline.</returns>
    protected async ValueTask<TResponse> HandleAsync(
        TRequest request,
        Func<CancellationToken, ValueTask<TResponse>> next,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(next);

        if (request is not ITransactionalRequest)
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }

        var transactionScope = await CreateTransactionScopeAsync(cancellationToken).ConfigureAwait(false);
        await using (transactionScope.ConfigureAwait(false))
        {
            TResponse response;
            try
            {
                response = await next(cancellationToken).ConfigureAwait(false);
                await transactionScope.CommitAsync(cancellationToken).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogTransactionErrorAction(_logger, typeof(TRequest).Name, ex);

                try
                {
                    await transactionScope.RollbackAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (InvalidOperationException rollbackEx)
                {
                    LogRollbackErrorAction(_logger, typeof(TRequest).Name, rollbackEx);
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Implement to provide the target transaction mechanism (e.g. System.Transactions or EF Core).
    /// </summary>
    /// <returns>A new <see cref="ITransactionScope"/> instance.</returns>
    protected abstract ValueTask<ITransactionScope> CreateTransactionScopeAsync(CancellationToken cancellationToken);
}
