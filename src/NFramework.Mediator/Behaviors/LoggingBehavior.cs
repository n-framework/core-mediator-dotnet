using System.Diagnostics;
using NFramework.Mediator.Configuration;

namespace NFramework.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that logs request lifecycle events.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : global::Mediator.IPipelineBehavior<TRequest, TResponse>
    where TRequest : global::Mediator.IMessage
{
    private readonly IRequestLogger<TRequest> _requestLogger;
    private readonly IRequestPipelinePolicyProvider _pipelinePolicyProvider;

    /// <summary>
    /// Creates a new logging behavior instance.
    /// </summary>
    /// <param name="requestLogger">The request logger.</param>
    /// <param name="pipelinePolicyProvider">The pipeline policy provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when requestLogger or pipelinePolicyProvider is null.</exception>
    public LoggingBehavior(
        IRequestLogger<TRequest> requestLogger,
        IRequestPipelinePolicyProvider pipelinePolicyProvider
    )
    {
        ArgumentNullException.ThrowIfNull(requestLogger);
        ArgumentNullException.ThrowIfNull(pipelinePolicyProvider);

        _requestLogger = requestLogger;
        _pipelinePolicyProvider = pipelinePolicyProvider;
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TRequest message,
        global::Mediator.MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var configuration = _pipelinePolicyProvider.GetConfiguration(typeof(TRequest));
        if (!configuration.UseLogging)
        {
            return await next(message, cancellationToken);
        }

        var stopwatch = Stopwatch.StartNew();
        _requestLogger.RequestStarted(message);

        try
        {
            var response = await next(message, cancellationToken);

            stopwatch.Stop();

            if (response is IShortCircuitResult shortCircuitResult && shortCircuitResult.IsShortCircuited)
            {
                _requestLogger.RequestShortCircuited(message, stopwatch.Elapsed);
                return response;
            }

            _requestLogger.RequestCompleted(message, stopwatch.Elapsed);
            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            _requestLogger.RequestFailed(message, exception, stopwatch.Elapsed);
            throw;
        }
    }
}
