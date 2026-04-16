using System.Diagnostics.CodeAnalysis;

namespace NFramework.Mediator.Abstractions.Validation;

[SuppressMessage(
    "Design",
    "CA1040:Avoid empty interfaces",
    Justification = "Marker interface for validatable requests."
)]
public interface IValidatableRequest { }
