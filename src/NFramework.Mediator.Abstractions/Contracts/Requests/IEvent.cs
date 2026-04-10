namespace NFramework.Mediator.Abstractions.Contracts.Requests;

/// <summary>
/// Marker interface for domain events.
/// Events represent something that has occurred in the domain and can be handled by multiple handlers (fan-out).
/// </summary>
public interface IEvent;
