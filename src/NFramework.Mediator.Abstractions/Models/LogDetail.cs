namespace NFramework.Mediator.Abstractions.Models;

public class LogDetail
{
    public string RequestName { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public object? Parameters { get; set; }
}
