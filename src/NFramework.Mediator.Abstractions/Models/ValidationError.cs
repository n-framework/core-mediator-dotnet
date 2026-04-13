namespace NFramework.Mediator.Abstractions.Models;

public class ValidationError
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
