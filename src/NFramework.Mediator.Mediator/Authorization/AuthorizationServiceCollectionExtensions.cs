using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace NFramework.Mediator.Mediator.Authorization;

public static class AuthorizationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the NFramework Authorization behavior.
        /// </summary>
        public IServiceCollection AddNFrameworkAuthorization()
        {
            return services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        }
    }
}
