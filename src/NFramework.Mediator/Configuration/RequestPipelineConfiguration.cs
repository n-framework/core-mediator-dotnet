namespace NFramework.Mediator.Configuration;

public sealed class RequestPipelineConfiguration
{
    public bool UseLogging { get; set; }

    public bool UseValidation { get; set; }

    public bool UseTransaction { get; set; }
}

public sealed class RequestPipelineConfigurationBuilder
{
    private readonly RequestPipelineConfiguration _configuration = new();

    internal RequestPipelineConfiguration Build() => _configuration;

    public RequestPipelineConfigurationBuilder UseLogging()
    {
        _configuration.UseLogging = true;
        return this;
    }

    public RequestPipelineConfigurationBuilder UseValidation()
    {
        _configuration.UseValidation = true;
        return this;
    }

    public RequestPipelineConfigurationBuilder UseTransaction()
    {
        _configuration.UseTransaction = true;
        return this;
    }
}

public interface IRequestPipelinePolicyProvider
{
    RequestPipelineConfiguration GetConfiguration(Type requestType);
}

internal sealed class RequestPipelinePolicyProvider : IRequestPipelinePolicyProvider
{
    private readonly IReadOnlyDictionary<Type, RequestPipelineConfiguration> _configurationByRequestType;
    private readonly bool _requireExplicitConfiguration;

    public RequestPipelinePolicyProvider(
        IReadOnlyDictionary<Type, RequestPipelineConfiguration> configurationByRequestType,
        bool requireExplicitConfiguration
    )
    {
        _configurationByRequestType = configurationByRequestType;
        _requireExplicitConfiguration = requireExplicitConfiguration;
    }

    public RequestPipelineConfiguration GetConfiguration(Type requestType)
    {
        if (_configurationByRequestType.TryGetValue(requestType, out var configuration))
        {
            return configuration;
        }

        if (_requireExplicitConfiguration)
        {
            throw new InvalidOperationException(
                $"No mediator pipeline configuration was registered for request type '{requestType.FullName}'. Configure it using MediatorBehaviorOptions.ConfigureFor<TRequest>(...)."
            );
        }

        return new RequestPipelineConfiguration();
    }
}
