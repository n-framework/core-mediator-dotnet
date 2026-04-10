using NFramework.Mediator.Abstractions.Contracts.Handlers;

namespace NFramework.Mediator.Abstractions.Tests.Discovery;

internal enum HandlerKind
{
    Command,
    Query,
    Stream,
    Event,
}

internal sealed record HandlerDiscoveryResult(
    Type HandlerType,
    bool IsDiscoverable,
    HandlerKind? Kind,
    Type? RequestType,
    string? FailureReason
);

internal static class HandlerDiscoverabilityClassifier
{
    public static IReadOnlyList<HandlerDiscoveryResult> ClassifyAll(IEnumerable<Type> handlerTypes)
    {
        var initial = handlerTypes.Select(Classify).ToList();

        var duplicateInvalidations = initial
            .Where(result =>
                result.IsDiscoverable && result.Kind is HandlerKind.Command or HandlerKind.Query or HandlerKind.Stream
            )
            .GroupBy(result => new { result.Kind, result.RequestType })
            .Where(group => group.Count() > 1)
            .SelectMany(group => group)
            .ToHashSet();

        return initial
            .Select(result =>
                duplicateInvalidations.Contains(result)
                    ? result with
                    {
                        IsDiscoverable = false,
                        FailureReason = "Duplicate handler declaration",
                    }
                    : result
            )
            .ToList();
    }

    public static HandlerDiscoveryResult Classify(Type handlerType)
    {
        if (handlerType.IsGenericTypeDefinition)
        {
            return new HandlerDiscoveryResult(
                handlerType,
                false,
                null,
                null,
                "Open generic handler types are not discoverable"
            );
        }

        var interfaces = handlerType.GetInterfaces().Where(interfaceType => interfaceType.IsGenericType).ToList();

        foreach (var interfaceType in interfaces)
        {
            var definition = interfaceType.GetGenericTypeDefinition();
            var args = interfaceType.GetGenericArguments();

            if (args.Any(arg => arg.IsGenericParameter))
            {
                return new HandlerDiscoveryResult(handlerType, false, null, null, "Generic parameters must be closed");
            }

            if (definition == typeof(ICommandHandler<,>))
            {
                return Discoverable(handlerType, HandlerKind.Command, args[0]);
            }

            if (definition == typeof(IQueryHandler<,>))
            {
                return Discoverable(handlerType, HandlerKind.Query, args[0]);
            }

            if (definition == typeof(IStreamQueryHandler<,>))
            {
                return Discoverable(handlerType, HandlerKind.Stream, args[0]);
            }

            if (definition == typeof(IEventHandler<>))
            {
                return Discoverable(handlerType, HandlerKind.Event, args[0]);
            }
        }

        return new HandlerDiscoveryResult(
            handlerType,
            false,
            null,
            null,
            "Type does not implement a supported handler contract"
        );
    }

    private static HandlerDiscoveryResult Discoverable(Type handlerType, HandlerKind kind, Type requestType)
    {
        return new HandlerDiscoveryResult(handlerType, true, kind, requestType, null);
    }
}
