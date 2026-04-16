using Mediator;
using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Mediator.Authorization;
using NFramework.Mediator.Mediator.Caching;
using NFramework.Mediator.Mediator.Configuration;
using NFramework.Mediator.Mediator.Logging;
using NFramework.Mediator.Mediator.Performance;
using NFramework.Mediator.Mediator.Transactions;
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

        // Act - Register behaviors in the desired order
        _ = services
            .AddNFrameworkLogging()
            .AddNFrameworkAuthorization()
            .AddNFrameworkTransactions()
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

        // Assert
        // Expected order matches registration order in martinothamar/Mediator
        registeredBehaviors.Count.ShouldBe(7);

        registeredBehaviors[0].ShouldBe(typeof(LoggingBehavior<,>));
        registeredBehaviors[1].ShouldBe(typeof(AuthorizationBehavior<,>));
        registeredBehaviors[2].ShouldBe(typeof(TransactionBehavior<,>));
        registeredBehaviors[3].ShouldBe(typeof(CachingBehavior<,>));
        registeredBehaviors[4].ShouldBe(typeof(CacheRemovingBehavior<,>));
        registeredBehaviors[5].ShouldBe(typeof(PerformanceBehavior<,>));
        registeredBehaviors[6].ShouldBe(typeof(ValidationBehavior<,>));
    }
}
