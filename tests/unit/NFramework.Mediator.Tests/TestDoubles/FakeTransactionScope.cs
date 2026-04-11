using NFramework.Mediator.Behaviors;

namespace NFramework.Mediator.Tests.TestDoubles;

internal sealed class FakeTransactionScope : ITransactionScope
{
    public int CommitCallCount { get; private set; }

    public int RollbackCallCount { get; private set; }

    public ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        CommitCallCount++;
        return ValueTask.CompletedTask;
    }

    public ValueTask RollbackAsync(CancellationToken cancellationToken)
    {
        RollbackCallCount++;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
