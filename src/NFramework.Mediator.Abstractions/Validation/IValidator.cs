using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NFramework.Mediator.Abstractions.Validation;

/// <summary>
/// Provides an abstraction over specific validation libraries (FluentValidation, DataAnnotations, etc.).
/// </summary>
public interface IValidator<in T>
{
    ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(T instance, CancellationToken cancellationToken);
}

public interface IValidationError
{
    string Code { get; }

    string Message { get; }

    string? PropertyName { get; }
}

public interface IValidationExceptionFactory
{
    Exception CreateValidationException(IEnumerable<IValidationError> errors);
}
