using NFramework.Mediator.Abstractions.Validation;
using NFramework.Mediator.Mediator.Railway.UnionRailway;
using UnionRailway;

namespace NFramework.Mediator.Mediator.Railway.UnionRailway.Tests;

public sealed class NFrameworkErrorMappingTests
{
    [Fact]
    public void ToUnionError_NullException_Throws()
    {
        Should.Throw<ArgumentNullException>(() => NFrameworkErrorMapping.ToUnionError(null!));
    }

    [Fact]
    public void ToUnionError_ValidationException_GroupsMessagesByProperty()
    {
        var exception = new ValidationException([
            new TestValidationError("NAME_REQUIRED", "Name is required.", "Name"),
            new TestValidationError("NAME_TOO_SHORT", "Name is too short.", "Name"),
            new TestValidationError("AGE_RANGE", "Age is out of range.", "Age"),
        ]);

        UnionError error = NFrameworkErrorMapping.ToUnionError(exception);

        error.TryGet(out UnionError.Validation validation).ShouldBeTrue();
        validation.Fields.Count.ShouldBe(2);
        validation.Fields["Name"].ShouldBe(["Name is required.", "Name is too short."]);
        validation.Fields["Age"].ShouldBe(["Age is out of range."]);
    }

    [Fact]
    public void ToUnionError_ValidationException_NullPropertyName_UsesUnscopedKey()
    {
        var exception = new ValidationException([new TestValidationError("GENERAL", "Something is wrong.", null)]);

        UnionError error = NFrameworkErrorMapping.ToUnionError(exception);

        error.TryGet(out UnionError.Validation validation).ShouldBeTrue();
        validation.Fields.ShouldContainKey(NFrameworkErrorMapping.UnscopedValidationKey);
        validation.Fields[NFrameworkErrorMapping.UnscopedValidationKey].ShouldBe(["Something is wrong."]);
    }

    [Fact]
    public void ToUnionError_UnauthorizedAccessException_MapsToUnauthorized()
    {
        UnionError error = NFrameworkErrorMapping.ToUnionError(new UnauthorizedAccessException("nope"));

        error.TryGet(out UnionError.Unauthorized _).ShouldBeTrue();
    }

    [Fact]
    public void ToUnionError_ConcurrencyConflictByName_MapsToConflict()
    {
        UnionError error = NFrameworkErrorMapping.ToUnionError(
            new ConcurrencyConflictException("Row version mismatch.")
        );

        error.TryGet(out UnionError.Conflict conflict).ShouldBeTrue();
        conflict.Reason.ShouldBe("Row version mismatch.");
    }

    [Fact]
    public void ToUnionError_UnknownException_MapsToSystemFailure()
    {
        var inner = new InvalidOperationException("boom");

        UnionError error = NFrameworkErrorMapping.ToUnionError(inner);

        error.TryGet(out UnionError.SystemFailure failure).ShouldBeTrue();
        failure.Ex.ShouldBeSameAs(inner);
    }

    [Theory]
    [InlineData(typeof(UnauthorizedAccessException), true)]
    [InlineData(typeof(InvalidOperationException), false)]
    public void IsMappedFrameworkError_ClassifiesException(Type exceptionType, bool expected)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        NFrameworkErrorMapping.IsMappedFrameworkError(exception).ShouldBe(expected);
    }

    [Fact]
    public void IsMappedFrameworkError_ValidationException_IsTrue()
    {
        var exception = new ValidationException([new TestValidationError("C", "m", "P")]);

        NFrameworkErrorMapping.IsMappedFrameworkError(exception).ShouldBeTrue();
    }

    [Fact]
    public void IsMappedFrameworkError_ConcurrencyConflict_IsTrue()
    {
        NFrameworkErrorMapping.IsMappedFrameworkError(new ConcurrencyConflictException("x")).ShouldBeTrue();
    }

    private sealed class TestValidationError(string code, string message, string? propertyName) : IValidationError
    {
        public string Code { get; } = code;
        public string Message { get; } = message;
        public string? PropertyName { get; } = propertyName;
    }

    /// <summary>
    /// Mirrors the persistence package's exception type by name only, so the mapping's
    /// name-based concurrency detection can be exercised without a persistence dependency.
    /// </summary>
    private sealed class ConcurrencyConflictException(string message) : Exception(message);
}
