using FluentAssertions;
using NFramework.Mediator.Behaviors;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCommit_WhenHandlerSucceeds()
    {
        var transactionScope = new TestDoubles.FakeTransactionScope();
        var behavior = new TransactionBehavior<TestRequest, string>(
            new TestDoubles.FakeTransactionScopeFactory(transactionScope)
        );

        string response = await behavior.Handle(new TestRequest(), (_, _) => ValueTask.FromResult("ok"), default);

        _ = response.Should().Be("ok");
        _ = transactionScope.CommitCallCount.Should().Be(1);
        _ = transactionScope.RollbackCallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldRollback_WhenHandlerThrows()
    {
        var transactionScope = new TestDoubles.FakeTransactionScope();
        var behavior = new TransactionBehavior<TestRequest, string>(
            new TestDoubles.FakeTransactionScopeFactory(transactionScope)
        );

        var action = async () =>
            await behavior.Handle(
                new TestRequest(),
                (_, _) => ValueTask.FromException<string>(new InvalidOperationException("boom")),
                default
            );

        _ = await action.Should().ThrowAsync<InvalidOperationException>();
        _ = transactionScope.CommitCallCount.Should().Be(0);
        _ = transactionScope.RollbackCallCount.Should().Be(1);
    }

    private sealed record TestRequest : global::Mediator.IMessage;
}
