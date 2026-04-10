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
        Assert.True(typeof(ICommand<int>).IsInterface);
        Assert.True(typeof(IQuery<int>).IsInterface);
        Assert.True(typeof(IStreamQuery<int>).IsInterface);
        Assert.True(typeof(IEvent).IsInterface);
    }

    [Fact]
    public void RequiredHandlerContracts_ArePublicInterfaces()
    {
        Assert.True(typeof(ICommandHandler<ICommand<int>, int>).IsInterface);
        Assert.True(typeof(IQueryHandler<IQuery<int>, int>).IsInterface);
        Assert.True(typeof(IStreamQueryHandler<IStreamQuery<int>, int>).IsInterface);
        Assert.True(typeof(IEventHandler<IEvent>).IsInterface);
    }

    [Fact]
    public void AsyncContracts_UseCancellationTokenAndValueTask()
    {
        AssertHandleSignature(typeof(ICommandHandler<,>), typeof(ValueTask<>));
        AssertHandleSignature(typeof(IQueryHandler<,>), typeof(ValueTask<>));
        AssertHandleSignature(typeof(IStreamQueryHandler<,>), typeof(IAsyncEnumerable<>));
        AssertHandleSignature(typeof(IEventHandler<>), typeof(ValueTask));

        var mediatorMethods = typeof(IMediator).GetMethods();
        Assert.All(
            mediatorMethods,
            method =>
            {
                var tokenParameter = method
                    .GetParameters()
                    .FirstOrDefault(parameter => parameter.ParameterType == typeof(CancellationToken));
                Assert.NotNull(tokenParameter);
            }
        );
    }

    [Fact]
    public void PipelineBehavior_UsesNextDelegate()
    {
        var behaviorType = typeof(IPipelineBehavior<,>);
        Assert.True(ContractShapeInspector.HasHandleMethod(behaviorType));

        var handleMethod = behaviorType.GetMethod("Handle");
        Assert.NotNull(handleMethod);
        Assert.True(ContractShapeInspector.HasCancellationTokenParameter(handleMethod!));

        var nextParameter = handleMethod!.GetParameters()[1].ParameterType;
        Assert.Equal(typeof(RequestHandlerDelegate<>), nextParameter.GetGenericTypeDefinition());
    }

    private static void AssertHandleSignature(Type handlerInterface, Type expectedReturnTypeDefinition)
    {
        var handleMethod = handlerInterface.GetMethod("Handle");
        Assert.NotNull(handleMethod);
        Assert.True(ContractShapeInspector.HasCancellationTokenParameter(handleMethod!));

        if (expectedReturnTypeDefinition.IsGenericTypeDefinition)
        {
            Assert.Equal(expectedReturnTypeDefinition, handleMethod!.ReturnType.GetGenericTypeDefinition());
            return;
        }

        Assert.Equal(expectedReturnTypeDefinition, handleMethod!.ReturnType);
    }
}
