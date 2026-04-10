using NFramework.Mediator.Abstractions.Contracts;
using NFramework.Mediator.Abstractions.Contracts.Handlers;
using NFramework.Mediator.Abstractions.Contracts.Pipeline;
using NFramework.Mediator.Abstractions.Contracts.Requests;
using NFramework.Mediator.Abstractions.Tests.Discovery;

namespace NFramework.Mediator.Abstractions.Tests.PublicApi;

public sealed class PublicApiContractTests
{
    [Fact]
    public void RequiredMarkerContracts_ArePublicInterfaces()
    {
        typeof(ICommand<int>).IsInterface.ShouldBeTrue();
        typeof(IQuery<int>).IsInterface.ShouldBeTrue();
        typeof(IStreamQuery<int>).IsInterface.ShouldBeTrue();
        typeof(IEvent).IsInterface.ShouldBeTrue();
    }

    [Fact]
    public void RequiredHandlerContracts_ArePublicInterfaces()
    {
        typeof(ICommandHandler<ICommand<int>, int>).IsInterface.ShouldBeTrue();
        typeof(IQueryHandler<IQuery<int>, int>).IsInterface.ShouldBeTrue();
        typeof(IStreamQueryHandler<IStreamQuery<int>, int>).IsInterface.ShouldBeTrue();
        typeof(IEventHandler<IEvent>).IsInterface.ShouldBeTrue();
    }

    [Fact]
    public void AsyncContracts_UseCancellationTokenAndValueTask()
    {
        AssertHandleSignature(typeof(ICommandHandler<,>), typeof(ValueTask<>));
        AssertHandleSignature(typeof(IQueryHandler<,>), typeof(ValueTask<>));
        AssertHandleSignature(typeof(IStreamQueryHandler<,>), typeof(IAsyncEnumerable<>));
        AssertHandleSignature(typeof(IEventHandler<>), typeof(ValueTask));

        var mediatorMethods = typeof(IMediator).GetMethods();
        foreach (var method in mediatorMethods)
        {
            var hasToken = method.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken));
            hasToken.ShouldBeTrue();
        }
    }

    [Fact]
    public void PipelineBehavior_UsesNextDelegate()
    {
        var behaviorType = typeof(IPipelineBehavior<,>);
        ContractShapeInspector.HasHandleMethod(behaviorType).ShouldBeTrue();

        var handleMethod = behaviorType.GetMethod("Handle");
        handleMethod.ShouldNotBeNull();
        ContractShapeInspector.HasCancellationTokenParameter(handleMethod!).ShouldBeTrue();

        var nextParameter = handleMethod!.GetParameters()[1].ParameterType;
        nextParameter.GetGenericTypeDefinition().ShouldBe(typeof(RequestHandlerDelegate<>));
    }

    private static void AssertHandleSignature(Type handlerInterface, Type expectedReturnTypeDefinition)
    {
        var handleMethod = handlerInterface.GetMethod("Handle");
        handleMethod.ShouldNotBeNull();
        ContractShapeInspector.HasCancellationTokenParameter(handleMethod!).ShouldBeTrue();

        if (expectedReturnTypeDefinition.IsGenericTypeDefinition)
        {
            handleMethod!.ReturnType.GetGenericTypeDefinition().ShouldBe(expectedReturnTypeDefinition);
            return;
        }

        handleMethod!.ReturnType.ShouldBe(expectedReturnTypeDefinition);
    }
}