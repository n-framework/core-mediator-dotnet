using Microsoft.AspNetCore.Http;
using UnionRailway;
using UnionRailway.AspNetCore;

namespace NFramework.Mediator.Mediator.Railway.UnionRailway;

/// <summary>
/// Translates NFramework framework exceptions thrown by the mediator pipeline (validation,
/// authorization, optimistic concurrency) into consistent RFC 7807 problem responses, so handlers
/// can return <c>Rail&lt;T&gt;</c> while cross-cutting behaviors keep signalling failures via exceptions.
/// Exceptions that are not recognized framework errors are re-thrown for the host's own handler.
/// </summary>
public sealed class RailwayExceptionTranslationMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    /// <summary>
    /// Invokes the next middleware and converts recognized framework exceptions into problem responses.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="errorMapper">Optional custom error mapper resolved from request services.</param>
    public async Task InvokeAsync(HttpContext context, IUnionErrorMapper? errorMapper = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception exception) when (NFrameworkErrorMapping.IsMappedFrameworkError(exception))
        {
            UnionError error = NFrameworkErrorMapping.ToUnionError(exception);
            IResult problem = error.ToHttpResult(errorMapper: errorMapper);
            await problem.ExecuteAsync(context).ConfigureAwait(false);
        }
    }
}
