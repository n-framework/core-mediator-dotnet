using Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using NFramework.Mediator.Abstractions.Authorization;
using NFramework.Mediator.Mediator.Authorization;

namespace NFramework.Mediator.Tests.Authorization;

public sealed class AuthorizationBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsAuthorization_WhenRequestDoesNotImplementISecuredRequest()
    {
        Mock<ISecurityContext> securityContext = new Mock<ISecurityContext>();
        Mock<ILogger<AuthorizationBehavior<UnsecuredRequest, string>>> logger =
            new Mock<ILogger<AuthorizationBehavior<UnsecuredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        AuthorizationBehavior<UnsecuredRequest, string> behavior = new AuthorizationBehavior<UnsecuredRequest, string>(
            securityContext.Object,
            logger.Object
        );
        bool handlerInvoked = false;
        MessageHandlerDelegate<UnsecuredRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("success");
        };

        string result = await behavior.Handle(new UnsecuredRequest(), next, default);

        result.ShouldBe("success");
        handlerInvoked.ShouldBeTrue();
        securityContext.Verify(s => s.IsAuthenticated, Times.Never);
    }

    [Fact]
    public async Task Handle_Continues_WhenNoRolesOrOperationsRequired()
    {
        Mock<ISecurityContext> securityContext = new Mock<ISecurityContext>();
        Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>> logger =
            new Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        AuthorizationBehavior<SecuredRequest, string> behavior = new AuthorizationBehavior<SecuredRequest, string>(
            securityContext.Object,
            logger.Object
        );
        SecuredRequest request = new SecuredRequest
        {
            RequiredRoles = Array.Empty<string>(),
            RequiredOperations = Array.Empty<string>(),
        };
        MessageHandlerDelegate<SecuredRequest, string> next = (_, _) => ValueTask.FromResult("success");

        string result = await behavior.Handle(request, next, default);

        result.ShouldBe("success");
        securityContext.Verify(s => s.IsAuthenticated, Times.Never);
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenNotAuthenticated()
    {
        Mock<ISecurityContext> securityContext = new Mock<ISecurityContext>();
        _ = securityContext.Setup(s => s.IsAuthenticated).Returns(false);
        Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>> logger =
            new Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        AuthorizationBehavior<SecuredRequest, string> behavior = new AuthorizationBehavior<SecuredRequest, string>(
            securityContext.Object,
            logger.Object
        );
        SecuredRequest request = new SecuredRequest { RequiredRoles = new List<string> { "Admin" } };
        MessageHandlerDelegate<SecuredRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            behavior.Handle(request, next, default).AsTask()
        );

        ex.Message.ShouldContain("not authenticated");

        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("is not authenticated")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenMissingRequiredRoles()
    {
        Mock<ISecurityContext> securityContext = new Mock<ISecurityContext>();
        _ = securityContext.Setup(s => s.IsAuthenticated).Returns(true);
        _ = securityContext.Setup(s => s.HasAnyRole(It.IsAny<IReadOnlyList<string>>())).Returns(false);
        Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>> logger =
            new Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        AuthorizationBehavior<SecuredRequest, string> behavior = new AuthorizationBehavior<SecuredRequest, string>(
            securityContext.Object,
            logger.Object
        );
        SecuredRequest request = new SecuredRequest { RequiredRoles = new List<string> { "Admin" } };
        MessageHandlerDelegate<SecuredRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            behavior.Handle(request, next, default).AsTask()
        );

        ex.Message.ShouldContain("required roles");

        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("lacks required roles")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenMissingRequiredOperations()
    {
        Mock<ISecurityContext> securityContext = new Mock<ISecurityContext>();
        _ = securityContext.Setup(s => s.IsAuthenticated).Returns(true);
        _ = securityContext.Setup(s => s.HasAnyRole(It.IsAny<IReadOnlyList<string>>())).Returns(true);
        _ = securityContext.Setup(s => s.HasAllOperations(It.IsAny<IReadOnlyList<string>>())).Returns(false);
        Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>> logger =
            new Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        AuthorizationBehavior<SecuredRequest, string> behavior = new AuthorizationBehavior<SecuredRequest, string>(
            securityContext.Object,
            logger.Object
        );
        SecuredRequest request = new SecuredRequest
        {
            RequiredRoles = new List<string> { "Admin" },
            RequiredOperations = new List<string> { "product.delete" },
        };
        MessageHandlerDelegate<SecuredRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            behavior.Handle(request, next, default).AsTask()
        );

        ex.Message.ShouldContain("required permissions");

        logger.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("lacks required permissions")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_Continues_WhenAllChecksPass()
    {
        Mock<ISecurityContext> securityContext = new Mock<ISecurityContext>();
        _ = securityContext.Setup(s => s.IsAuthenticated).Returns(true);
        _ = securityContext.Setup(s => s.HasAnyRole(It.IsAny<IReadOnlyList<string>>())).Returns(true);
        _ = securityContext.Setup(s => s.HasAllOperations(It.IsAny<IReadOnlyList<string>>())).Returns(true);
        Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>> logger =
            new Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        AuthorizationBehavior<SecuredRequest, string> behavior = new AuthorizationBehavior<SecuredRequest, string>(
            securityContext.Object,
            logger.Object
        );
        SecuredRequest request = new SecuredRequest
        {
            RequiredRoles = new List<string> { "Admin" },
            RequiredOperations = new List<string> { "product.read" },
        };
        bool handlerInvoked = false;
        MessageHandlerDelegate<SecuredRequest, string> next = (_, _) =>
        {
            handlerInvoked = true;
            return ValueTask.FromResult("authorized");
        };

        string result = await behavior.Handle(request, next, default);

        result.ShouldBe("authorized");
        handlerInvoked.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_RespectsCancellationToken()
    {
        using var ctSource = new CancellationTokenSource();
        var ct = ctSource.Token;

        Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>> logger =
            new Mock<ILogger<AuthorizationBehavior<SecuredRequest, string>>>();
        _ = logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        Mock<ISecurityContext> securityContext = new Mock<ISecurityContext>();
        _ = securityContext.Setup(u => u.IsAuthenticated).Returns(true);
        securityContext
            .Setup(u => u.HasAnyRole(It.Is<IReadOnlyList<string>>(r => r.Contains("Admin"))))
            .Returns(true)
            .Verifiable();

        AuthorizationBehavior<SecuredRequest, string> behavior = new AuthorizationBehavior<SecuredRequest, string>(
            securityContext.Object,
            logger.Object
        );
        MessageHandlerDelegate<SecuredRequest, string> next = (_, t) =>
        {
            t.ShouldBe(ct);
            return ValueTask.FromResult("authorized");
        };

        string result = await behavior.Handle(new SecuredRequest { RequiredRoles = AdminRole }, next, ct);

        result.ShouldBe("authorized");
        securityContext.Verify();
    }

    private static readonly string[] AdminRole = ["Admin"];

    internal sealed record UnsecuredRequest : IMessage;

    internal sealed record SecuredRequest : IMessage, ISecuredRequest
    {
        public IReadOnlyList<string> RequiredRoles { get; init; } = Array.Empty<string>();
        public IReadOnlyList<string> RequiredOperations { get; init; } = Array.Empty<string>();
    }
}
