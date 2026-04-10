namespace NFramework.Mediator.Abstractions.Contracts.Requests;

/// <summary>
/// Marker interface for queries that return a result of type <typeparamref name="TResult"/>.
/// Queries represent read operations and should be idempotent and side-effect free.
/// </summary>
/// <typeparam name="TResult">The type of result returned by the query.</typeparam>
public interface IQuery<TResult>;
