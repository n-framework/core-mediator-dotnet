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
            bool hasToken = method.GetParameters().Any(p => p.ParameterType == typeof(CancellationToken));
            hasToken.ShouldBeTrue();
        }
    }

    [Fact]
    public void InterfaceNames_StartWithIPrefix()
    {
        typeof(ICommand<>).IsInterface.ShouldBeTrue();
        typeof(ICommand<>).Name.ShouldStartWith("I");

        typeof(IQuery<>).IsInterface.ShouldBeTrue();
        typeof(IQuery<>).Name.ShouldStartWith("I");

        typeof(IStreamQuery<>).IsInterface.ShouldBeTrue();
        typeof(IStreamQuery<>).Name.ShouldStartWith("I");

        typeof(IEvent).IsInterface.ShouldBeTrue();
        typeof(IEvent).Name.ShouldStartWith("I");

        typeof(ICommandHandler<,>).IsInterface.ShouldBeTrue();
        typeof(ICommandHandler<,>).Name.ShouldStartWith("I");

        typeof(IQueryHandler<,>).IsInterface.ShouldBeTrue();
        typeof(IQueryHandler<,>).Name.ShouldStartWith("I");

        typeof(IStreamQueryHandler<,>).IsInterface.ShouldBeTrue();
        typeof(IStreamQueryHandler<,>).Name.ShouldStartWith("I");

        typeof(IEventHandler<>).IsInterface.ShouldBeTrue();
        typeof(IEventHandler<>).Name.ShouldStartWith("I");

        typeof(IMediator).IsInterface.ShouldBeTrue();
        typeof(IMediator).Name.ShouldStartWith("I");

        typeof(IPipelineBehavior<,>).IsInterface.ShouldBeTrue();
        typeof(IPipelineBehavior<,>).Name.ShouldStartWith("I");
    }

    [Fact]
    public void PipelineBehavior_UsesNextDelegate()
    {
        var behaviorType = typeof(IPipelineBehavior<,>);
        ContractShapeInspector.HasHandleMethod(behaviorType).ShouldBeTrue();

        var handleMethod = behaviorType.GetMethod("Handle");
        _ = handleMethod.ShouldNotBeNull();
        ContractShapeInspector.HasCancellationTokenParameter(handleMethod!).ShouldBeTrue();

        var parameters = handleMethod!.GetParameters();
        (parameters.Length >= 2).ShouldBeTrue();
        var nextParameter = parameters[1].ParameterType;
        nextParameter.IsGenericType.ShouldBeTrue();
        nextParameter.GetGenericTypeDefinition().ShouldBe(typeof(RequestHandlerDelegate<>));
    }

    private static void AssertHandleSignature(Type handlerInterface, Type expectedReturnTypeDefinition)
    {
        var handleMethod = handlerInterface.GetMethod("Handle");
        _ = handleMethod.ShouldNotBeNull();
        ContractShapeInspector.HasCancellationTokenParameter(handleMethod!).ShouldBeTrue();

        if (expectedReturnTypeDefinition.IsGenericTypeDefinition)
        {
            handleMethod!.ReturnType.IsGenericType.ShouldBeTrue();
            handleMethod!.ReturnType.GetGenericTypeDefinition().ShouldBe(expectedReturnTypeDefinition);
            return;
        }

        handleMethod!.ReturnType.ShouldBe(expectedReturnTypeDefinition);
    }
}
