using Mediator;
using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.MartinothamarMediator.Behaviors;

namespace NFramework.Mediator.MartinothamarMediator.Configuration;

public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddNFrameworkPipelineBehaviors(
        this IServiceCollection services,
        Action<NFrameworkPipelineOptions>? configure = null)
    {
        var pipelineOptions = new NFrameworkPipelineOptions();
        configure?.Invoke(pipelineOptions);

        if (pipelineOptions.EnableLogging)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        if (pipelineOptions.EnableAuthorization)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        if (pipelineOptions.EnableValidation)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        if (pipelineOptions.EnableTransaction)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        if (pipelineOptions.EnablePerformance)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        if (pipelineOptions.EnableCaching)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheRemovingBehavior<,>));
        }

        return services;
    }
}

public class NFrameworkPipelineOptions
{
    public bool EnableLogging { get; set; } = true;
    public bool EnableAuthorization { get; set; } = true;
    public bool EnableValidation { get; set; } = true;
    public bool EnableTransaction { get; set; } = true;
    public bool EnableCaching { get; set; } = true;
    public bool EnablePerformance { get; set; } = true;

    public TimeSpan TransactionScopeTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
