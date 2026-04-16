using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace NFramework.Mediator.Mediator.Logging;

public static class LoggingServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the NFramework Logging behavior.
        /// </summary>
        public IServiceCollection AddNFrameworkLogging()
        {
            return services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }
    }
}
