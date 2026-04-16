namespace NFramework.Mediator.Mediator.Logging;

/// <summary>
/// Logger for request lifecycle events.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public interface IRequestLogger<TRequest>
{
    /// <summary>
    /// Called when a request begins processing.
    /// </summary>
    /// <param name="request">The request being processed.</param>
    void RequestStarted(TRequest request);

    /// <summary>
    /// Called when a request completes successfully.
    /// </summary>
    /// <param name="request">The request that completed.</param>
    /// <param name="elapsed">The elapsed processing time.</param>
    void RequestCompleted(TRequest request, TimeSpan elapsed);

    /// <summary>
    /// Called when a request fails with an exception.
    /// </summary>
    /// <param name="request">The request that failed.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="elapsed">The elapsed processing time.</param>
    void RequestFailed(TRequest request, Exception exception, TimeSpan elapsed);

    /// <summary>
    /// Called when a request is short-circuited (e.g., validation failure).
    /// </summary>
    /// <param name="request">The request that was short-circuited.</param>
    /// <param name="elapsed">The elapsed processing time.</param>
    void RequestShortCircuited(TRequest request, TimeSpan elapsed);
}

/// <summary>
/// Marker interface for responses that can indicate short-circuit behavior.
/// </summary>
public interface IShortCircuitResult
{
    /// <summary>
    /// Gets whether the request was short-circuited.
    /// </summary>
    bool IsShortCircuited { get; }
}
