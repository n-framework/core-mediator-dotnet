using System.Transactions;
using Mediator;
using NFramework.Mediator.Abstractions.Behaviors;

namespace NFramework.Mediator.MartinothamarMediator.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITransactionalRequest)
        {
            return await next(request, cancellationToken);
        }

        using var transactionScope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var response = await next(request, cancellationToken);
        
        transactionScope.Complete();
        
        return response;
    }
}
