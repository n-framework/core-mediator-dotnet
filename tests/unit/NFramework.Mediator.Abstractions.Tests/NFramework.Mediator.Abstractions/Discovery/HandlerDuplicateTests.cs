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

        Assert.All(results, result => Assert.False(result.IsDiscoverable));
        Assert.All(results, result => Assert.Equal("Duplicate handler declaration", result.FailureReason));
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

        Assert.All(results, result => Assert.False(result.IsDiscoverable));
        Assert.All(results, result => Assert.Equal("Duplicate handler declaration", result.FailureReason));
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

        Assert.All(results, result => Assert.False(result.IsDiscoverable));
        Assert.All(results, result => Assert.Equal("Duplicate handler declaration", result.FailureReason));
    }
}