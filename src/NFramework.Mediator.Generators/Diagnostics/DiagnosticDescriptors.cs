using Microsoft.CodeAnalysis;

namespace NFramework.Mediator.Generators.Diagnostics;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MultiInterfaceHandler = new(
        id: "NFMED001",
        title: "Handler implements multiple handler categories",
        messageFormat: "Handler '{0}' implements multiple handler categories; split handlers by concern",
        category: "NFramework.Mediator.Generators",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A single handler should implement command, query, or event contracts separately."
    );

    public static readonly DiagnosticDescriptor UnresolvedGenericHandler = new(
        id: "NFMED002",
        title: "Unresolved generic handler",
        messageFormat: "Handler '{0}' uses unresolved generic type arguments; use closed generic handlers",
        category: "NFramework.Mediator.Generators",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Source generation requires closed generic handler types."
    );

    public static readonly DiagnosticDescriptor UnsupportedReturnType = new(
        id: "NFMED003",
        title: "Unsupported handler return type",
        messageFormat: "Handler '{0}' has unsupported return type '{1}'; use ValueTask<TResult> or ValueTask",
        category: "NFramework.Mediator.Generators",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Handler methods must use supported async return patterns."
    );

    public static readonly DiagnosticDescriptor DuplicateRegistration = new(
        id: "NFMED004",
        title: "Duplicate handler registration",
        messageFormat: "Duplicate registration detected for '{0}'",
        category: "NFramework.Mediator.Generators",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Multiple handlers were discovered for the same service/implementation pair."
    );
}

internal sealed class DiagnosticEnvelope
{
    public DiagnosticEnvelope(DiagnosticDescriptor descriptor, Location? location, params object[] messageArguments)
    {
        Descriptor = descriptor;
        Location = location;
        MessageArguments = messageArguments;
    }

    public DiagnosticDescriptor Descriptor { get; }
    public Location? Location { get; }
    public object[] MessageArguments { get; }
}
