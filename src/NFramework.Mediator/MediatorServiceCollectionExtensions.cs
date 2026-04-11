using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Abstractions.Contracts;
using NFramework.Mediator.Behaviors;
using NFramework.Mediator.Configuration;

namespace NFramework.Mediator;

public static class MediatorServiceCollectionExtensions
{
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

    public static IServiceCollection AddMediatorAdapter(this IServiceCollection services)
    {
        _ = services.AddSingleton<IMediator, MediatorAdapter>();
        return services;
    }
}
