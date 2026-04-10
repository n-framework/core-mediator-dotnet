using NFramework.Mediator.Abstractions.Tests.Discovery.Fixtures;

namespace NFramework.Mediator.Abstractions.Tests.Discovery;

public sealed class HandlerDuplicateTests
{
    [Fact]
    public void ClassifyAll_DuplicateCommandHandlers_AreNotDiscoverable()
    {
        var results = HandlerDiscoverabilityClassifier.ClassifyAll(
            new[]
            {
                typeof(ValidHandlerFixtures.ValidCommandHandler),
                typeof(ValidHandlerFixtures.DuplicateCommandHandler)
            }
        );

        foreach (var result in results)
        {
            result.IsDiscoverable.ShouldBeFalse();
            result.FailureReason.ShouldBe("Duplicate handler declaration");
        }
    }

    [Fact]
    public void ClassifyAll_DuplicateQueryHandlers_AreNotDiscoverable()
    {
        var results = HandlerDiscoverabilityClassifier.ClassifyAll(
            new[]
            {
                typeof(ValidHandlerFixtures.ValidQueryHandler),
                typeof(ValidHandlerFixtures.DuplicateQueryHandler)
            }
        );

        foreach (var result in results)
        {
            result.IsDiscoverable.ShouldBeFalse();
            result.FailureReason.ShouldBe("Duplicate handler declaration");
        }
    }

    [Fact]
    public void ClassifyAll_DuplicateStreamHandlers_AreNotDiscoverable()
    {
        var results = HandlerDiscoverabilityClassifier.ClassifyAll(
            new[]
            {
                typeof(ValidHandlerFixtures.ValidStreamHandler),
                typeof(ValidHandlerFixtures.DuplicateStreamHandler)
            }
        );

        foreach (var result in results)
        {
            result.IsDiscoverable.ShouldBeFalse();
            result.FailureReason.ShouldBe("Duplicate handler declaration");
        }
    }
}