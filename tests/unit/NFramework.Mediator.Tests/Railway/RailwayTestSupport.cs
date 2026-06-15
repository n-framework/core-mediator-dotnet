using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Abstractions.Authorization;
using NFramework.Mediator.Mediator.Railway;
using NFramework.Mediator.Mediator.Transactions;

namespace NFramework.Mediator.Tests.Railway;

/// <summary>
/// Builds a service collection wired for the railway pipeline. Because martinothamar's
/// <c>AddMediator()</c> registers every handler in this assembly into each host, all common
/// handler dependencies (spy, security context, transaction options) must always be present.
/// </summary>
internal static class RailwayTestHost
{
    public static IServiceCollection CreateServices(
        HandlerExecutionSpy spy,
        bool authenticated = true,
        bool hasRole = true
    )
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediator();
        services.AddNFrameworkRailwayBehaviors();
        services.AddSingleton(spy);
        services.AddSingleton<ISecurityContext>(new TestSecurityContext(authenticated, hasRole));
        services.AddSingleton(new MediatorTransactionOptions());
        return services;
    }
}

/// <summary>
/// Records whether a handler executed. Shared across all railway pipeline tests because
/// martinothamar's <c>AddMediator()</c> registers every handler in the assembly into every
/// test host, so all handlers must depend on a single, commonly-registered spy type.
/// </summary>
public sealed class HandlerExecutionSpy
{
    public bool Executed { get; set; }
}

/// <summary>
/// Configurable <see cref="ISecurityContext"/> for authorization tests.
/// </summary>
internal sealed class TestSecurityContext(bool authenticated, bool hasRole) : ISecurityContext
{
    public bool IsAuthenticated { get; } = authenticated;

    public string? UserId { get; } = authenticated ? "user-1" : null;

    public bool HasAnyRole(IReadOnlyList<string> roles) => hasRole;

    public bool HasAllOperations(IReadOnlyList<string> operations) => hasRole;
}
