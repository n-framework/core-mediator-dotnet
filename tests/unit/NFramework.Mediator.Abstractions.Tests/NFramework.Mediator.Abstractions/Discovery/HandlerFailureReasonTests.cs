using NFramework.Mediator.Abstractions.Tests.Discovery.Fixtures;

namespace NFramework.Mediator.Abstractions.Tests.Discovery;

public sealed class HandlerFailureReasonTests
{
    [Fact]
    public void Classify_OpenGenericHandler_ReturnsExplicitFailureReason()
    {
        var result = HandlerDiscoverabilityClassifier.Classify(
            typeof(InvalidHandlerFixtures.OpenGenericCommandHandler<>)
        );

        Assert.False(result.IsDiscoverable);
        Assert.Equal("Open generic handler types are not discoverable", result.FailureReason);
    }

    [Fact]
    public void Classify_MissingContract_ReturnsExplicitFailureReason()
    {
        var result = HandlerDiscoverabilityClassifier.Classify(
            typeof(InvalidHandlerFixtures.MissingInterfaceCommandHandler)
        );

        Assert.False(result.IsDiscoverable);
        Assert.Equal("Type does not implement a supported handler contract", result.FailureReason);
    }
}
