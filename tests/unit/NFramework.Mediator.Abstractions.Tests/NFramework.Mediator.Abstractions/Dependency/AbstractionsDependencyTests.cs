namespace NFramework.Mediator.Abstractions.Tests.Dependency;

public sealed class AbstractionsDependencyTests
{
    [Fact]
    public void AbstractionsAssembly_DoesNotReferenceInfrastructureAssemblies()
    {
        var assembly = typeof(NFramework.Mediator.Abstractions.Contracts.IMediator).Assembly;
        var references = assembly.GetReferencedAssemblies();

        foreach (var reference in references)
        {
            var isNFramework = reference.Name != null && reference.Name.StartsWith("NFramework");
            var isNotAbstractions = reference.Name != "NFramework.Mediator.Abstractions";
            if (isNFramework && isNotAbstractions)
            {
                throw new Exception($"Abstractions should not reference other NFramework assemblies, but references: {reference.Name}");
            }
        }
    }
}
