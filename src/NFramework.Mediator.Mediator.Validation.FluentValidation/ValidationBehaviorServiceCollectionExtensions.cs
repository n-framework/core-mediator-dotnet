using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace NFramework.Mediator.Mediator.Validation.FluentValidation;

public static class ValidationBehaviorServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the NFramework FluentValidation behavior.
        /// </summary>
        public IServiceCollection AddNFrameworkFluentValidation()
        {
            return services.AddTransient(
                typeof(IPipelineBehavior<,>),
                typeof(Validation.FluentValidation.ValidationBehavior<,>)
            );
        }
    }
}
