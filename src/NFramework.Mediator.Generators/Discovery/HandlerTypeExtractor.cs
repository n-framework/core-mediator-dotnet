using Microsoft.CodeAnalysis;
using NFramework.Mediator.Generators.Diagnostics;
using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Discovery;

internal static class HandlerTypeExtractor
{
    private const string HandlersNamespace = "NFramework.Mediator.Abstractions.Contracts.Handlers";
    private const string CommandHandlerMetadataName = "ICommandHandler`2";
    private const string QueryHandlerMetadataName = "IQueryHandler`2";
    private const string EventHandlerMetadataName = "IEventHandler`1";

    public static ExtractionResult Extract(INamedTypeSymbol handlerType)
    {
        var models = new List<HandlerRegistrationModel>();
        var diagnostics = new List<DiagnosticEnvelope>();

        foreach (INamedTypeSymbol implementedInterface in handlerType.AllInterfaces)
        {
            INamedTypeSymbol interfaceDefinition = implementedInterface.OriginalDefinition;
            string metadataName = interfaceDefinition.MetadataName;
            string containingNamespace = interfaceDefinition.ContainingNamespace.ToDisplayString();

            bool isCommandHandler =
                metadataName == CommandHandlerMetadataName && containingNamespace == HandlersNamespace;
            bool isQueryHandler = metadataName == QueryHandlerMetadataName && containingNamespace == HandlersNamespace;
            bool isEventHandler = metadataName == EventHandlerMetadataName && containingNamespace == HandlersNamespace;

            if (!isCommandHandler && !isQueryHandler && !isEventHandler)
            {
                continue;
            }

            if (
                implementedInterface.IsUnboundGenericType
                || implementedInterface.TypeArguments.Any(t => t is ITypeParameterSymbol)
            )
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
            string category;
            string requestType;
            string? responseType = null;
            string? httpMethod = null;

            if (isEventHandler)
            {
                category = "event";
                requestType = implementedInterface
                    .TypeArguments[0]
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            }
            else
            {
                requestType = implementedInterface
                    .TypeArguments[0]
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                responseType = implementedInterface
                    .TypeArguments[1]
                    .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                category = isCommandHandler ? "command" : "query";
                httpMethod = isCommandHandler ? "POST" : "GET";
            }

            models.Add(
                new HandlerRegistrationModel(
                    implementedInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    handlerType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    requestType,
                    responseType,
                    category,
                    isApiExposed,
                    isApiExposed ? BuildRouteTemplate(implementedInterface.TypeArguments[0].Name) : null,
                    httpMethod,
                    handlerType.Locations.FirstOrDefault()
                )
            );
        }

        int distinctCategories = models.Select(model => model.HandlerCategory).Distinct(StringComparer.Ordinal).Count();
        if (distinctCategories > 1)
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

    private static string BuildRouteTemplate(string requestTypeName)
    {
        string routeName = requestTypeName;
        if (routeName.EndsWith("Command", StringComparison.Ordinal))
        {
            routeName = routeName.Substring(0, routeName.Length - "Command".Length);
        }
        else if (routeName.EndsWith("Query", StringComparison.Ordinal))
        {
            routeName = routeName.Substring(0, routeName.Length - "Query".Length);
        }

        string kebab = string.Concat(
            routeName.Select(
                (character, index) =>
                    index > 0 && char.IsUpper(character)
                        ? $"-{char.ToLowerInvariant(character)}"
                        : char.ToLowerInvariant(character).ToString()
            )
        );

        if (!kebab.EndsWith("s", StringComparison.Ordinal))
        {
            kebab += "s";
        }

        return $"/api/{kebab}";
    }
}

internal sealed class ExtractionResult
{
    public ExtractionResult(
        IReadOnlyList<HandlerRegistrationModel> models,
        IReadOnlyList<DiagnosticEnvelope> diagnostics
    )
    {
        Models = models;
        Diagnostics = diagnostics;
    }

    public IReadOnlyList<HandlerRegistrationModel> Models { get; }
    public IReadOnlyList<DiagnosticEnvelope> Diagnostics { get; }
}
