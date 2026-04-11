using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Behaviors;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class BehaviorOrderTests
{
    [Fact]
    public void AddMediatorBehaviors_ShouldRegisterDefaultOrder()
    {
        var services = new ServiceCollection();

        _ = services.AddMediatorBehaviors();

        var registrations = services
            .Where(descriptor => descriptor.ServiceType == typeof(global::Mediator.IPipelineBehavior<,>))
            .Select(descriptor => descriptor.ImplementationType)
            .ToArray();

        _ = registrations
            .Should()
            .ContainInOrder(typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>), typeof(TransactionBehavior<,>));
    }

    [Fact]
    public void AddMediatorBehaviors_ShouldRespectCustomOrder()
    {
        var services = new ServiceCollection();

        _ = services.AddMediatorBehaviors(options =>
        {
            options.SetOrder<ValidationBehavior<TestMessage, object>>(100);
            options.SetOrder<LoggingBehavior<TestMessage, object>>(200);
            options.SetOrder<TransactionBehavior<TestMessage, object>>(300);
        });

        var registrations = services
            .Where(descriptor => descriptor.ServiceType == typeof(global::Mediator.IPipelineBehavior<,>))
            .Select(descriptor => descriptor.ImplementationType)
            .ToArray();

        _ = registrations
            .Should()
            .ContainInOrder(typeof(ValidationBehavior<,>), typeof(LoggingBehavior<,>), typeof(TransactionBehavior<,>));
    }

    [Fact]
    public void AddMediatorBehaviors_ShouldRequireExplicitRequestConfigurationByDefault()
    {
        var services = new ServiceCollection();
        _ = services.AddMediatorBehaviors();
        var provider = services.BuildServiceProvider();

        var policyProvider =
            provider.GetRequiredService<NFramework.Mediator.Configuration.IRequestPipelinePolicyProvider>();

        var action = () => policyProvider.GetConfiguration(typeof(TestMessage));
        _ = action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddMediatorBehaviors_ShouldReturnConfiguredPipelinesPerRequest()
    {
        var services = new ServiceCollection();

        _ = services.AddMediatorBehaviors(options =>
        {
            options.ConfigureFor<TestMessage>(pipeline => pipeline.UseLogging().UseValidation());
        });

        var provider = services.BuildServiceProvider();
        var policyProvider =
            provider.GetRequiredService<NFramework.Mediator.Configuration.IRequestPipelinePolicyProvider>();

        var configuration = policyProvider.GetConfiguration(typeof(TestMessage));

        _ = configuration.UseLogging.Should().BeTrue();
        _ = configuration.UseValidation.Should().BeTrue();
        _ = configuration.UseTransaction.Should().BeFalse();
    }

    private sealed record TestMessage : global::Mediator.IMessage;
}
