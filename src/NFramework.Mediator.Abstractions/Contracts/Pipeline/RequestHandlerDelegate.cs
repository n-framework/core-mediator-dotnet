namespace NFramework.Mediator.Abstractions.Contracts.Pipeline;

/// <summary>
/// Delegate for invoking the next handler in the pipeline.
/// Used by <see cref="IPipelineBehavior{TRequest, TResponse}"/> to continue processing.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the handler.</typeparam>
public delegate ValueTask<TResult> RequestHandlerDelegate<TResult>(CancellationToken cancellationToken);
