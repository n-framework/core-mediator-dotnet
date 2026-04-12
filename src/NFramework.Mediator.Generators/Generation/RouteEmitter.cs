using System.Text;
using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Generation;

internal static class RouteEmitter
{
    public static string Emit(IReadOnlyList<RouteMappingModel> routes)
    {
        if (routes is null)
        {
            throw new ArgumentNullException(nameof(routes));
        }

        var builder = new StringBuilder();
        _ = builder.AppendLine("using Microsoft.AspNetCore.Builder;");
        _ = builder.AppendLine("using Microsoft.AspNetCore.Http;");
        _ = builder.AppendLine("using System.Threading;");
        _ = builder.AppendLine("using NFramework.Mediator.Abstractions.Contracts;");
        _ = builder.AppendLine();
        _ = builder.AppendLine("namespace NFramework.Mediator.Generated;");
        _ = builder.AppendLine();
        _ = builder.AppendLine("public static class MediatorGeneratedRouteMappings");
        _ = builder.AppendLine("{");
        _ = builder.AppendLine(
            "    public static IEndpointRouteBuilder MapMediatorGeneratedRoutes(this IEndpointRouteBuilder app)"
        );
        _ = builder.AppendLine("    {");

        foreach (RouteMappingModel route in routes.OrderBy(r => r.RouteTemplate, StringComparer.Ordinal))
        {
            if (route.HttpMethod == "POST")
            {
                _ = builder.AppendLine(
                    $"        _ = app.MapPost(\"{route.RouteTemplate}\", async ({route.RequestDisplayName} request, IMediator mediator, CancellationToken ct) =>"
                );
                _ = builder.AppendLine("            await mediator.SendAsync(request, ct));");
            }
            else if (route.HttpMethod == "GET")
            {
                _ = builder.AppendLine(
                    $"        _ = app.MapGet(\"{route.RouteTemplate}\", async ({route.RequestDisplayName} request, IMediator mediator, CancellationToken ct) =>"
                );
                _ = builder.AppendLine("            await mediator.SendAsync(request, ct));");
            }
        }

        _ = builder.AppendLine("        return app;");
        _ = builder.AppendLine("    }");
        _ = builder.AppendLine("}");

        return builder.ToString();
    }
}
