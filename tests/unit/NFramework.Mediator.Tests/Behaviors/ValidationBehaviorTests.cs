using FluentValidation;
using Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using NFramework.Mediator.Abstractions.Validation;
using NFramework.Mediator.MartinothamarMediator.Validation;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsValidation_WhenRequestDoesNotImplementIValidatable()
    {
        var logger = new Mock<ILogger<ValidationBehavior<NonValidatableRequest, string>>>();
        var behavior = new ValidationBehavior<NonValidatableRequest, string>([], logger.Object);
        bool handlerInvoked = false;
        MessageHandlerDelegate<NonValidatableRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new NonValidatableRequest(), next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_Continues_WhenNoValidatorsRegistered()
    {
        var logger = new Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>>();
        var behavior = new ValidationBehavior<ValidatableRequest, string>([], logger.Object);
        bool handlerInvoked = false;
        MessageHandlerDelegate<ValidatableRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new ValidatableRequest { Name = "test" }, next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ThrowsInvalidOperationException_WhenValidationFails()
    {
        var logger = new Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>>();
        var validator = new InlineValidator<ValidatableRequest>();
        _ = validator.RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");

        var behavior = new ValidationBehavior<ValidatableRequest, string>(
            new[] { (FluentValidation.IValidator<ValidatableRequest>)validator },
            logger.Object
        );
        MessageHandlerDelegate<ValidatableRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(new ValidatableRequest { Name = "" }, next, default).AsTask()
        );

        ex.Message.ShouldContain("Validation failed");
    }

    [Fact]
    public async Task Handle_Continues_WhenValidationPasses()
    {
        var logger = new Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>>();
        var validator = new InlineValidator<ValidatableRequest>();
        _ = validator.RuleFor(x => x.Name).NotEmpty();

        var behavior = new ValidationBehavior<ValidatableRequest, string>(
            new[] { (FluentValidation.IValidator<ValidatableRequest>)validator },
            logger.Object
        );
        bool handlerInvoked = false;
        MessageHandlerDelegate<ValidatableRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new ValidatableRequest { Name = "valid" }, next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
    }

    public sealed record NonValidatableRequest : IMessage;

    public sealed record ValidatableRequest : IMessage, IValidatableRequest
    {
        public string Name { get; init; } = string.Empty;
    }
}
