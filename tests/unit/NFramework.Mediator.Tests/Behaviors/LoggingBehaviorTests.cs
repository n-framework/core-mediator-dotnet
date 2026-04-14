using Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using NFramework.Mediator.Abstractions.Logging;
using NFramework.Mediator.MartinothamarMediator.Logging;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsLogging_WhenRequestDoesNotImplementILoggable()
    {
        var logger = new Mock<ILogger<LoggingBehavior<NonLoggableRequest, string>>>();
        var behavior = new LoggingBehavior<NonLoggableRequest, string>(logger.Object);
        bool handlerInvoked = false;
        MessageHandlerDelegate<NonLoggableRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new NonLoggableRequest(), next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_LogsRequestStart_WhenRequestIsLoggable()
    {
        var logger = new Mock<ILogger<LoggingBehavior<LoggableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var behavior = new LoggingBehavior<LoggableRequest, string>(logger.Object);
        MessageHandlerDelegate<LoggableRequest, string> next = (_, _) => ValueTask.FromResult("success");

        string result = await behavior.Handle(new LoggableRequest { Name = "test" }, next, default);

        result.ShouldBe("success");
        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Handling")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task Handle_LogsResponse_WhenLogResponseIsEnabled()
    {
        var logger = new Mock<ILogger<LoggingBehavior<LoggableWithResponseRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var behavior = new LoggingBehavior<LoggableWithResponseRequest, string>(logger.Object);
        MessageHandlerDelegate<LoggableWithResponseRequest, string> next = (_, _) =>
            ValueTask.FromResult("the-response");

        _ = await behavior.Handle(new LoggableWithResponseRequest { Name = "test" }, next, default);

        // Should log both "Handling" and "Handled"
        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task Handle_ExcludesParameter_WhenMarkedForExclusion()
    {
        var logger = new Mock<ILogger<LoggingBehavior<LoggableWithExclusionRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var behavior = new LoggingBehavior<LoggableWithExclusionRequest, string>(logger.Object);
        MessageHandlerDelegate<LoggableWithExclusionRequest, string> next = (_, _) => ValueTask.FromResult("success");

        _ = await behavior.Handle(
            new LoggableWithExclusionRequest { Name = "test", Password = "secret123" },
            next,
            default
        );

        // The log should NOT include the password value
        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => !v.ToString()!.Contains("secret123")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    public sealed record NonLoggableRequest : IMessage;

    public sealed record LoggableRequest : IMessage, ILoggableRequest
    {
        public string Name { get; init; } = string.Empty;
        public LogOptions LogOptions => LogOptions.Default;
    }

    public sealed record LoggableWithResponseRequest : IMessage, ILoggableRequest
    {
        public string Name { get; init; } = string.Empty;
        public LogOptions LogOptions => new(logResponse: true);
    }

    public sealed record LoggableWithExclusionRequest : IMessage, ILoggableRequest
    {
        public string Name { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;

        public LogOptions LogOptions => new(new LogExcludeParameter("Password"));
    }
}
