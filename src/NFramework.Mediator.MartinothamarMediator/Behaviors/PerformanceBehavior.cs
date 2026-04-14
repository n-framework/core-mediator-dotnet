using System.Diagnostics;
using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Behaviors;

namespace NFramework.Mediator.MartinothamarMediator.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private readonly Stopwatch _timer = new Stopwatch();

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (request is not IIntervalRequest)
        {
            return await next(request, cancellationToken);
        }

        _timer.Start();
        var response = await next(request, cancellationToken);
        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var requestName = typeof(TRequest).Name;
            logger.LogWarning(
                "Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds)",
                requestName,
                elapsedMilliseconds
            );
        }

        return response;
    }
}
