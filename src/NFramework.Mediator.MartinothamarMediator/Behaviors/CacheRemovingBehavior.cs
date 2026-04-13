using Mediator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NFramework.Mediator.Abstractions.Behaviors;

namespace NFramework.Mediator.MartinothamarMediator.Behaviors;

public sealed class CacheRemovingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CacheRemovingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheRemoverRequest remover)
        {
            return await next(request, cancellationToken);
        }

        var response = await next(request, cancellationToken);

        if (remover.CacheKeyPatterns != null)
        {
            foreach (var pattern in remover.CacheKeyPatterns)
            {
                await cache.RemoveAsync(pattern, cancellationToken);
                logger.LogDebug("Cache removed for key pattern: {CacheKey}", pattern);
            }
        }

        return response;
    }
}
