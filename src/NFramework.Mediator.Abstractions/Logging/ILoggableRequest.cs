namespace NFramework.Mediator.Abstractions.Logging;

/// <summary>
/// Only requests implementing this interface will be logged by the pipeline.
/// </summary>
public interface ILoggableRequest
{
    LogOptions LogOptions { get; }
}
