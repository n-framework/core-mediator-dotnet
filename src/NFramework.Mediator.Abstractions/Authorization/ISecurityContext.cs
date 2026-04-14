namespace NFramework.Mediator.Abstractions.Authorization;

/// <summary>
/// Allows checking user roles and operation claims without depending on runtime UI frameworks (e.g. ASP.NET).
/// Implementations handle the mapping between the environment's identity system and these pure abstractions.
/// </summary>
public interface ISecurityContext
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    bool HasAnyRole(IReadOnlyList<string> roles);

    bool HasAllOperations(IReadOnlyList<string> operations);
}
