using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Performance;

namespace NFramework.Mediator.MartinothamarMediator.Performance;

/// <summary>
/// Martinothamar Mediator implementation for execution time monitoring.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : PerformanceBehaviorBase<TRequest, TResponse>(logger),
        IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        return await HandleAsync(request, ct => next(request, ct), cancellationToken);
    }
}
