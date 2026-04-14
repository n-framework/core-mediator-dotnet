using Mediator;
using Moq;
using NFramework.Mediator.Abstractions.Authorization;
using NFramework.Mediator.MartinothamarMediator.Authorization;

namespace NFramework.Mediator.Tests.Behaviors;

public sealed class AuthorizationBehaviorTests
{
    [Fact]
    public async Task Handle_SkipsAuthorization_WhenRequestDoesNotImplementISecuredRequest()
    {
        var securityContext = new Mock<ISecurityContext>();
        var behavior = new AuthorizationBehavior<UnsecuredRequest, string>(securityContext.Object);
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
        var securityContext = new Mock<ISecurityContext>();
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(securityContext.Object);
        var request = new SecuredRequest { RequiredRoles = [], RequiredOperations = [] };
        MessageHandlerDelegate<SecuredRequest, string> next = (_, _) => ValueTask.FromResult("success");

        string result = await behavior.Handle(request, next, default);

        result.ShouldBe("success");
        securityContext.Verify(s => s.IsAuthenticated, Times.Never);
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenNotAuthenticated()
    {
        var securityContext = new Mock<ISecurityContext>();
        _ = securityContext.Setup(s => s.IsAuthenticated).Returns(false);
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(securityContext.Object);
        var request = new SecuredRequest { RequiredRoles = ["Admin"] };
        MessageHandlerDelegate<SecuredRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            behavior.Handle(request, next, default).AsTask()
        );

        ex.Message.ShouldContain("not authenticated");
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenMissingRequiredRoles()
    {
        var securityContext = new Mock<ISecurityContext>();
        _ = securityContext.Setup(s => s.IsAuthenticated).Returns(true);
        _ = securityContext.Setup(s => s.HasAnyRole(It.IsAny<IReadOnlyList<string>>())).Returns(false);
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(securityContext.Object);
        var request = new SecuredRequest { RequiredRoles = ["Admin"] };
        MessageHandlerDelegate<SecuredRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            behavior.Handle(request, next, default).AsTask()
        );

        ex.Message.ShouldContain("required roles");
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenMissingRequiredOperations()
    {
        var securityContext = new Mock<ISecurityContext>();
        _ = securityContext.Setup(s => s.IsAuthenticated).Returns(true);
        _ = securityContext.Setup(s => s.HasAnyRole(It.IsAny<IReadOnlyList<string>>())).Returns(true);
        _ = securityContext.Setup(s => s.HasAllOperations(It.IsAny<IReadOnlyList<string>>())).Returns(false);
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(securityContext.Object);
        var request = new SecuredRequest { RequiredRoles = ["Admin"], RequiredOperations = ["product.delete"] };
        MessageHandlerDelegate<SecuredRequest, string> next = (_, _) => ValueTask.FromResult("should-not-reach");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            behavior.Handle(request, next, default).AsTask()
        );

        ex.Message.ShouldContain("required permissions");
    }

    [Fact]
    public async Task Handle_Continues_WhenAllChecksPass()
    {
        var securityContext = new Mock<ISecurityContext>();
        _ = securityContext.Setup(s => s.IsAuthenticated).Returns(true);
        _ = securityContext.Setup(s => s.HasAnyRole(It.IsAny<IReadOnlyList<string>>())).Returns(true);
        _ = securityContext.Setup(s => s.HasAllOperations(It.IsAny<IReadOnlyList<string>>())).Returns(true);
        var behavior = new AuthorizationBehavior<SecuredRequest, string>(securityContext.Object);
        var request = new SecuredRequest { RequiredRoles = ["Admin"], RequiredOperations = ["product.read"] };
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

    private sealed record UnsecuredRequest : IMessage;

    private sealed record SecuredRequest : IMessage, ISecuredRequest
    {
        public string[] RequiredRoles { get; init; } = [];
        public string[] RequiredOperations { get; init; } = [];
    }
}
