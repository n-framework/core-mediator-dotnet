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

        result.IsDiscoverable.ShouldBeFalse();
        result.FailureReason.ShouldBe("Open generic handler types are not discoverable");
    }

    [Fact]
    public void Classify_MissingContract_ReturnsExplicitFailureReason()
    {
        var result = HandlerDiscoverabilityClassifier.Classify(
            typeof(InvalidHandlerFixtures.MissingInterfaceCommandHandler)
        );

        result.IsDiscoverable.ShouldBeFalse();
        result.FailureReason.ShouldBe("Type does not implement a supported handler contract");
    }
}
