using NFramework.Mediator.Abstractions.Contracts.Handlers;
using NFramework.Mediator.Abstractions.Contracts.Requests;

namespace NFramework.Mediator.Abstractions.Tests.Discovery.Fixtures;

public static class InvalidHandlerFixtures
{
    public sealed record MissingContractCommand(int Quantity) : ICommand<int>;

    public sealed class MissingInterfaceCommandHandler
    {
        public ValueTask<int> Handle(MissingContractCommand command, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(command.Quantity);
        }
    }

    public sealed record OpenGenericCommand<T>(T Value) : ICommand<T>;

    public sealed class OpenGenericCommandHandler<T> : ICommandHandler<OpenGenericCommand<T>, T>
    {
        public ValueTask<T> Handle(OpenGenericCommand<T> command, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(command.Value)!;
        }
    }

    public sealed record InvalidEvent(string Message) : IEvent;

    public sealed class NonContractEventHandler
    {
        public ValueTask Handle(InvalidEvent @event, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }
}
