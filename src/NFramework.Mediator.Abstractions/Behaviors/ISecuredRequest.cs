namespace NFramework.Mediator.Abstractions.Behaviors;

/// <summary>
/// Marker interface for the authorization behavior.
/// Only requests implementing this interface will be checked for authorization.
/// </summary>
public interface ISecuredRequest
{
    /// <summary>
    /// Required roles array.
    /// </summary>
    string[] RequiredRoles { get; }

    /// <summary>
    /// Required operation claims array.
    /// </summary>
    string[] RequiredOperations { get; }
}
