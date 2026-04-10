using NFramework.Mediator.Abstractions.Tests.Discovery.Fixtures;

namespace NFramework.Mediator.Abstractions.Tests.Discovery;

public sealed class EventFanoutTests
{
    [Fact]
    public void ClassifyAll_EventFanout_AllowsMultipleHandlersForSameEvent()
    {
        var results = HandlerDiscoverabilityClassifier.ClassifyAll(
            new[] { typeof(ValidHandlerFixtures.ValidEventHandlerA), typeof(ValidHandlerFixtures.ValidEventHandlerB) }
        );

        Assert.Equal(2, results.Count);
        Assert.All(
            results,
            result =>
            {
                Assert.True(result.IsDiscoverable);
                Assert.Equal(HandlerKind.Event, result.Kind);
                Assert.Equal(typeof(ValidHandlerFixtures.OrderCreatedEvent), result.RequestType);
            }
        );
    }
}
