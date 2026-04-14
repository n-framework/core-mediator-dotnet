namespace NFramework.Mediator.Abstractions.Authorization;

/// <summary>
/// Only requests implementing this interface will be checked for authorization by the pipeline.
/// </summary>
public interface ISecuredRequest
{
    string[] RequiredRoles { get; }

    string[] RequiredOperations { get; }
}
