namespace NFramework.Mediator.Abstractions.Behaviors;

/// <summary>
/// Marker interface for the request transaction behavior.
/// Only requests implementing this interface will run within a transaction scope.
/// </summary>
public interface ITransactionalRequest { }
