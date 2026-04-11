namespace NFramework.Mediator.Configuration;

/// <summary>
/// Configuration for a specific request type's pipeline behaviors.
/// </summary>
public sealed class RequestPipelineConfiguration
{
    /// <summary>
    /// Gets or sets whether logging behavior is enabled for this request type.
    /// </summary>
    public bool UseLogging { get; init; }

    /// <summary>
    /// Gets or sets whether validation behavior is enabled for this request type.
    /// </summary>
    public bool UseValidation { get; init; }

    /// <summary>
    /// Gets or sets whether transaction behavior is enabled for this request type.
    /// </summary>
    public bool UseTransaction { get; init; }
}

/// <summary>
/// Builder for configuring <see cref="RequestPipelineConfiguration"/>.
/// </summary>
public sealed class RequestPipelineConfigurationBuilder
{
    private bool _useLogging;
    private bool _useValidation;
    private bool _useTransaction;

    internal RequestPipelineConfiguration Build() =>
        new()
        {
            UseLogging = _useLogging,
            UseValidation = _useValidation,
            UseTransaction = _useTransaction,
        };

    /// <summary>
    /// Enables logging behavior for the request pipeline.
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    public RequestPipelineConfigurationBuilder UseLogging()
    {
        _useLogging = true;
        return this;
    }

    /// <summary>
    /// Enables validation behavior for the request pipeline.
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    public RequestPipelineConfigurationBuilder UseValidation()
    {
        _useValidation = true;
        return this;
    }

    /// <summary>
    /// Enables transaction behavior for the request pipeline.
    /// </summary>
    /// <returns>The builder for method chaining.</returns>
    public RequestPipelineConfigurationBuilder UseTransaction()
    {
        _useTransaction = true;
        return this;
    }
}

/// <summary>
/// Provides pipeline configuration for request types.
/// </summary>
public interface IRequestPipelinePolicyProvider
{
    /// <summary>
    /// Gets the pipeline configuration for the specified request type.
    /// </summary>
    /// <param name="requestType">The request type to get configuration for.</param>
    /// <returns>The pipeline configuration.</returns>
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
        ArgumentNullException.ThrowIfNull(configurationByRequestType);
        _configurationByRequestType = configurationByRequestType;
        _requireExplicitConfiguration = requireExplicitConfiguration;
    }

    public RequestPipelineConfiguration GetConfiguration(Type requestType)
    {
        ArgumentNullException.ThrowIfNull(requestType);

        if (_configurationByRequestType.TryGetValue(requestType, out var configuration))
        {
            return configuration;
        }

        if (_requireExplicitConfiguration)
        {
            throw new InvalidOperationException(
                $"No mediator pipeline configuration was registered for request type '{requestType.FullName}'.\n"
                    + $"To fix, configure the request type using:\n"
                    + $"  services.AddMediator(options =>\n"
                    + $"  {{\n"
                    + $"      options.BehaviorOptions.ConfigureFor<{requestType.Name}>(builder =>\n"
                    + $"      {{\n"
                    + $"          builder.UseLogging();\n"
                    + $"          builder.UseValidation();\n"
                    + $"          builder.UseTransaction();\n"
                    + $"      }});\n"
                    + $"  }});\n"
                    + $"Or disable explicit configuration with:\n"
                    + $"  options.BehaviorOptions.RequireExplicitRequestConfiguration = false;"
            );
        }

        return new RequestPipelineConfiguration
        {
            UseLogging = true,
            UseValidation = true,
            UseTransaction = true,
        };
    }
}
