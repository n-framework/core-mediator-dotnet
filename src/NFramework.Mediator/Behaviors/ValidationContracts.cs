namespace NFramework.Mediator.Behaviors;

public interface IValidationError
{
    string Code { get; }

    string Message { get; }
}

public interface IRequestValidator<TRequest>
{
    ValueTask<IReadOnlyCollection<IValidationError>> ValidateAsync(
        TRequest request,
        CancellationToken cancellationToken
    );
}

public interface IValidationFailureResponseFactory<TResponse>
{
    TResponse Create(IReadOnlyCollection<IValidationError> errors);
}
