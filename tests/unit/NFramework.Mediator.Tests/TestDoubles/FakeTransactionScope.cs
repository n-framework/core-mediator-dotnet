using NFramework.Mediator.Behaviors;

namespace NFramework.Mediator.Tests.TestDoubles;

internal sealed class FakeTransactionScope : ITransactionScope
{
    public int CommitCallCount { get; private set; }

    public int RollbackCallCount { get; private set; }

    public Exception? CommitException { get; set; }

    public Exception? RollbackException { get; set; }

    public ValueTask CommitAsync(CancellationToken cancellationToken)
    {
        CommitCallCount++;
        if (CommitException is not null)
        {
            throw CommitException;
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask RollbackAsync(CancellationToken cancellationToken)
    {
        RollbackCallCount++;
        if (RollbackException is not null)
        {
            throw RollbackException;
        }
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
