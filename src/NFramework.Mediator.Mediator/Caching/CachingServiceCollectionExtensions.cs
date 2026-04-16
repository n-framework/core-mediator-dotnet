using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace NFramework.Mediator.Mediator.Caching;

public static class CachingServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the NFramework Caching behaviors.
        /// </summary>
        public IServiceCollection AddNFrameworkCaching()
        {
            _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            _ = services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheRemovingBehavior<,>));
            return services;
        }
    }
}
