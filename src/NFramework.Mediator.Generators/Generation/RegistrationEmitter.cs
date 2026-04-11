using System.Text;
using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Generation;

internal static class RegistrationEmitter
{
    public static string Emit(
        IReadOnlyList<HandlerRegistrationModel> commandHandlers,
        IReadOnlyList<HandlerRegistrationModel> queryHandlers,
        IReadOnlyList<HandlerRegistrationModel> eventHandlers
    )
    {
        var all = commandHandlers
            .Concat(queryHandlers)
            .Concat(eventHandlers)
            .OrderBy(model => model.InterfaceDisplayName, StringComparer.Ordinal)
            .ThenBy(model => model.HandlerDisplayName, StringComparer.Ordinal)
            .ToArray();

        var builder = new StringBuilder();
        _ = builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        _ = builder.AppendLine();
        _ = builder.AppendLine("namespace NFramework.Mediator.Generated;");
        _ = builder.AppendLine();
        _ = builder.AppendLine("public static class MediatorGeneratedServiceCollectionExtensions");
        _ = builder.AppendLine("{");
        _ = builder.AppendLine(
            "    public static IServiceCollection AddMediatorGeneratedHandlers(this IServiceCollection services)"
        );
        _ = builder.AppendLine("    {");

        foreach (HandlerRegistrationModel model in all)
        {
            _ = builder.AppendLine(
                $"        _ = services.AddTransient<{model.InterfaceDisplayName}, {model.HandlerDisplayName}>();"
            );
        }

        _ = builder.AppendLine("        return services;");
        _ = builder.AppendLine("    }");
        _ = builder.AppendLine("}");

        return builder.ToString();
    }

    public static string EmitCategoryFile(string className, IReadOnlyList<HandlerRegistrationModel> models)
    {
        var builder = new StringBuilder();
        _ = builder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        _ = builder.AppendLine();
        _ = builder.AppendLine("namespace NFramework.Mediator.Generated;");
        _ = builder.AppendLine();
        _ = builder.AppendLine($"public static class {className}");
        _ = builder.AppendLine("{");
        _ = builder.AppendLine("    public static IServiceCollection Register(IServiceCollection services)");
        _ = builder.AppendLine("    {");

        foreach (
            HandlerRegistrationModel model in models
                .OrderBy(m => m.InterfaceDisplayName, StringComparer.Ordinal)
                .ThenBy(m => m.HandlerDisplayName, StringComparer.Ordinal)
        )
        {
            _ = builder.AppendLine(
                $"        _ = services.AddTransient<{model.InterfaceDisplayName}, {model.HandlerDisplayName}>();"
            );
        }

        _ = builder.AppendLine("        return services;");
        _ = builder.AppendLine("    }");
        _ = builder.AppendLine("}");

        return builder.ToString();
    }
}
