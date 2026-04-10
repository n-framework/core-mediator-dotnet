namespace NFramework.Mediator.Abstractions.Contracts.Requests;

/// <summary>
/// Marker interface for stream queries that return an async sequence of <typeparamref name="TResult"/>.
/// Stream queries are used for scenarios requiring pagination or real-time data streaming.
/// </summary>
/// <typeparam name="TResult">The type of items in the streamed sequence.</typeparam>
public interface IStreamQuery<TResult>;
