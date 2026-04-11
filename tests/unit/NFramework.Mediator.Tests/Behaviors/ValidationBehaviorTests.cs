using FluentAssertions;
using NFramework.Mediator.Behaviors;
using NFramework.Mediator.Tests.TestDoubles;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldContinue_WhenValidationSucceeds()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [new PassThroughValidator()],
            new TestFailureResponseFactory(),
            new FakeRequestPipelinePolicyProvider()
        );

        var response = await behavior.Handle(
            new TestRequest("ok"),
            (_, _) => ValueTask.FromResult(new TestResponse(false, [])),
            default
        );

        _ = response.IsShortCircuited.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldShortCircuit_WhenValidationFails()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [new FailValidator("E001", "Name is required")],
            new TestFailureResponseFactory(),
            new FakeRequestPipelinePolicyProvider()
        );

        var response = await behavior.Handle(
            new TestRequest(string.Empty),
            (_, _) => throw new InvalidOperationException("Should not call next"),
            default
        );

        _ = response.IsShortCircuited.Should().BeTrue();
        _ = response.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldAggregateErrors_FromMultipleValidators()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [new FailValidator("E001", "first"), new FailValidator("E002", "second")],
            new TestFailureResponseFactory(),
            new FakeRequestPipelinePolicyProvider()
        );

        var response = await behavior.Handle(
            new TestRequest("bad"),
            (_, _) => throw new InvalidOperationException("Should not call next"),
            default
        );

        _ = response.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenValidationFailsAndNoFactoryRegistered()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [new FailValidator("E001", "error")],
            failureResponseFactory: null,
            new FakeRequestPipelinePolicyProvider()
        );

        var action = async () =>
            await behavior.Handle(
                new TestRequest("bad"),
                (_, _) => ValueTask.FromResult(new TestResponse(false, [])),
                default
            );

        _ = await action
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*Validation failed*no*IValidationFailureResponseFactory*");
    }

    [Fact]
    public async Task Handle_ShouldContinue_WhenNoValidatorsRegistered()
    {
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [],
            new TestFailureResponseFactory(),
            new FakeRequestPipelinePolicyProvider()
        );

        bool nextCalled = false;
        var response = await behavior.Handle(
            new TestRequest("test"),
            (_, _) =>
            {
                nextCalled = true;
                return ValueTask.FromResult(new TestResponse(false, []));
            },
            default
        );

        _ = nextCalled.Should().BeTrue();
        _ = response.IsShortCircuited.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldPropagateCancellationToken_ToHandler()
    {
        using var cts = new CancellationTokenSource();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            [new PassThroughValidator()],
            new TestFailureResponseFactory(),
            new FakeRequestPipelinePolicyProvider()
        );

        CancellationToken capturedToken = default;
        var response = await behavior.Handle(
            new TestRequest("test"),
            (_, ct) =>
            {
                capturedToken = ct;
                return ValueTask.FromResult(new TestResponse(false, []));
            },
            cts.Token
        );

        _ = capturedToken.Should().Be(cts.Token);
    }

    private sealed record TestRequest(string Name) : global::Mediator.IMessage;

    private sealed record TestResponse(bool IsShortCircuited, IReadOnlyCollection<IValidationError> Errors)
        : IShortCircuitResult;

    private sealed class PassThroughValidator : IRequestValidator<TestRequest>
    {
        public ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(
            TestRequest request,
            CancellationToken cancellationToken
        )
        {
            return ValueTask.FromResult((IReadOnlyCollection<IValidationError>)[]);
        }
    }

    private sealed class FailValidator(string code, string message) : IRequestValidator<TestRequest>
    {
        public ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(
            TestRequest request,
            CancellationToken cancellationToken
        )
        {
            return ValueTask.FromResult(
                (IReadOnlyCollection<IValidationError>)[new TestValidationError(code, message)]
            );
        }
    }

    private sealed class TestFailureResponseFactory : IValidationFailureResponseFactory<TestResponse>
    {
        public TestResponse Create(IReadOnlyCollection<IValidationError> errors)
        {
            return new TestResponse(true, errors);
        }
    }

    private sealed record TestValidationError(string Code, string Message) : IValidationError;
}
