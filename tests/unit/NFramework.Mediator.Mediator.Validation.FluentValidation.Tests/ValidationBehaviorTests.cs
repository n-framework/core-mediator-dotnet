using FluentValidation;
using FluentValidation.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using NFramework.Mediator.Abstractions.Validation;

namespace NFramework.Mediator.Mediator.Validation.FluentValidation.Tests;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsValidation_WhenRequestDoesNotImplementIValidatable()
    {
        Mock<ILogger<ValidationBehavior<NonValidatableRequest, string>>> logger =
            new Mock<ILogger<ValidationBehavior<NonValidatableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        ValidationBehavior<NonValidatableRequest, string> behavior = new ValidationBehavior<
            NonValidatableRequest,
            string
        >([], logger.Object);
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
        Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>> logger =
            new Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        ValidationBehavior<ValidatableRequest, string> behavior = new ValidationBehavior<ValidatableRequest, string>(
            [],
            logger.Object
        );
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
    public async Task Handle_ThrowsValidationException_WhenValidationFails()
    {
        Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>> logger =
            new Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        InlineValidator<ValidatableRequest> validator = new InlineValidator<ValidatableRequest>();
        _ = validator.RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");

        ValidationBehavior<ValidatableRequest, string> behavior = new ValidationBehavior<ValidatableRequest, string>(
            new[] { (global::FluentValidation.IValidator<ValidatableRequest>)validator },
            logger.Object
        );
        MessageHandlerDelegate<ValidatableRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<Abstractions.Validation.ValidationException>(() =>
            behavior.Handle(new ValidatableRequest { Name = "" }, next, default).AsTask()
        );

        ex.Message.ShouldContain("Validation failed");
        ex.Errors.ShouldNotBeEmpty();
        ex.Errors[0].Message.ShouldBe("Name is required");

        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Validation failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_Continues_WhenValidationPasses()
    {
        Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>> logger =
            new Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        InlineValidator<ValidatableRequest> validator = new InlineValidator<ValidatableRequest>();
        _ = validator.RuleFor(x => x.Name).NotEmpty();

        ValidationBehavior<ValidatableRequest, string> behavior = new ValidationBehavior<ValidatableRequest, string>(
            new[] { (global::FluentValidation.IValidator<ValidatableRequest>)validator },
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

    [Fact]
    public async Task Handle_AggregatesErrors_FromMultipleValidators()
    {
        Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>> logger =
            new Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        InlineValidator<ValidatableRequest> validator1 = new InlineValidator<ValidatableRequest>();
        _ = validator1.RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");

        InlineValidator<ValidatableRequest> validator2 = new InlineValidator<ValidatableRequest>();
        _ = validator2.RuleFor(x => x.Name).MinimumLength(10).WithMessage("Name too short");

        ValidationBehavior<ValidatableRequest, string> behavior = new ValidationBehavior<ValidatableRequest, string>(
            new[] { validator1, validator2 },
            logger.Object
        );
        MessageHandlerDelegate<ValidatableRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<Abstractions.Validation.ValidationException>(() =>
            behavior.Handle(new ValidatableRequest { Name = "" }, next, default).AsTask()
        );

        ex.Errors.Count.ShouldBe(2);
        ex.Errors.Any(e => e.Message == "Name is required").ShouldBeTrue();
        ex.Errors.Any(e => e.Message == "Name too short").ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_FiltersNullFailures()
    {
        Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>> logger =
            new Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        // Mock a validator that returns a collection containing null
        Mock<global::FluentValidation.IValidator<ValidatableRequest>> validator =
            new Mock<global::FluentValidation.IValidator<ValidatableRequest>>();
        ValidationResult validationResult = new global::FluentValidation.Results.ValidationResult(
            new[] { (global::FluentValidation.Results.ValidationFailure)null! }
        );

        _ = validator
            .As<IValidator>()
            .Setup(x => x.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        ValidationBehavior<ValidatableRequest, string> behavior = new ValidationBehavior<ValidatableRequest, string>(
            new[] { validator.Object },
            logger.Object
        );

        bool handlerInvoked = false;
        MessageHandlerDelegate<ValidatableRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        _ = await behavior.Handle(new ValidatableRequest { Name = "test" }, next, default);

        handlerInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_UsesCustomExceptionFactory_WhenProvided()
    {
        Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>> logger =
            new Mock<ILogger<ValidationBehavior<ValidatableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        Mock<IValidationExceptionFactory> factory = new Mock<IValidationExceptionFactory>();
        Exception customEx = new Exception("Custom validation error");
        _ = factory
            .Setup(x => x.CreateValidationException(It.IsAny<IEnumerable<IValidationError>>()))
            .Returns(customEx);

        InlineValidator<ValidatableRequest> validator = new InlineValidator<ValidatableRequest>();
        _ = validator.RuleFor(x => x.Name).NotEmpty();

        ValidationBehavior<ValidatableRequest, string> behavior = new ValidationBehavior<ValidatableRequest, string>(
            new[] { (global::FluentValidation.IValidator<ValidatableRequest>)validator },
            logger.Object,
            factory.Object
        );

        MessageHandlerDelegate<ValidatableRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            behavior.Handle(new ValidatableRequest { Name = "" }, next, default).AsTask()
        );

        ex.Message.ShouldBe("Custom validation error");
        factory.Verify(x => x.CreateValidationException(It.IsAny<IEnumerable<IValidationError>>()), Times.Once);
    }

    internal sealed record NonValidatableRequest : IMessage;

    internal sealed record ValidatableRequest : IMessage, IValidatableRequest
    {
        public string Name { get; init; } = string.Empty;
    }
}
