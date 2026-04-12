using Microsoft.CodeAnalysis;
using NFramework.Mediator.Generators.Diagnostics;
using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Discovery;

/// <summary>
/// Extracts handler registration models from type symbols by analyzing implemented interfaces.
/// </summary>
internal static class HandlerTypeExtractor
{
    private const string HandlersNamespace = "NFramework.Mediator.Abstractions.Contracts.Handlers";
    private const string CommandHandlerMetadataName = "ICommandHandler`2";
    private const string QueryHandlerMetadataName = "IQueryHandler`2";
    private const string EventHandlerMetadataName = "IEventHandler`1";

    /// <summary>
    /// Extracts handler models from a type symbol by examining its implemented handler interfaces.
    /// </summary>
    /// <param name="handlerType">The handler type to analyze</param>
    /// <returns>An extraction result containing discovered models and any diagnostics</returns>
    public static ExtractionResult Extract(INamedTypeSymbol handlerType)
    {
        var models = new List<HandlerRegistrationModel>();
        var diagnostics = new List<DiagnosticEnvelope>();

        foreach (INamedTypeSymbol implementedInterface in handlerType.AllInterfaces)
        {
            INamedTypeSymbol interfaceDefinition = implementedInterface.OriginalDefinition;
            string metadataName = interfaceDefinition.MetadataName;
            string containingNamespace = interfaceDefinition.ContainingNamespace.ToDisplayString();

            string? category = ResolveCategory(metadataName, containingNamespace);
            if (category is null)
            {
                continue;
            }

            if (IsUnresolvedGeneric(implementedInterface))
            {
                diagnostics.Add(
                    new DiagnosticEnvelope(
                        DiagnosticDescriptors.UnresolvedGenericHandler,
                        handlerType.Locations.FirstOrDefault(),
                        handlerType.ToDisplayString()
                    )
                );
                continue;
            }

            bool isApiExposed = HasApiExposedAttribute(handlerType);

            models.Add(BuildRegistrationModel(handlerType, implementedInterface, category, isApiExposed));
        }

        if (HasMultipleCategories(models))
        {
            diagnostics.Add(
                new DiagnosticEnvelope(
                    DiagnosticDescriptors.MultiInterfaceHandler,
                    handlerType.Locations.FirstOrDefault(),
                    handlerType.ToDisplayString()
                )
            );
        }

        return new ExtractionResult(models, diagnostics);
    }

    private static string? ResolveCategory(string metadataName, string containingNamespace)
    {
        if (containingNamespace != HandlersNamespace)
        {
            return null;
        }

        if (metadataName == CommandHandlerMetadataName)
        {
            return "command";
        }

        if (metadataName == QueryHandlerMetadataName)
        {
            return "query";
        }

        if (metadataName == EventHandlerMetadataName)
        {
            return "event";
        }

        return null;
    }

    private static bool IsUnresolvedGeneric(INamedTypeSymbol implementedInterface)
    {
        return implementedInterface.IsUnboundGenericType
            || implementedInterface.TypeArguments.Any(t => t is ITypeParameterSymbol);
    }

    private static HandlerRegistrationModel BuildRegistrationModel(
        INamedTypeSymbol handlerType,
        INamedTypeSymbol implementedInterface,
        string category,
        bool isApiExposed
    )
    {
        string requestType = implementedInterface
            .TypeArguments[0]
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        string? responseType =
            category != "event"
                ? implementedInterface.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                : null;

        string? httpMethod = category switch
        {
            "command" => "POST",
            "query" => "GET",
            _ => null,
        };

        string? routeTemplate = isApiExposed
            ? RouteTemplateBuilder.BuildRouteTemplate(implementedInterface.TypeArguments[0].Name)
            : null;

        return new HandlerRegistrationModel(
            implementedInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            handlerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            requestType,
            responseType,
            category,
            isApiExposed,
            routeTemplate,
            httpMethod,
            handlerType.Locations.FirstOrDefault()
        );
    }

    private static bool HasMultipleCategories(IReadOnlyList<HandlerRegistrationModel> models)
    {
        if (models.Count < 2)
        {
            return false;
        }

        int distinctCategories = models.Select(model => model.HandlerCategory).Distinct(StringComparer.Ordinal).Count();

        return distinctCategories > 1;
    }

    private static bool HasApiExposedAttribute(INamedTypeSymbol handlerType)
    {
        foreach (AttributeData attribute in handlerType.GetAttributes())
        {
            string attributeName = attribute.AttributeClass?.Name ?? string.Empty;
            if (
                attributeName.Equals("ApiExposedAttribute", StringComparison.Ordinal)
                || attributeName.Equals("ApiExposed", StringComparison.Ordinal)
            )
            {
                return true;
            }
        }

        return false;
    }
}
