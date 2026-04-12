using NFramework.Mediator.Generators.Diagnostics;
using NFramework.Mediator.Generators.Discovery.Models;

namespace NFramework.Mediator.Generators.Discovery;

/// <summary>
/// Contains the results of extracting handler information from a type,
/// including discovered models and any diagnostics generated during extraction.
/// </summary>
internal sealed class ExtractionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtractionResult"/> class.
    /// </summary>
    /// <param name="models">Handler models discovered during extraction</param>
    /// <param name="diagnostics">Diagnostics generated during extraction</param>
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
