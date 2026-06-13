using Mediator;
using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Mediator.Caching;
using NFramework.Mediator.Mediator.Configuration;
using NFramework.Mediator.Mediator.Logging;
using NFramework.Mediator.Mediator.Performance;
using NFramework.Mediator.Mediator.Validation.FluentValidation;
using Shouldly;
using Xunit;

namespace NFramework.Mediator.Tests.Integration;

public class BehaviorOrderTests
{
    private sealed record DummyRequest : IMessage;

    [Fact]
    public void SeparateExtensions_RegistersBehaviorsInCorrectOrder()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Register remaining behaviors in the desired order
        // Authorization and Transaction are now handled by the railway pipeline
        // via AddNFrameworkRailwayBehaviors (closed registrations from source generator).
        _ = services
            .AddNFrameworkLogging()
            .AddNFrameworkCaching()
            .AddNFrameworkPerformance()
            .AddNFrameworkFluentValidation();

        var registeredBehaviors = services
            .Where(sd =>
                sd.ServiceType == typeof(IPipelineBehavior<DummyRequest, string>)
                || (
                    sd.ServiceType.IsGenericType
                    && sd.ServiceType.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)
                )
            )
            .Select(sd => sd.ImplementationType)
            .ToList();

        // Assert - expected order matches registration order in martinothamar/Mediator
        registeredBehaviors.Count.ShouldBe(5);

        registeredBehaviors[0].ShouldBe(typeof(LoggingBehavior<,>));
        registeredBehaviors[1].ShouldBe(typeof(CachingBehavior<,>));
        registeredBehaviors[2].ShouldBe(typeof(CacheRemovingBehavior<,>));
        registeredBehaviors[3].ShouldBe(typeof(PerformanceBehavior<,>));
        registeredBehaviors[4].ShouldBe(typeof(ValidationBehavior<,>));
    }
}
