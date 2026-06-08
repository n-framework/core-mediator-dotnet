using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using UnionRailway.AspNetCore;

namespace NFramework.Mediator.Mediator.Railway.UnionRailway;

/// <summary>
/// Registration helpers that wire UnionRailway into an NFramework application.
/// </summary>
public static class RailwayMediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers UnionRailway services together with the NFramework default error mapper.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional callback to configure <see cref="RailwayOptions"/>.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddNFrameworkRailway(
        this IServiceCollection services,
        Action<RailwayOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddRailway<NFrameworkUnionErrorMapper>(configure);
    }

    /// <summary>
    /// Adds the middleware that converts NFramework framework exceptions into RFC 7807 problem responses.
    /// Register this early in the pipeline, before endpoint execution.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder for chaining.</returns>
    public static IApplicationBuilder UseNFrameworkRailway(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<RailwayExceptionTranslationMiddleware>();
    }
}
