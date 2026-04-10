using NFramework.Mediator.Abstractions.Contracts.Handlers;
using NFramework.Mediator.Abstractions.Tests.Discovery.Fixtures;

namespace NFramework.Mediator.Abstractions.Tests.Discovery;

public sealed class HandlerBoundaryTests
{
    [Fact]
    public void ClassifyAll_EmptySet_ReturnsNoResults()
    {
        var results = HandlerDiscoverabilityClassifier.ClassifyAll(Array.Empty<Type>());

        results.Count.ShouldBe(0);
    }

    [Fact]
    public void ClassifyAll_DuplicateCommandHandlers_AreInvalid()
    {
        var results = HandlerDiscoverabilityClassifier.ClassifyAll(
            new[] { typeof(DuplicateCommandHandlerA), typeof(DuplicateCommandHandlerB) }
        );

        foreach (var result in results)
        {
            result.IsDiscoverable.ShouldBeFalse();
            result.FailureReason.ShouldBe("Duplicate handler declaration");
        }
    }

    [Fact]
    public void ClassifyAll_DuplicateQueryHandlers_AreInvalid()
    {
        var results = HandlerDiscoverabilityClassifier.ClassifyAll(
            new[] { typeof(DuplicateQueryHandlerA), typeof(DuplicateQueryHandlerB) }
        );

        foreach (var result in results)
        {
            result.IsDiscoverable.ShouldBeFalse();
        }
    }

    [Fact]
    public void ClassifyAll_DuplicateStreamHandlers_AreInvalid()
    {
        var results = HandlerDiscoverabilityClassifier.ClassifyAll(
            new[] { typeof(DuplicateStreamHandlerA), typeof(DuplicateStreamHandlerB) }
        );

        foreach (var result in results)
        {
            result.IsDiscoverable.ShouldBeFalse();
        }
    }

    private sealed class DuplicateCommandHandlerA : ICommandHandler<ValidHandlerFixtures.CreateOrderCommand, int>
    {
        public ValueTask<int> Handle(
            ValidHandlerFixtures.CreateOrderCommand command,
            CancellationToken cancellationToken
        ) => ValueTask.FromResult(command.Quantity);
    }

    private sealed class DuplicateCommandHandlerB : ICommandHandler<ValidHandlerFixtures.CreateOrderCommand, int>
    {
        public ValueTask<int> Handle(
            ValidHandlerFixtures.CreateOrderCommand command,
            CancellationToken cancellationToken
        ) => ValueTask.FromResult(command.Quantity);
    }

    private sealed class DuplicateQueryHandlerA : IQueryHandler<ValidHandlerFixtures.GetOrderQuery, string>
    {
        public ValueTask<string> Handle(
            ValidHandlerFixtures.GetOrderQuery query,
            CancellationToken cancellationToken
        ) => ValueTask.FromResult(query.OrderId);
    }

    private sealed class DuplicateQueryHandlerB : IQueryHandler<ValidHandlerFixtures.GetOrderQuery, string>
    {
        public ValueTask<string> Handle(
            ValidHandlerFixtures.GetOrderQuery query,
            CancellationToken cancellationToken
        ) => ValueTask.FromResult(query.OrderId);
    }

    private sealed class DuplicateStreamHandlerA : IStreamQueryHandler<ValidHandlerFixtures.GetOrderStreamQuery, string>
    {
        public async IAsyncEnumerable<string> Handle(
            ValidHandlerFixtures.GetOrderStreamQuery query,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            yield return query.CustomerId;
            await Task.CompletedTask;
        }
    }

    private sealed class DuplicateStreamHandlerB : IStreamQueryHandler<ValidHandlerFixtures.GetOrderStreamQuery, string>
    {
        public async IAsyncEnumerable<string> Handle(
            ValidHandlerFixtures.GetOrderStreamQuery query,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken
        )
        {
            yield return query.CustomerId;
            await Task.CompletedTask;
        }
    }
}
