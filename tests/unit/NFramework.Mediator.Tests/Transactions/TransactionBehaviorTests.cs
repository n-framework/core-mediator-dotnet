using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Transactions;
using NFramework.Mediator.Mediator.Transactions;

namespace NFramework.Mediator.Tests.Transactions;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsTransaction_WhenRequestDoesNotImplementITransactional()
    {
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<TransactionBehavior<NonTransactionalRequest, string>>();
        TransactionBehavior<NonTransactionalRequest, string> behavior = new TransactionBehavior<
            NonTransactionalRequest,
            string
        >(logger, new MediatorTransactionOptions());
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
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<TransactionBehavior<TransactionalRequest, string>>();
        TransactionBehavior<TransactionalRequest, string> behavior = new TransactionBehavior<
            TransactionalRequest,
            string
        >(logger, new MediatorTransactionOptions());
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
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<TransactionBehavior<TransactionalRequest, string>>();
        TransactionBehavior<TransactionalRequest, string> behavior = new TransactionBehavior<
            TransactionalRequest,
            string
        >(logger, new MediatorTransactionOptions());
        MessageHandlerDelegate<TransactionalRequest, string> next = (_, _) =>
            throw new InvalidOperationException("Test exception");

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new TransactionalRequest(), next, default).AsTask()
        );
    }

    [Fact]
    public async Task Handle_RollsBackTransaction_WhenHandlerThrows()
    {
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<TestTransactionBehavior<TransactionalRequest, string>>();
        await using var scope = new FakeTransactionScope();
        TestTransactionBehavior<TransactionalRequest, string> behavior = new TestTransactionBehavior<
            TransactionalRequest,
            string
        >(logger, scope);

        MessageHandlerDelegate<TransactionalRequest, string> next = (_, _) =>
            throw new InvalidOperationException("Test exception");

        _ = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new TransactionalRequest(), ct => next(new TransactionalRequest(), ct), default).AsTask()
        );

        scope.Committed.ShouldBeFalse();
        scope.RolledBack.ShouldBeTrue();
        scope.Disposed.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_RespectsCancellationToken()
    {
        using var ctSource = new CancellationTokenSource();
        var ct = ctSource.Token;

        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<TestTransactionBehavior<TransactionalRequest, string>>();
        await using var scope = new FakeTransactionScope();
        TestTransactionBehavior<TransactionalRequest, string> behavior = new TestTransactionBehavior<
            TransactionalRequest,
            string
        >(logger, scope);

        MessageHandlerDelegate<TransactionalRequest, string> next = (_, t) =>
        {
            t.ShouldBe(ct);
            return ValueTask.FromResult("success");
        };

        _ = await behavior.Handle(new TransactionalRequest(), ct => next(new TransactionalRequest(), ct), ct);

        scope.Committed.ShouldBeTrue();
        scope.Disposed.ShouldBeTrue();
    }

    [Fact]
    public void Handle_RespectsTimeoutConfiguration()
    {
        using var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<TransactionBehavior<TransactionalRequest, string>>();
        MediatorTransactionOptions options = new MediatorTransactionOptions
        {
            TransactionScopeTimeout = TimeSpan.FromSeconds(45),
        };

        _ = new TransactionBehavior<TransactionalRequest, string>(logger, options);

        // Accessing protected property via reflection or just trust the code if it's simple enough
        // But the best is to verify if it's passed to the scope.
        // However, TransactionScope doesn't expose its timeout easily.
        // We can at least verify the override logic.

        // Since TransactionScopeTimeoutSeconds is protected, we can't easily check it here without a subclass.
        TestTimeoutTransactionBehavior<TransactionalRequest, string> testBehavior = new TestTimeoutTransactionBehavior<
            TransactionalRequest,
            string
        >(logger, options);
        testBehavior.GetTimeoutSeconds().ShouldBe(45);
    }

    private sealed class TestTimeoutTransactionBehavior<TReq, TRes>(
        ILogger<TransactionBehavior<TReq, TRes>> logger,
        MediatorTransactionOptions options
    ) : TransactionBehavior<TReq, TRes>(logger, options)
        where TReq : IMessage
    {
        public int GetTimeoutSeconds() => TransactionScopeTimeoutSeconds;
    }

    private sealed class FakeTransactionScope : ITransactionScope
    {
        public bool Committed { get; private set; }
        public bool RolledBack { get; private set; }
        public bool Disposed { get; private set; }

        public ValueTask CommitAsync(CancellationToken cancellationToken)
        {
            Committed = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask RollbackAsync(CancellationToken cancellationToken)
        {
            RolledBack = true;
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }

    internal sealed record NonTransactionalRequest : IMessage;

    internal sealed record TransactionalRequest : IMessage, ITransactionalRequest;

    internal sealed class TestTransactionBehavior<TReq, TRes>(
        ILogger<TestTransactionBehavior<TReq, TRes>> logger,
        ITransactionScope scope
    ) : TransactionBehaviorBase<TReq, TRes>(logger)
    {
        protected override ValueTask<ITransactionScope> CreateTransactionScopeAsync(
            CancellationToken cancellationToken
        ) => ValueTask.FromResult(scope);

        public async ValueTask<TRes> Handle(
            TReq request,
            Func<CancellationToken, ValueTask<TRes>> next,
            CancellationToken ct
        ) => await HandleAsync(request, next, ct).ConfigureAwait(false);
    }
}
