using Mediator;
using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Abstractions;
using NFramework.Mediator.Abstractions.Authorization;
using NFramework.Mediator.Tests.Railway;
using UnionRailway;

#pragma warning disable CS8601 // Test assertions guarantee non-null values after ShouldBeTrue()

namespace NFramework.Mediator.Tests.Authorization;

public sealed class AuthorizationBehaviorTests
{
    [Fact]
    public async Task Unauthenticated_ReturnsUnauthorizedRailFailure()
    {
        IMediator mediator = BuildMediator(out HandlerExecutionSpy spy, authenticated: false);

        Rail<int> result = await mediator.Send(new SecuredRailRequest());

        result.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        error!.Value.TryGet(out UnionError.Unauthorized _).ShouldBeTrue();
        spy.Executed.ShouldBeFalse();
    }

    [Fact]
    public async Task MissingRole_ReturnsForbiddenRailFailure()
    {
        IMediator mediator = BuildMediator(out HandlerExecutionSpy spy, authenticated: true, hasRole: false);

        Rail<int> result = await mediator.Send(new SecuredRailRequest());

        result.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        error!.Value.TryGet(out UnionError.Forbidden _).ShouldBeTrue();
        spy.Executed.ShouldBeFalse();
    }

    [Fact]
    public async Task CorrectRole_ExecutesHandlerAndReturnsSuccess()
    {
        IMediator mediator = BuildMediator(out HandlerExecutionSpy spy, authenticated: true, hasRole: true);

        Rail<int> result = await mediator.Send(new SecuredRailRequest());

        result.IsSuccess(out int value, out _).ShouldBeTrue();
        value.ShouldBe(42);
        spy.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task UnsecuredRequest_PassesThroughWithoutAuthCheck()
    {
        IMediator mediator = BuildMediator(out HandlerExecutionSpy spy, authenticated: false);

        Rail<int> result = await mediator.Send(new UnsecuredRailRequest());

        result.IsSuccess(out int value, out _).ShouldBeTrue();
        value.ShouldBe(42);
        spy.Executed.ShouldBeTrue();
    }

    private static IMediator BuildMediator(out HandlerExecutionSpy spy, bool authenticated, bool hasRole = true)
    {
        spy = new HandlerExecutionSpy();
        return RailwayTestHost
            .CreateServices(spy, authenticated, hasRole)
            .BuildServiceProvider()
            .GetRequiredService<IMediator>();
    }

    internal sealed record SecuredRailRequest : IRailRequest<int>, ISecuredRequest
    {
        public IReadOnlyList<string> RequiredRoles { get; } = ["Admin"];
        public IReadOnlyList<string> RequiredOperations { get; } = [];
    }

    internal sealed record UnsecuredRailRequest : IRailRequest<int>;

    internal sealed class SecuredHandler(HandlerExecutionSpy spy) : IRequestHandler<SecuredRailRequest, Rail<int>>
    {
        public ValueTask<Rail<int>> Handle(SecuredRailRequest request, CancellationToken cancellationToken)
        {
            spy.Executed = true;
            return ValueTask.FromResult<Rail<int>>(Union.Ok(42));
        }
    }

    internal sealed class UnsecuredHandler(HandlerExecutionSpy spy) : IRequestHandler<UnsecuredRailRequest, Rail<int>>
    {
        public ValueTask<Rail<int>> Handle(UnsecuredRailRequest request, CancellationToken cancellationToken)
        {
            spy.Executed = true;
            return ValueTask.FromResult<Rail<int>>(Union.Ok(42));
        }
    }
}
