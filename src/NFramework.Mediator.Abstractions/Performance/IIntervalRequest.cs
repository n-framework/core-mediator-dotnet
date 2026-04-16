using System.Diagnostics.CodeAnalysis;

namespace NFramework.Mediator.Abstractions.Performance;

[SuppressMessage(
    "Design",
    "CA1040:Avoid empty interfaces",
    Justification = "Marker interface for interval-based requests."
)]
public interface IIntervalRequest { }
