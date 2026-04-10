namespace NFramework.Mediator.Abstractions.Contracts.Pipeline;

public delegate ValueTask<TResult> RequestHandlerDelegate<TResult>(CancellationToken cancellationToken);
