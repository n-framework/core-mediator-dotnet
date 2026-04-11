namespace NFramework.Mediator.Configuration;

public sealed class MediatorBehaviorOptions
{
    private readonly Dictionary<Type, int> _customOrder = new();
    private readonly Dictionary<Type, RequestPipelineConfiguration> _requestPipelineConfiguration = new();

    public bool EnableLogging { get; set; } = true;

    public bool EnableValidation { get; set; } = true;

    public bool EnableTransaction { get; set; } = true;

    public bool RequireExplicitRequestConfiguration { get; set; } = true;

    public void SetOrder<TBehavior>(int priority)
    {
        var behaviorType = typeof(TBehavior);
        _customOrder[Normalize(behaviorType)] = priority;
    }

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
