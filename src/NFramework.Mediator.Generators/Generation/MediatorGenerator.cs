using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NFramework.Mediator.Generators.Diagnostics;
using NFramework.Mediator.Generators.Discovery;
using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Generation;

/// <summary>
/// Source generator that produces DI registration code and route mappings for Mediator handlers.
/// </summary>
/// <remarks>
/// This generator discovers implementations of ICommandHandler&lt;T,TResult&gt;, IQueryHandler&lt;T,TResult&gt;,
/// and IEventHandler&lt;T&gt; interfaces, and generates:
/// - Extension methods for registering handlers with DI
/// - HTTP route mappings for handlers marked with [ApiExposed]
/// </remarks>
[Generator]
public sealed class MediatorGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the incremental generator pipeline for discovering handlers and emitting source code.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Attribute-based path kept to satisfy high-performance Roslyn filtering for API-exposed handlers.
        IncrementalValuesProvider<INamedTypeSymbol> apiExposedTypes =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: "ApiExposedAttribute",
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (syntaxContext, _) => (INamedTypeSymbol)syntaxContext.TargetSymbol
            );

        IncrementalValuesProvider<ExtractionResult> handlerCandidates = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (syntaxContext, _) => ExtractFromContext(syntaxContext)
            )
            .Where(static result => result.Models.Count > 0 || result.Diagnostics.Count > 0);

        IncrementalValueProvider<ImmutableArray<ExtractionResult>> collected = handlerCandidates.Collect();
        IncrementalValueProvider<ImmutableArray<INamedTypeSymbol>> apiExposedCollected = apiExposedTypes.Collect();

        context.RegisterSourceOutput(
            collected.Combine(apiExposedCollected),
            static (productionContext, input) =>
            {
                ImmutableArray<ExtractionResult> extractionResults = input.Left;
                ImmutableArray<INamedTypeSymbol> apiExposed = input.Right;
                var apiExposedSet = new HashSet<string>(StringComparer.Ordinal);
                foreach (INamedTypeSymbol symbol in apiExposed)
                {
                    _ = apiExposedSet.Add(symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }

                var allModels = extractionResults.SelectMany(result => result.Models).ToArray();
                foreach (DiagnosticEnvelope diagnostic in extractionResults.SelectMany(result => result.Diagnostics))
                {
                    productionContext.ReportDiagnostic(
                        Diagnostic.Create(diagnostic.Descriptor, diagnostic.Location, diagnostic.MessageArguments)
                    );
                }

                var normalizedModels = allModels
                    .Select(model =>
                        apiExposedSet.Contains(model.HandlerDisplayName) ? model.WithApiExposed(true) : model
                    )
                    .ToArray();

                var commandHandlers = HandlerDiscovery.SelectByCategory(normalizedModels, "command");
                var queryHandlers = HandlerDiscovery.SelectByCategory(normalizedModels, "query");
                var eventHandlers = HandlerDiscovery.SelectByCategory(normalizedModels, "event");

                var duplicateKeys = commandHandlers
                    .Concat(queryHandlers)
                    .Concat(eventHandlers)
                    .GroupBy(model => model.InterfaceDisplayName, StringComparer.Ordinal)
                    .Where(group => group.Count() > 1);

                foreach (IGrouping<string, HandlerRegistrationModel> duplicate in duplicateKeys)
                {
                    productionContext.ReportDiagnostic(
                        Diagnostic.Create(DiagnosticDescriptors.DuplicateRegistration, Location.None, duplicate.Key)
                    );
                }

                productionContext.AddSource(
                    "MediatorExtensions.g.cs",
                    RegistrationEmitter.Emit(commandHandlers, queryHandlers, eventHandlers)
                );
                productionContext.AddSource(
                    "CommandRegistrations.g.cs",
                    RegistrationEmitter.EmitCategoryFile("MediatorGeneratedCommandRegistrations", commandHandlers)
                );
                productionContext.AddSource(
                    "QueryRegistrations.g.cs",
                    RegistrationEmitter.EmitCategoryFile("MediatorGeneratedQueryRegistrations", queryHandlers)
                );
                productionContext.AddSource(
                    "EventRegistrations.g.cs",
                    RegistrationEmitter.EmitCategoryFile("MediatorGeneratedEventRegistrations", eventHandlers)
                );

                var routes = commandHandlers
                    .Concat(queryHandlers)
                    .Where(model =>
                        model.IsApiExposed && model.RouteTemplate is not null && model.HttpMethod is not null
                    )
                    .Select(model => new RouteMappingModel(
                        model.HttpMethod!,
                        model.RouteTemplate!,
                        model.RequestDisplayName,
                        model.ResponseDisplayName,
                        model.HandlerCategory
                    ))
                    .OrderBy(model => model.RouteTemplate, StringComparer.Ordinal)
                    .ToArray();

                productionContext.AddSource("RouteMappings.g.cs", RouteEmitter.Emit(routes));
            }
        );
    }

    /// <summary>
    /// Extracts handler information from a class declaration, validating return types.
    /// </summary>
    private static ExtractionResult ExtractFromContext(GeneratorSyntaxContext syntaxContext)
    {
        if (syntaxContext.Node is not ClassDeclarationSyntax)
        {
            return new ExtractionResult(Array.Empty<HandlerRegistrationModel>(), Array.Empty<DiagnosticEnvelope>());
        }

        if (syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node) is not INamedTypeSymbol typeSymbol)
        {
            return new ExtractionResult(Array.Empty<HandlerRegistrationModel>(), Array.Empty<DiagnosticEnvelope>());
        }

        ExtractionResult result = HandlerTypeExtractor.Extract(typeSymbol);
        var diagnostics = new List<DiagnosticEnvelope>(result.Diagnostics);

        foreach (HandlerRegistrationModel model in result.Models)
        {
            if (model.HandlerCategory == "event")
            {
                continue;
            }

            bool hasSupportedReturnType = HasSupportedHandleReturnType(typeSymbol);
            if (!hasSupportedReturnType)
            {
                diagnostics.Add(
                    new DiagnosticEnvelope(
                        DiagnosticDescriptors.UnsupportedReturnType,
                        model.Location,
                        typeSymbol.ToDisplayString(),
                        "unsupported"
                    )
                );
            }
        }

        return new ExtractionResult(result.Models, diagnostics);
    }

    /// <summary>
    /// Checks if the type has a Handle method returning ValueTask or ValueTask&lt;TResult&gt;.
    /// </summary>
    private static bool HasSupportedHandleReturnType(INamedTypeSymbol typeSymbol)
    {
        foreach (IMethodSymbol method in typeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.Name != "Handle")
            {
                continue;
            }

            string returnDisplay = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (returnDisplay.StartsWith("global::System.Threading.Tasks.ValueTask", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
