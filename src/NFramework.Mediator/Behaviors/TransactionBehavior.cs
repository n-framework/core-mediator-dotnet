using NFramework.Mediator.Configuration;

namespace NFramework.Mediator.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse> : global::Mediator.IPipelineBehavior<TRequest, TResponse>
    where TRequest : global::Mediator.IMessage
{
    private readonly ITransactionScopeFactory _transactionScopeFactory;
    private readonly IRequestPipelinePolicyProvider? _pipelinePolicyProvider;

    public TransactionBehavior(
        ITransactionScopeFactory transactionScopeFactory,
        IRequestPipelinePolicyProvider? pipelinePolicyProvider = null
    )
    {
        _transactionScopeFactory = transactionScopeFactory;
        _pipelinePolicyProvider = pipelinePolicyProvider;
    }

    public async ValueTask<TResponse> Handle(
        TRequest message,
        global::Mediator.MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (_pipelinePolicyProvider is not null)
        {
            var configuration = _pipelinePolicyProvider.GetConfiguration(typeof(TRequest));
            if (!configuration.UseTransaction)
            {
                return await next(message, cancellationToken);
            }
        }

        await using var transactionScope = await _transactionScopeFactory.CreateAsync(cancellationToken);

        try
        {
            var response = await next(message, cancellationToken);
            await transactionScope.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transactionScope.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
