using System.Diagnostics;
using NFramework.Mediator.Configuration;

namespace NFramework.Mediator.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : global::Mediator.IPipelineBehavior<TRequest, TResponse>
    where TRequest : global::Mediator.IMessage
{
    private readonly IRequestLogger<TRequest> _requestLogger;
    private readonly IRequestPipelinePolicyProvider? _pipelinePolicyProvider;

    public LoggingBehavior(
        IRequestLogger<TRequest> requestLogger,
        IRequestPipelinePolicyProvider? pipelinePolicyProvider = null
    )
    {
        _requestLogger = requestLogger;
        _pipelinePolicyProvider = pipelinePolicyProvider;
    }

    public async ValueTask<TResponse> Handle(
        TRequest message,
        global::Mediator.MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (_pipelinePolicyProvider is not null)
        {
            var configuration = _pipelinePolicyProvider.GetConfiguration(typeof(TRequest));
            if (!configuration.UseLogging)
            {
                return await next(message, cancellationToken);
            }
        }

        _requestLogger.RequestStarted(message);

        var stopwatch = Stopwatch.StartNew();

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
