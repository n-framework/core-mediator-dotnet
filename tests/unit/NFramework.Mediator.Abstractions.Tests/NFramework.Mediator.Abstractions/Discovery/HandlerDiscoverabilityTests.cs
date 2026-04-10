using NFramework.Mediator.Abstractions.Tests.Discovery.Fixtures;

namespace NFramework.Mediator.Abstractions.Tests.Discovery;

public sealed class HandlerDiscoverabilityTests
{
    [Fact]
    public void ClassifyAll_ValidShapes_AreDiscoverable()
    {
        var handlerTypes = new[]
        {
            typeof(ValidHandlerFixtures.ValidCommandHandler),
            typeof(ValidHandlerFixtures.ValidQueryHandler),
            typeof(ValidHandlerFixtures.ValidStreamHandler),
            typeof(ValidHandlerFixtures.ValidEventHandlerA),
        };

        var results = HandlerDiscoverabilityClassifier.ClassifyAll(handlerTypes);

        foreach (var result in results)
        {
            result.IsDiscoverable.ShouldBeTrue();
        }

        results.ShouldContain(r => r.Kind == HandlerKind.Command);
        results.ShouldContain(r => r.Kind == HandlerKind.Query);
        results.ShouldContain(r => r.Kind == HandlerKind.Stream);
        results.ShouldContain(r => r.Kind == HandlerKind.Event);
    }

    [Fact]
    public void ClassifyAll_InvalidShapes_AreNotDiscoverable()
    {
        var handlerTypes = new[]
        {
            typeof(InvalidHandlerFixtures.MissingInterfaceCommandHandler),
            typeof(InvalidHandlerFixtures.NonContractEventHandler),
        };

        var results = HandlerDiscoverabilityClassifier.ClassifyAll(handlerTypes);

        foreach (var result in results)
        {
            result.IsDiscoverable.ShouldBeFalse();
            result.FailureReason.ShouldNotBeNullOrWhiteSpace();
        }
    }
}
