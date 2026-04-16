using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace NFramework.Mediator.Mediator.Performance;

public static class PerformanceServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the NFramework Performance behavior.
        /// </summary>
        public IServiceCollection AddNFrameworkPerformance()
        {
            _ = services.AddSingleton<PerformanceOptions>();
            return services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        }
    }
}
