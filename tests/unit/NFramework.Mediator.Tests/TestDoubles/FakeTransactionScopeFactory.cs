using NFramework.Mediator.Behaviors;

namespace NFramework.Mediator.Tests.TestDoubles;

internal sealed class FakeTransactionScopeFactory : ITransactionScopeFactory
{
    private readonly ITransactionScope _scope;

    public FakeTransactionScopeFactory(ITransactionScope scope)
    {
        _scope = scope;
    }

    public ValueTask<ITransactionScope> CreateAsync(CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_scope);
    }
}
