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

        results.Count.ShouldBe(2);
        foreach (var result in results)
        {
            result.IsDiscoverable.ShouldBeTrue();
            result.Kind.ShouldBe(HandlerKind.Event);
            result.RequestType.ShouldBe(typeof(ValidHandlerFixtures.OrderCreatedEvent));
        }
    }
}