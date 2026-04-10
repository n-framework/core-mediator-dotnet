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

        Assert.All(results, result => Assert.True(result.IsDiscoverable));
        Assert.Contains(results, result => result.Kind == HandlerKind.Command);
        Assert.Contains(results, result => result.Kind == HandlerKind.Query);
        Assert.Contains(results, result => result.Kind == HandlerKind.Stream);
        Assert.Contains(results, result => result.Kind == HandlerKind.Event);
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

        Assert.All(results, result => Assert.False(result.IsDiscoverable));
        Assert.All(results, result => Assert.False(string.IsNullOrWhiteSpace(result.FailureReason)));
    }
}
