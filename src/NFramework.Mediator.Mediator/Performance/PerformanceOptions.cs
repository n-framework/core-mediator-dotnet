namespace NFramework.Mediator.Mediator.Performance;

/// <summary>
/// Configuration options for NFramework performance behavior.
/// </summary>
public class PerformanceOptions
{
    /// <summary>
    /// Request duration threshold before triggering a performance warning. Default is 500ms.
    /// </summary>
    public TimeSpan PerformanceThreshold { get; set; } = TimeSpan.FromMilliseconds(500);
}
