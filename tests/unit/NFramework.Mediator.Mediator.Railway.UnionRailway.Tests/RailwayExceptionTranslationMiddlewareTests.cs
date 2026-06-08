using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Abstractions.Validation;
using NFramework.Mediator.Mediator.Railway.UnionRailway;

namespace NFramework.Mediator.Mediator.Railway.UnionRailway.Tests;

public sealed class RailwayExceptionTranslationMiddlewareTests
{
    [Fact]
    public async Task ValidationException_ProducesProblemResponse()
    {
        HttpContext context = await InvokeWithAsync(
            new ValidationException([new TestValidationError("NAME_REQUIRED", "Name is required.", "Name")])
        );

        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        string body = await ReadBodyAsync(context);
        body.ShouldContain("Name");
    }

    [Fact]
    public async Task UnauthorizedAccessException_ProducesUnauthorizedResponse()
    {
        HttpContext context = await InvokeWithAsync(new UnauthorizedAccessException());

        context.Response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task ConcurrencyConflict_ProducesConflictResponse()
    {
        HttpContext context = await InvokeWithAsync(new ConcurrencyConflictException("conflict"));

        context.Response.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task UnknownException_IsRethrown()
    {
        var thrown = new InvalidOperationException("unexpected");

        await Should.ThrowAsync<InvalidOperationException>(() => InvokeWithAsync(thrown).AsTask());
    }

    [Fact]
    public async Task SuccessfulRequest_IsLeftUntouched()
    {
        var context = new DefaultHttpContext();
        var middleware = new RailwayExceptionTranslationMiddleware(ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    private static async ValueTask<HttpContext> InvokeWithAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider();

        var middleware = new RailwayExceptionTranslationMiddleware(_ => throw exception);
        await middleware.InvokeAsync(context);
        return context;
    }

    private static async Task<string> ReadBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private sealed class TestValidationError(string code, string message, string? propertyName) : IValidationError
    {
        public string Code { get; } = code;
        public string Message { get; } = message;
        public string? PropertyName { get; } = propertyName;
    }

    private sealed class ConcurrencyConflictException(string message) : Exception(message);
}
