using FluentAssertions;
using NFramework.Mediator.Behaviors;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class ShortCircuitTests
{
    [Fact]
    public async Task ValidationBehavior_ShouldShortCircuit_AndSkipNext()
    {
        bool called = false;

        var behavior = new ValidationBehavior<TestRequest, TestResponse>([new FailValidator()], new FailureFactory());

        var response = await behavior.Handle(
            new TestRequest(),
            (_, _) =>
            {
                called = true;
                return ValueTask.FromResult(new TestResponse(false));
            },
            default
        );

        _ = response.IsShortCircuited.Should().BeTrue();
        _ = called.Should().BeFalse();
    }

    [Fact]
    public async Task LoggingBehavior_ShouldCaptureShortCircuitEvent()
    {
        var logger = new TestDoubles.FakeRequestLogger<TestRequest>();
        var behavior = new LoggingBehavior<TestRequest, TestResponse>(logger);

        _ = await behavior.Handle(new TestRequest(), (_, _) => ValueTask.FromResult(new TestResponse(true)), default);

        _ = logger.ShortCircuitCallCount.Should().Be(1);
    }

    private sealed record TestRequest : global::Mediator.IMessage;

    private sealed record TestResponse(bool IsShortCircuited) : IShortCircuitResult;

    private sealed class FailValidator : IRequestValidator<TestRequest>
    {
        public ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(
            TestRequest request,
            CancellationToken cancellationToken
        )
        {
            return ValueTask.FromResult(
                (IReadOnlyCollection<IValidationError>)[new ValidationError("E001", "Invalid")]
            );
        }
    }

    private sealed class FailureFactory : IValidationFailureResponseFactory<TestResponse>
    {
        public TestResponse Create(IReadOnlyCollection<IValidationError> errors)
        {
            return new TestResponse(true);
        }
    }

    private sealed record ValidationError(string Code, string Message) : IValidationError;
}
