namespace NFramework.Mediator.Abstractions.Contracts.Pipeline;

public interface IPipelineBehavior<TRequest, TResponse>
{
    ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    );
}
