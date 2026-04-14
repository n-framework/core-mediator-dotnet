using Mediator;
using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.MartinothamarMediator.Authorization;
using NFramework.Mediator.MartinothamarMediator.Caching;
using NFramework.Mediator.MartinothamarMediator.Logging;
using NFramework.Mediator.MartinothamarMediator.Performance;
using NFramework.Mediator.MartinothamarMediator.Transactions;
using NFramework.Mediator.MartinothamarMediator.Validation;

namespace NFramework.Mediator.MartinothamarMediator.Configuration;

public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddNFrameworkPipelineBehaviors(
        this IServiceCollection services,
        Action<NFrameworkPipelineOptions>? configure = null
    )
    {
        var pipelineOptions = new NFrameworkPipelineOptions();
        configure?.Invoke(pipelineOptions);

        if (pipelineOptions.EnableLogging)
            _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        if (pipelineOptions.EnableAuthorization)
            _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        if (pipelineOptions.EnableValidation)
            _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        if (pipelineOptions.EnableTransaction)
            _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        if (pipelineOptions.EnablePerformance)
            _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        if (pipelineOptions.EnableCaching)
        {
            _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheRemovingBehavior<,>));
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
