using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Abstractions.Contracts;
using NFramework.Mediator.Behaviors;
using NFramework.Mediator.Configuration;

namespace NFramework.Mediator;

/// <summary>
/// Extension methods for registering mediator services in the dependency injection container.
/// </summary>
public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers mediator pipeline behaviors with the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action for <see cref="MediatorBehaviorOptions"/>.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorBehaviors(
        this IServiceCollection services,
        Action<MediatorBehaviorOptions>? configure = null
    )
    {
        var options = new MediatorBehaviorOptions();
        configure?.Invoke(options);
        var policyProvider = options.BuildPolicyProvider();

        var behaviorRegistrations = new List<(Type BehaviorType, int Priority)>();

        if (options.EnableLogging)
        {
            behaviorRegistrations.Add((typeof(LoggingBehavior<,>), options.ResolveOrder(typeof(LoggingBehavior<,>))));
        }

        if (options.EnableValidation)
        {
            behaviorRegistrations.Add(
                (typeof(ValidationBehavior<,>), options.ResolveOrder(typeof(ValidationBehavior<,>)))
            );
        }

        if (options.EnableTransaction)
        {
            behaviorRegistrations.Add(
                (typeof(TransactionBehavior<,>), options.ResolveOrder(typeof(TransactionBehavior<,>)))
            );
        }

        foreach (var (behaviorType, _) in behaviorRegistrations.OrderBy(pair => pair.Priority))
        {
            _ = services.AddTransient(typeof(global::Mediator.IPipelineBehavior<,>), behaviorType);
        }

        _ = services.AddSingleton(policyProvider);
        _ = services.AddSingleton<IRequestPipelinePolicyProvider>(policyProvider);

        return services;
    }

    /// <summary>
    /// Registers the NFramework mediator adapter.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddMediatorAdapter(this IServiceCollection services)
    {
        _ = services.AddSingleton<IMediator, MediatorAdapter>();
        return services;
    }
}
