using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Transactions;
using NFramework.Mediator.MartinothamarMediator.Transactions;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsTransaction_WhenRequestDoesNotImplementITransactional()
    {
        var logger = new LoggerFactory().CreateLogger<TransactionBehavior<NonTransactionalRequest, string>>();
        var behavior = new TransactionBehavior<NonTransactionalRequest, string>(logger);
        bool handlerInvoked = false;
        MessageHandlerDelegate<NonTransactionalRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new NonTransactionalRequest(), next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_CompletesTransaction_WhenHandlerSucceeds()
    {
        var logger = new LoggerFactory().CreateLogger<TransactionBehavior<TransactionalRequest, string>>();
        var behavior = new TransactionBehavior<TransactionalRequest, string>(logger);
        bool handlerInvoked = false;
        MessageHandlerDelegate<TransactionalRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new TransactionalRequest(), next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_PropagatesException_WhenHandlerThrows()
    {
        var logger = new LoggerFactory().CreateLogger<TransactionBehavior<TransactionalRequest, string>>();
        var behavior = new TransactionBehavior<TransactionalRequest, string>(logger);
        MessageHandlerDelegate<TransactionalRequest, string> next = (_, _) =>
            throw new InvalidOperationException("Test exception");

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new TransactionalRequest(), next, default).AsTask()
        );
    }

    private sealed record NonTransactionalRequest : IMessage;

    private sealed record TransactionalRequest : IMessage, ITransactionalRequest;
}
