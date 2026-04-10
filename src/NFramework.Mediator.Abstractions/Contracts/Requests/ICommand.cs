namespace NFramework.Mediator.Abstractions.Contracts.Requests;

/// <summary>
/// Marker interface for commands that produce a result of type <typeparamref name="TResult"/>.
/// Commands represent intent to perform an action and should only be executed once.
/// </summary>
/// <typeparam name="TResult">The type of result produced by executing the command.</typeparam>
public interface ICommand<TResult>;
