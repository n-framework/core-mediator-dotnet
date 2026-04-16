using System.Reflection;
using Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using NFramework.Mediator.Abstractions.Logging;
using NFramework.Mediator.Mediator.Logging;

namespace NFramework.Mediator.Tests.Logging;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsLogging_WhenRequestDoesNotImplementILoggable()
    {
        Mock<ILogger<LoggingBehavior<NonLoggableRequest, string>>> logger =
            new Mock<ILogger<LoggingBehavior<NonLoggableRequest, string>>>();
        LoggingBehavior<NonLoggableRequest, string> behavior = new LoggingBehavior<NonLoggableRequest, string>(
            logger.Object
        );
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
        Mock<ILogger<LoggingBehavior<LoggableRequest, string>>> logger =
            new Mock<ILogger<LoggingBehavior<LoggableRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        LoggingBehavior<LoggableRequest, string> behavior = new LoggingBehavior<LoggableRequest, string>(logger.Object);
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
        Mock<ILogger<LoggingBehavior<LoggableWithResponseRequest, string>>> logger =
            new Mock<ILogger<LoggingBehavior<LoggableWithResponseRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        LoggingBehavior<LoggableWithResponseRequest, string> behavior = new LoggingBehavior<
            LoggingBehaviorTests.LoggableWithResponseRequest,
            string
        >(logger.Object);
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
        Mock<ILogger<LoggingBehavior<LoggableWithExclusionRequest, string>>> logger =
            new Mock<ILogger<LoggingBehavior<LoggableWithExclusionRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        LoggingBehavior<LoggableWithExclusionRequest, string> behavior = new LoggingBehavior<
            LoggableWithExclusionRequest,
            string
        >(logger.Object);
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

    [Fact]
    public async Task Handle_UsesFallback_WhenParameterExtractionFails()
    {
        Mock<ILogger<LoggingBehavior<LoggableWithFaultyPropertyRequest, string>>> logger =
            new Mock<ILogger<LoggingBehavior<LoggableWithFaultyPropertyRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        LoggingBehavior<LoggableWithFaultyPropertyRequest, string> behavior = new LoggingBehavior<
            LoggableWithFaultyPropertyRequest,
            string
        >(logger.Object);
        MessageHandlerDelegate<LoggableWithFaultyPropertyRequest, string> next = (_, _) =>
            ValueTask.FromResult("success");

        _ = await behavior.Handle(new LoggableWithFaultyPropertyRequest(), next, default);

        // Should log "Handling" with the fallback error message
        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, _) =>
                            v.ToString()!.Contains("_fallbackError")
                            && v.ToString()!.Contains("Failed to extract parameters")
                    ),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    internal sealed record NonLoggableRequest : IMessage;

    internal sealed record LoggableRequest : IMessage, ILoggableRequest
    {
        public string Name { get; init; } = string.Empty;
        public LogOptions LogOptions => LogOptions.Default;
    }

    internal sealed record LoggableWithResponseRequest : IMessage, ILoggableRequest
    {
        public string Name { get; init; } = string.Empty;
        public LogOptions LogOptions => new(logResponse: true);
    }

    internal sealed record LoggableWithExclusionRequest : IMessage, ILoggableRequest
    {
        public string Name { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;

        public LogOptions LogOptions => new(excludeParameters: [new LogExcludeParameter("Password")]);
    }

    internal sealed record LoggableWithFaultyPropertyRequest : IMessage, ILoggableRequest
    {
        public string FaultyProperty => throw new AmbiguousMatchException("Simulated reflection error");
        public LogOptions LogOptions => LogOptions.Default;
    }
}
