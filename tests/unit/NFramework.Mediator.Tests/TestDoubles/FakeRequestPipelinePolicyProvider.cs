using NFramework.Mediator.Configuration;

namespace NFramework.Mediator.Tests.TestDoubles;

internal sealed class FakeRequestPipelinePolicyProvider : IRequestPipelinePolicyProvider
{
    public RequestPipelineConfiguration GetConfiguration(Type requestType) =>
        new()
        {
            UseLogging = true,
            UseValidation = true,
            UseTransaction = true,
        };
}
