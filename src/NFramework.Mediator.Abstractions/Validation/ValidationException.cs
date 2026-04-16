namespace NFramework.Mediator.Abstractions.Validation;

/// <summary>
/// Thrown when request validation fails, containing the detailed list of failures.
/// </summary>
public sealed class ValidationException : Exception
{
    public IReadOnlyList<IValidationError> Errors { get; }

    public ValidationException()
        : this([]) { }

    public ValidationException(string message)
        : this(message, []) { }

    public ValidationException(string message, IEnumerable<IValidationError> errors)
        : base(message)
    {
        var errorList = errors?.Where(e => e != null).ToList() ?? [];

        Errors = errorList.AsReadOnly();
    }

    public ValidationException(IEnumerable<IValidationError> errors)
        : base(BuildMessage(errors))
    {
        var errorList = errors?.Where(e => e != null).ToList() ?? throw new ArgumentNullException(nameof(errors));

        if (errorList.Count == 0)
        {
            throw new ArgumentException("Validation errors collection cannot be empty.", nameof(errors));
        }

        Errors = errorList.AsReadOnly();
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = [];
    }

    private static string BuildMessage(IEnumerable<IValidationError> errors)
    {
        if (errors == null)
            return "Validation failed.";
        List<string> messageParts = [.. errors.Where(e => e != null).Select(e => $"{e.Code}: {e.Message}")];
        return "Validation failed: " + (messageParts.Count > 0 ? string.Join("; ", messageParts) : "Unknown errors");
    }
}
