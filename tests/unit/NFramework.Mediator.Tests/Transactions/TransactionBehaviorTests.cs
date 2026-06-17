using Mediator;
using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Abstractions;
using NFramework.Mediator.Abstractions.Transactions;
using NFramework.Mediator.Tests.Railway;
using UnionRailway;

#pragma warning disable CS8600, CS8601 // Test assertions guarantee non-null values after ShouldBeTrue()

namespace NFramework.Mediator.Tests.Transactions;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task NonTransactionalRequest_PassesThrough()
    {
        IMediator mediator = BuildMediator(out HandlerExecutionSpy spy);

        Rail<int> result = await mediator.Send(new NonTransactionalRailRequest());

        result.IsSuccess(out int value, out _).ShouldBeTrue();
        value.ShouldBe(42);
        spy.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task TransactionalSuccess_CommitsAndReturnsSuccess()
    {
        IMediator mediator = BuildMediator(out HandlerExecutionSpy spy);

        Rail<int> result = await mediator.Send(new TransactionalRailRequest(ShouldThrow: false));

        result.IsSuccess(out int value, out _).ShouldBeTrue();
        value.ShouldBe(42);
        spy.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task TransactionalHandlerThrows_ReturnsSystemFailure()
    {
        IMediator mediator = BuildMediator(out HandlerExecutionSpy spy);

        Rail<int> result = await mediator.Send(new TransactionalRailRequest(ShouldThrow: true));

        result.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        error!.Value.TryGet(out UnionError.SystemFailure failure).ShouldBeTrue();
        failure.Ex.Message.ShouldBe("boom");
        spy.Executed.ShouldBeTrue();
    }

    private static IMediator BuildMediator(out HandlerExecutionSpy spy)
    {
        spy = new HandlerExecutionSpy();
        return RailwayTestHost.CreateServices(spy).BuildServiceProvider().GetRequiredService<IMediator>();
    }

    internal sealed record NonTransactionalRailRequest : IRailRequest<int>;

    internal sealed record TransactionalRailRequest(bool ShouldThrow) : IRailRequest<int>, ITransactionalRequest;

    internal sealed class NonTransactionalHandler(HandlerExecutionSpy spy)
        : IRequestHandler<NonTransactionalRailRequest, Rail<int>>
    {
        public ValueTask<Rail<int>> Handle(NonTransactionalRailRequest request, CancellationToken cancellationToken)
        {
            spy.Executed = true;
            return ValueTask.FromResult<Rail<int>>(Union.Ok(42));
        }
    }

    internal sealed class TransactionalHandler(HandlerExecutionSpy spy)
        : IRequestHandler<TransactionalRailRequest, Rail<int>>
    {
        public ValueTask<Rail<int>> Handle(TransactionalRailRequest request, CancellationToken cancellationToken)
        {
            spy.Executed = true;
            if (request.ShouldThrow)
            {
                throw new InvalidOperationException("boom");
            }

            return ValueTask.FromResult<Rail<int>>(Union.Ok(42));
        }
    }
}
