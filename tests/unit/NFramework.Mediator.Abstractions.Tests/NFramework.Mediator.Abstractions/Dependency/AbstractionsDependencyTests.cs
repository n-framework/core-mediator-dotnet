namespace NFramework.Mediator.Abstractions.Tests.Dependency;

public sealed class AbstractionsDependencyTests
{
    [Fact]
    public void AbstractionsAssembly_DoesNotReferenceInfrastructureAssemblies()
    {
        var assembly = typeof(NFramework.Mediator.Abstractions.Contracts.IMediator).Assembly;

        var references = assembly.GetReferencedAssemblies();
        Assert.DoesNotContain(
            references,
            reference =>
                reference.Name is not null
                && reference.Name.StartsWith("NFramework", StringComparison.Ordinal)
                && !string.Equals(reference.Name, "NFramework.Mediator.Abstractions", StringComparison.Ordinal)
        );
    }
}
