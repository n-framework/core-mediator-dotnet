using Mediator;
using Microsoft.Extensions.DependencyInjection;
using NFramework.Mediator.Abstractions.Authorization;
using NFramework.Mediator.Abstractions.Validation;
using NFramework.Mediator.Mediator.Railway;
using NFramework.Mediator.Mediator.Transactions;
using UnionRailway;
using NFramework.Mediator.Abstractions;
using NFramework.Mediator.Tests.Railway;

namespace NFramework.Mediator.Tests.Validation;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task InvalidRequest_ShortCircuitsWithRailValidationFailure()
    {
        IMediator mediator = BuildMediator(valid: false, out HandlerExecutionSpy spy);

        Rail<int> result = await mediator.Send(new CreateThing("bad"));

        result.IsSuccess(out _, out UnionError? error).ShouldBeFalse();
        error!.Value.TryGet(out UnionError.Validation validation).ShouldBeTrue();
        validation.Fields.ShouldContainKey("Name");
        spy.Executed.ShouldBeFalse();
    }

    [Fact]
    public async Task ValidRequest_ExecutesHandlerAndReturnsSuccess()
    {
        IMediator mediator = BuildMediator(valid: true, out HandlerExecutionSpy spy);

        Rail<int> result = await mediator.Send(new CreateThing("good"));

        result.IsSuccess(out int value, out _).ShouldBeTrue();
        value.ShouldBe(42);
        spy.Executed.ShouldBeTrue();
    }

    [Fact]
    public async Task NoValidators_PassesThroughToHandler()
    {
        IMediator mediator = BuildMediator(valid: true, out _, registerValidator: false);

        Rail<int> result = await mediator.Send(new CreateThing("anything"));

        result.IsSuccess(out int value, out _).ShouldBeTrue();
        value.ShouldBe(42);
    }

    private static IMediator BuildMediator(bool valid, out HandlerExecutionSpy spy, bool registerValidator = true)
    {
        spy = new HandlerExecutionSpy();
        IServiceCollection services = RailwayTestHost.CreateServices(spy);
        if (registerValidator)
        {
            services.AddSingleton<IValidator<CreateThing>>(new CreateThingValidator(valid));
        }

        return services.BuildServiceProvider().GetRequiredService<IMediator>();
    }

    public sealed record CreateThing(string Name) : IRailRequest<int>;

    public sealed class CreateThingHandler(HandlerExecutionSpy spy) : IRequestHandler<CreateThing, Rail<int>>
    {
        public ValueTask<Rail<int>> Handle(CreateThing request, CancellationToken cancellationToken)
        {
            spy.Executed = true;
            return ValueTask.FromResult<Rail<int>>(Union.Ok(42));
        }
    }

    private sealed class CreateThingValidator(bool valid) : IValidator<CreateThing>
    {
        public ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(
            CreateThing instance,
            CancellationToken cancellationToken
        )
        {
            IReadOnlyCollection<IValidationError> errors = valid
                ? []
                : [new ValidationError("NAME_INVALID", "Name is invalid.", "Name")];
            return ValueTask.FromResult(errors);
        }
    }

    private sealed class ValidationError(string code, string message, string? propertyName) : IValidationError
    {
        public string Code { get; } = code;
        public string Message { get; } = message;
        public string? PropertyName { get; } = propertyName;
    }
}
