using NFramework.Mediator.Configuration;

namespace NFramework.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that manages transactions for request processing.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse> : global::Mediator.IPipelineBehavior<TRequest, TResponse>
    where TRequest : global::Mediator.IMessage
{
    private readonly ITransactionScopeFactory _transactionScopeFactory;
    private readonly IRequestPipelinePolicyProvider _pipelinePolicyProvider;

    /// <summary>
    /// Creates a new transaction behavior instance.
    /// </summary>
    /// <param name="transactionScopeFactory">The transaction scope factory.</param>
    /// <param name="pipelinePolicyProvider">The pipeline policy provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when transactionScopeFactory or pipelinePolicyProvider is null.</exception>
    public TransactionBehavior(
        ITransactionScopeFactory transactionScopeFactory,
        IRequestPipelinePolicyProvider pipelinePolicyProvider
    )
    {
        ArgumentNullException.ThrowIfNull(transactionScopeFactory);
        ArgumentNullException.ThrowIfNull(pipelinePolicyProvider);

        _transactionScopeFactory = transactionScopeFactory;
        _pipelinePolicyProvider = pipelinePolicyProvider;
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TRequest message,
        global::Mediator.MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var configuration = _pipelinePolicyProvider.GetConfiguration(typeof(TRequest));
        if (!configuration.UseTransaction)
        {
            return await next(message, cancellationToken);
        }

        await using var transactionScope = await _transactionScopeFactory.CreateAsync(cancellationToken);

        try
        {
            var response = await next(message, cancellationToken);
            await transactionScope.CommitAsync(cancellationToken);
            return response;
        }
        catch (OperationCanceledException)
        {
            await transactionScope.RollbackAsync(cancellationToken);
            throw;
        }
        catch (Exception)
        {
            await transactionScope.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
