namespace NFramework.Mediator.Configuration;

/// <summary>
/// Configuration options for mediator pipeline behaviors.
/// </summary>
public sealed class MediatorBehaviorOptions
{
    private readonly Dictionary<Type, int> _customOrder = new();
    private readonly Dictionary<Type, RequestPipelineConfiguration> _requestPipelineConfiguration = new();

    /// <summary>
    /// Gets or sets whether logging behavior is enabled. Default is true.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets whether validation behavior is enabled. Default is true.
    /// </summary>
    public bool EnableValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether transaction behavior is enabled. Default is true.
    /// </summary>
    public bool EnableTransaction { get; set; } = true;

    /// <summary>
    /// Gets or sets whether explicit request configuration is required.
    /// When true, each request type must be explicitly configured via <see cref="ConfigureFor{TRequest}"/>.
    /// Default is true.
    /// </summary>
    public bool RequireExplicitRequestConfiguration { get; set; } = true;

    /// <summary>
    /// Sets the execution order for a behavior type.
    /// </summary>
    /// <typeparam name="TBehavior">The behavior type to configure.</typeparam>
    /// <param name="priority">The priority value (lower values execute first).</param>
    public void SetOrder<TBehavior>(int priority)
    {
        var behaviorType = typeof(TBehavior);
        _customOrder[Normalize(behaviorType)] = priority;
    }

    /// <summary>
    /// Configures pipeline behaviors for a specific request type.
    /// </summary>
    /// <typeparam name="TRequest">The request type to configure.</typeparam>
    /// <param name="configure">Configuration builder action.</param>
    public void ConfigureFor<TRequest>(Action<RequestPipelineConfigurationBuilder> configure)
        where TRequest : global::Mediator.IMessage
    {
        var builder = new RequestPipelineConfigurationBuilder();
        configure(builder);
        _requestPipelineConfiguration[typeof(TRequest)] = builder.Build();
    }

    internal int ResolveOrder(Type behaviorType)
    {
        var normalizedBehaviorType = Normalize(behaviorType);

        if (_customOrder.TryGetValue(normalizedBehaviorType, out int priority))
        {
            return priority;
        }

        if (normalizedBehaviorType == typeof(Behaviors.LoggingBehavior<,>))
        {
            return (int)BehaviorOrder.Logging;
        }

        if (normalizedBehaviorType == typeof(Behaviors.ValidationBehavior<,>))
        {
            return (int)BehaviorOrder.Validation;
        }

        if (normalizedBehaviorType == typeof(Behaviors.TransactionBehavior<,>))
        {
            return (int)BehaviorOrder.Transaction;
        }

        return int.MaxValue;
    }

    private static Type Normalize(Type behaviorType)
    {
        return behaviorType.IsGenericType ? behaviorType.GetGenericTypeDefinition() : behaviorType;
    }

    internal IRequestPipelinePolicyProvider BuildPolicyProvider()
    {
        return new RequestPipelinePolicyProvider(
            new Dictionary<Type, RequestPipelineConfiguration>(_requestPipelineConfiguration),
            RequireExplicitRequestConfiguration
        );
    }
}
