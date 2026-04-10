namespace NFramework.Mediator.Abstractions.Tests.Discovery;

public sealed record DiscoveryExpectationCase(
    string CaseName,
    Type ContractShape,
    bool ExpectedDiscoverable,
    string? FailureReason = null
);
